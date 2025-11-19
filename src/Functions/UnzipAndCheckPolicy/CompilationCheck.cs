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

        _logger.LogInformation("Processing compilation check for SubmissionId: {SubmissionId}, BlobUrl: {BlobUrl}",
            data.SubmissionId, data.BlobUrl);

        _logger.LogInformation("Processing compilation check for SubmissionId: {SubmissionId}, BlobUrl: {BlobUrl}",
            data.SubmissionId, data.BlobUrl);

        var uri = new Uri(data.BlobUrl);
        var container = uri.Segments[1].Trim('/');
        var blobPath = string.Join("", uri.Segments.Skip(2));

        _logger.LogInformation("Parsed blob location - Container: {Container}, BlobPath: {BlobPath}", container, blobPath);

        // Download blob manually
        _logger.LogInformation("Downloading blob from container: {Container}, path: {BlobPath}", container, blobPath);
        var containerClient = _blobServiceClient.GetBlobContainerClient(container);
        var blobClient = containerClient.GetBlobClient(blobPath);
        
        var stream = new MemoryStream();
        await blobClient.DownloadToAsync(stream);
        stream.Position = 0;
        _logger.LogInformation("Blob downloaded successfully. Size: {Size} bytes", stream.Length);
        
        _logger.LogInformation("Opening outer ZIP archive");
        using var outerZip = new ZipArchive(stream, ZipArchiveMode.Read);
        _logger.LogInformation("Outer ZIP contains {Count} entries", outerZip.Entries.Count);

        var innerZipEntry = outerZip.Entries
        .FirstOrDefault(e => e.FullName.EndsWith("solution.zip",
                        StringComparison.OrdinalIgnoreCase));


        if (innerZipEntry == null)
        {
            _logger.LogWarning("solution.zip not found in submission for SubmissionId: {SubmissionId}", data.SubmissionId);
            await ReportCompilationResultAsync(data.SubmissionId, false,
                "solution.zip not found in submission structure.");
            return;
        }
        
        _logger.LogInformation("Found solution.zip at path: {Path}", innerZipEntry.FullName);
        using var innerMem = new MemoryStream();
        using (var innerStream = innerZipEntry.Open())
        {
            innerStream.CopyTo(innerMem);
        }
        innerMem.Position = 0;
        _logger.LogInformation("solution.zip extracted. Size: {Size} bytes", innerMem.Length);
        
        _logger.LogInformation("Opening solution.zip");
        using var solutionZip = new ZipArchive(innerMem, ZipArchiveMode.Read);
        _logger.LogInformation("solution.zip contains {Count} entries", solutionZip.Entries.Count);

        _logger.LogInformation("solution.zip contains {Count} entries", solutionZip.Entries.Count);

        // Use shorter path to avoid Windows MAX_PATH (260 chars) limitation
        var workspaceRoot = Path.Combine(Path.GetTempPath(), "ec"); // "ec" = exam checker
        if (!Directory.Exists(workspaceRoot))
        {
            Directory.CreateDirectory(workspaceRoot);
            _logger.LogInformation("Created workspace root: {WorkspaceRoot}", workspaceRoot);
        }

        var workspace = Path.Combine(workspaceRoot, data.SubmissionId.ToString("N").Substring(0, 8));
        if (Directory.Exists(workspace))
        {
            _logger.LogInformation("Cleaning up existing workspace: {Workspace}", workspace);
            Directory.Delete(workspace, true);
        }
        Directory.CreateDirectory(workspace);
        _logger.LogInformation("Created workspace directory: {Workspace}", workspace);

        _logger.LogInformation("Extracting solution.zip to workspace");
        solutionZip.ExtractToDirectory(workspace);
        _logger.LogInformation("solution.zip extracted to {Workspace}", workspace);

        _logger.LogInformation("Searching for .sln file in {Workspace}", workspace);
        var slnFile = Directory.GetFiles(workspace, "*.sln", SearchOption.AllDirectories)
                       .FirstOrDefault();

        if (slnFile == null)
        {
            _logger.LogWarning("No .sln file found in workspace for SubmissionId: {SubmissionId}", data.SubmissionId);
            await ReportCompilationResultAsync(data.SubmissionId, false,
                "No .sln file found inside solution.zip.");
            return;
        }
        
        _logger.LogInformation("Found solution file: {SlnFile}", slnFile);
        _logger.LogInformation("Starting dotnet build for {SlnFile}", slnFile);
        var buildResult = await RunDotnetBuildAsync(slnFile);

        if (!buildResult.Success)
        {
            _logger.LogWarning("Build failed for SubmissionId: {SubmissionId}. Error: {Error}",
                data.SubmissionId, buildResult.ErrorMessage);
            await ReportCompilationResultAsync(data.SubmissionId, false,
                buildResult.ErrorMessage);
            return;
        }

        _logger.LogInformation("Build succeeded for SubmissionId: {SubmissionId}", data.SubmissionId);
        await ReportCompilationResultAsync(data.SubmissionId, true,
            "Build succeeded – project compiled successfully.");

        // Cleanup workspace
        _logger.LogInformation("Cleaning up workspace: {Workspace}", workspace);
        try 
        { 
            Directory.Delete(workspace, true);
            _logger.LogInformation("Workspace cleaned up successfully");
        }
        catch
        {
        }
}

    private async Task<(bool Success, string ErrorMessage)> RunDotnetBuildAsync(string slnPath)
    {
        var workingDir = Path.GetDirectoryName(slnPath)!;
        _logger.LogInformation("RunDotnetBuildAsync started - SlnPath: {SlnPath}, WorkingDir: {WorkingDir}", slnPath, workingDir);

        // First, run restore with timeout
        _logger.LogInformation("Starting dotnet restore for {SlnPath}", slnPath);
        
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

        _logger.LogInformation("Starting restore process - Command: dotnet {Arguments}", restorePsi.Arguments);
        using var restoreProc = System.Diagnostics.Process.Start(restorePsi)!;
        _logger.LogInformation("Restore process started with PID: {ProcessId}", restoreProc.Id);
        
        // Create cancellation token with timeout
        using var restoreCts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        
        try
        {
            await restoreProc.WaitForExitAsync(restoreCts.Token);
            _logger.LogInformation("Restore process completed with exit code: {ExitCode}", restoreProc.ExitCode);
        }
        catch (OperationCanceledException)
        {
            restoreProc.Kill(true);
            _logger.LogError("Restore timed out after 2 minutes. Process killed.");
            return (false, "NuGet restore timed out after 2 minutes.");
        }

        if (restoreProc.ExitCode != 0)
        {
            var restoreError = await restoreProc.StandardError.ReadToEndAsync();
            var restoreOutput = await restoreProc.StandardOutput.ReadToEndAsync();
            _logger.LogError("Restore failed with exit code {ExitCode}.\nStdErr: {Error}\nStdOut: {Output}",
                restoreProc.ExitCode, restoreError, restoreOutput);
            return (false, "Restore failed:\n" + restoreError);
        }

        var restoreSuccessOutput = await restoreProc.StandardOutput.ReadToEndAsync();
        _logger.LogInformation("Restore succeeded. Output: {Output}", restoreSuccessOutput);

        _logger.LogInformation("Starting dotnet build for {SlnPath}", slnPath);

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

        _logger.LogInformation("Starting build process - Command: dotnet {Arguments}", buildPsi.Arguments);
        using var buildProc = System.Diagnostics.Process.Start(buildPsi)!;
        _logger.LogInformation("Build process started with PID: {ProcessId}", buildProc.Id);
        
        // Create cancellation token with timeout
        using var buildCts = new CancellationTokenSource(TimeSpan.FromMinutes(3));
        
        try
        {
            await buildProc.WaitForExitAsync(buildCts.Token);
            _logger.LogInformation("Build process completed with exit code: {ExitCode}", buildProc.ExitCode);
        }
        catch (OperationCanceledException)
        {
            buildProc.Kill(true);
            _logger.LogError("Build timed out after 3 minutes. Process killed.");
            return (false, "Build process timed out after 3 minutes. Project may have infinite loops or blocking code.");
        }

        var output = await buildProc.StandardOutput.ReadToEndAsync();
        var error = await buildProc.StandardError.ReadToEndAsync();

        if (buildProc.ExitCode != 0)
        {
            _logger.LogError("Build failed with exit code {ExitCode}.\nStdOut: {Output}\nStdErr: {Error}",
                buildProc.ExitCode, output, error);
            return (false, "Build failed.\n" + error);
        }

        _logger.LogInformation("Build succeeded. Output: {Output}", output);
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
                ViolationType = ViolationPolicy.CompilationError.ToString(),
                Description = message
            });
        }

        var payload = new
        {
            SubmissionId = submissionId,
            Violations = violations
        };

        var apiUrl = _configuration["ExamAPI:BaseUrl"] ?? "http://localhost:5248";
        var endpoint = $"{apiUrl}/api/v1/violation/save";

        _logger.LogInformation("Sending violation report to {Endpoint} with payload: {@Payload}", endpoint, payload);

        using var httpClient = _httpClientFactory.CreateClient();
        var json = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );

        var response = await httpClient.PostAsync(endpoint, json);
        
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Compilation result saved for {SubmissionId}. Response: {Response}", submissionId, responseContent);
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to save compilation result for {SubmissionId}. Status: {Status}, Error: {Error}", 
                submissionId, response.StatusCode, errorContent);
        }
    }

}
