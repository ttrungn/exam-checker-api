using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace UnzipAndCheckPolicy;

public class UnzipAndCheck
{
    private readonly ILogger<UnzipAndCheck> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public UnzipAndCheck(
        ILogger<UnzipAndCheck> logger,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    // Blob: uploads/{examSubjectId}/{examinerId}/{name}.zip
    [Function(nameof(UnzipAndCheck))]
    public async Task Run(
        [BlobTrigger("uploads/{examSubjectId}/{examinerId}/{name}", 
            Connection = "AzureWebJobsStorage")]
        Stream zipStream,
        string examSubjectId,
        string examinerId,
        string name,
        FunctionContext context)
    {
        _logger.LogInformation(
            "Triggered for blob: uploads/{ExamSubjectId}/{ExaminerId}/{Name}",
            examSubjectId, examinerId, name);

        // 1. Parse examSubjectId
        if (!Guid.TryParse(examSubjectId, out var examSubjectGuid))
        {
            _logger.LogError("Invalid examSubjectId: {Value}", examSubjectId);
            return;
        }
        
        // 2. Parse examinerId
        if (!Guid.TryParse(examinerId, out var examinerGuid))
        {
            _logger.LogError("Invalid examinerId: {Value}", examinerId);
            return;
        }
        
        var ct = context.CancellationToken;

        try
        {
            // 3. Copy stream to memory to create form file
            await using var mem = new MemoryStream();
            await zipStream.CopyToAsync(mem, ct);
            mem.Position = 0;

            // 4. Create HTTP client and prepare multipart form data
            var httpClient = _httpClientFactory.CreateClient("ExamAPI");
            
            using var formData = new MultipartFormDataContent();
            
            // Add file content
            var fileContent = new StreamContent(mem);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
            formData.Add(fileContent, "ZipFile", name);
            
            // Add examSubjectId
            formData.Add(new StringContent(examSubjectGuid.ToString()), "ExamSubjectId");
            
            // Add examinerId
            formData.Add(new StringContent(examinerGuid.ToString()), "ExaminerId");

            // 5. Call API endpoint for processing blob
            var response = await httpClient.PostAsync("/api/v1/submissions/process-from-blob", formData, ct);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Successfully processed blob {Name} for ExamSubject {ExamSubjectId}",
                    name, examSubjectId);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError(
                    "API call failed with status {StatusCode}: {Error}",
                    response.StatusCode, errorContent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to process blob {Name} for ExamSubject {ExamSubjectId}", 
                name, examSubjectId);
            throw;
        }
    }
}
