using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace UnzipAndCheckPolicy;

public class UnzipAndCheck
{
    private readonly ILogger<UnzipAndCheck> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".zip", ".rar"
    };
    public UnzipAndCheck(
        ILogger<UnzipAndCheck> logger,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    // Blob: uploads/{examSubjectId}/{examinerId}/{moderatorId}/{name}.zip
    [Function(nameof(UnzipAndCheck))]
    public async Task Run(
        [BlobTrigger("uploads/{examSubjectId}/{examinerId}/{moderatorId}/{name}", 
            Connection = "AzureWebJobsStorage")]
        Stream zipStream,
        string examSubjectId,
        string examinerId,
        string moderatorId,
        string name,
        FunctionContext context)
    {
        _logger.LogInformation(
            "Triggered for blob: uploads/{ExamSubjectId}/{ExaminerId}/{ModeratorId}/{Name}",
            examSubjectId, examinerId, moderatorId, name);
        //Validate file extension
        var extension = Path.GetExtension(name);
        if (!SupportedExtensions.Contains(extension))
        {
            _logger.LogWarning("Unsupported file type: {Name}. Supported types: .zip, .rar", name);
            return;
        }
        //Parse examSubjectId
        if (!Guid.TryParse(examSubjectId, out var examSubjectGuid))
        {
            _logger.LogError("Invalid examSubjectId: {Value}", examSubjectId);
            return;
        }
        
        //Parse examinerId
        if (!Guid.TryParse(examinerId, out var examinerGuid))
        {
            _logger.LogError("Invalid examinerId: {Value}", examinerId);
            return;
        }
        
        //Parse moderatorId
        if (!Guid.TryParse(moderatorId, out var moderatorGuid))
        {
            _logger.LogError("Invalid examinerId: {Value}", moderatorId);
            return;
        }
        
        var ct = context.CancellationToken;

        try
        {
            // 3. Copy stream to memory to create form file
            await using var mem = new MemoryStream();
            await zipStream.CopyToAsync(mem, ct);
            mem.Position = 0;

            await SendToApiAsync(mem, examSubjectGuid, examinerGuid, moderatorGuid, name, extension, ct);
            _logger.LogInformation("Successfully processed blob {Name} for ExamSubject {ExamSubjectId}", name, examSubjectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to process blob {Name} for ExamSubject {ExamSubjectId}", 
                name, examSubjectId);
            throw;
        }
    }
    private async Task SendToApiAsync(
        MemoryStream archiveStream,
        Guid examSubjectId,
        Guid examinerId,
        Guid moderatorId,
        string fileName,
        string extension,
        CancellationToken ct)
    {
        var httpClient = _httpClientFactory.CreateClient("ExamAPI");
        
        using var formData = new MultipartFormDataContent();
        
        // Add file content
        var fileContent = new StreamContent(archiveStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(
            extension.ToLowerInvariant() == ".zip" 
                ? "application/zip" 
                : "application/x-rar-compressed");
        formData.Add(fileContent, "ArchiveFile", fileName);
        
        // Add examSubjectId
        formData.Add(new StringContent(examSubjectId.ToString()), "ExamSubjectId");
        
        // Add examinerId
        formData.Add(new StringContent(examinerId.ToString()), "ExaminerId");
        
        // Add moderatorId
        formData.Add(new StringContent(moderatorId.ToString()), "ModeratorId");
        
        // Call API endpoint
        var response = await httpClient.PostAsync("/api/v1/submissions/process-from-blob", formData, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError(
                "API call failed with status {StatusCode}: {Error}",
                response.StatusCode, errorContent);
            throw new HttpRequestException(
                $"API returned {response.StatusCode}: {errorContent}");
        }
    }
}
