using System;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Queues.Models;
using Domain.Enums;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UnzipAndCheckPolicy.Models;

namespace UnzipAndCheckPolicy;

public class CompilationCheck
{
    private readonly ILogger<CompilationCheck> _logger;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public CompilationCheck(
        ILogger<CompilationCheck> logger,
        BlobServiceClient blobServiceClient,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _blobServiceClient = blobServiceClient;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [Function(nameof(CompilationCheck))]
    public async Task RunAsync([QueueTrigger("compilation-check-items", Connection = "AzureWebJobsStorage")] QueueMessage message)
    {
        _logger.LogInformation($"Queue message received: {message}");

        // Safest decode approach (local or cloud)
        var payload = Encoding.UTF8.GetString(message.Body);

        // Deserialize your queue payload
        var data = JsonSerializer.Deserialize<CompilationCheckQueueMessage>(payload);
        if (data == null)
        {
            _logger.LogError("Invalid queue message JSON: {Payload}", payload);
            return;
        }

        var uri = new Uri(data.BlobUrl); 
        var container = uri.Segments[1].Trim('/');
        var blobPath = string.Join("", uri.Segments.Skip(2));

        // Download blob manually
        var containerClient = _blobServiceClient.GetBlobContainerClient(container);
        var blobClient = containerClient.GetBlobClient(blobPath);
        
        var stream = new MemoryStream();
        await blobClient.DownloadToAsync(stream);
        stream.Position = 0;
        
        using var outerZip = new ZipArchive(stream, ZipArchiveMode.Read);

        var innerZipEntry = outerZip.Entries
        .FirstOrDefault(e => e.FullName.EndsWith("solution.zip",
                        StringComparison.OrdinalIgnoreCase));


        if (innerZipEntry == null)
        {
            await ReportCompilationResultAsync(data.SubmissionId, false,
                "solution.zip not found in submission structure.");
            return;
        }
        using var innerMem = new MemoryStream();
        using (var innerStream = innerZipEntry.Open())
        {
            innerStream.CopyTo(innerMem);
        }
        innerMem.Position = 0;
        using var solutionZip = new ZipArchive(innerMem, ZipArchiveMode.Read);

        // Use shorter path to avoid Windows MAX_PATH (260 chars) limitation
        var workspaceRoot = Path.Combine(Path.GetTempPath(), "ec"); // "ec" = exam checker
        if (!Directory.Exists(workspaceRoot))
            Directory.CreateDirectory(workspaceRoot);

        var workspace = Path.Combine(workspaceRoot, data.SubmissionId.ToString("N").Substring(0, 8));
        if (Directory.Exists(workspace)) Directory.Delete(workspace, true);
        Directory.CreateDirectory(workspace);

        solutionZip.ExtractToDirectory(workspace);

        _logger.LogInformation("solution.zip extracted to {Workspace}", workspace);

        var slnFile = Directory.GetFiles(workspace, "*.sln", SearchOption.AllDirectories)
                       .FirstOrDefault();

        if (slnFile == null)
        {
            await ReportCompilationResultAsync(data.SubmissionId, false,
                "No .sln file found inside solution.zip.");
            return;
        }
        var buildResult = await RunDotnetBuildAsync(slnFile);

        if (!buildResult.Success)
        {
            await ReportCompilationResultAsync(data.SubmissionId, false,
                buildResult.ErrorMessage);
            return;
        }

        await ReportCompilationResultAsync(data.SubmissionId, true,
            "Build succeeded � project compiled successfully.");
        _logger.LogInformation("Build succeeded for {SubmissionId}", data.SubmissionId);

        // Cleanup workspace
        try { Directory.Delete(workspace, true); }
        catch
        {
        }
}

    private async Task<(bool Success, string ErrorMessage)> RunDotnetBuildAsync(string slnPath)
    {
        var workingDir = Path.GetDirectoryName(slnPath)!;

        // First, run restore with timeout
        _logger.LogInformation("Running dotnet restore for {SlnPath}", slnPath);
        
        var restorePsi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"restore \"{slnPath}\" --packages \"{Path.Combine(workingDir, "packages")}\"",
            WorkingDirectory = workingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var restoreProc = System.Diagnostics.Process.Start(restorePsi)!;
        
        // Create cancellation token with timeout
        using var restoreCts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        
        try
        {
            await restoreProc.WaitForExitAsync(restoreCts.Token);
        }
        catch (OperationCanceledException)
        {
            restoreProc.Kill(true);
            _logger.LogError("Restore timed out after 2 minutes");
            return (false, "NuGet restore timed out after 2 minutes.");
        }

        if (restoreProc.ExitCode != 0)
        {
            var restoreError = await restoreProc.StandardError.ReadToEndAsync();
            _logger.LogError("Restore failed:\n{Error}", restoreError);
            return (false, "NuGet restore failed.\n" + restoreError);
        }

        _logger.LogInformation("Running dotnet build for {SlnPath}", slnPath);

        var buildPsi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"build \"{slnPath}\" --no-restore --configuration Release",
            WorkingDirectory = workingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var buildProc = System.Diagnostics.Process.Start(buildPsi)!;
        
        // Create cancellation token with timeout
        using var buildCts = new CancellationTokenSource(TimeSpan.FromMinutes(3));
        
        try
        {
            await buildProc.WaitForExitAsync(buildCts.Token);
        }
        catch (OperationCanceledException)
        {
            buildProc.Kill(true);
            _logger.LogError("Build timed out after 3 minutes");
            return (false, "Build process timed out after 3 minutes. Project may have infinite loops or blocking code.");
        }

        var output = await buildProc.StandardOutput.ReadToEndAsync();
        var error = await buildProc.StandardError.ReadToEndAsync();

        if (buildProc.ExitCode != 0)
        {
            _logger.LogError("Build failed:\n{Output}\n{Error}", output, error);
            return (false, "Build failed.\n" + error);
        }

        _logger.LogInformation("Build succeeded");
        return (true, output);
    }

    private async Task ReportCompilationResultAsync(Guid submissionId, bool success, string message)
    {
        _logger.LogInformation("Reporting compilation result for {SubmissionId}: Success={Success}", submissionId, success);

        var violations = new List<object>();

        if (!success)
        {
            violations.Add(new
            {
                ViolationType = (int)ViolationPolicy.CompilationError,
                Description = message
            });
        }

        var payload = new
        {
            SubmissionId = submissionId,
            Violations = violations
        };

        var apiUrl = _configuration["ExamAPI:BaseUrl"] ?? "http://localhost:5248";
        var endpoint = $"{apiUrl}/api/v1/violations/save";

        using var httpClient = _httpClientFactory.CreateClient();
        var json = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );

        var response = await httpClient.PostAsync(endpoint, json);
        
        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Compilation result saved for {SubmissionId}", submissionId);
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to save compilation result for {SubmissionId}. Status: {Status}, Error: {Error}", 
                submissionId, response.StatusCode, errorContent);
        }
    }

}
