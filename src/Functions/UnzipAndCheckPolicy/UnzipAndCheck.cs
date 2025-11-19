using System.Net.Http.Headers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace UnzipAndCheckPolicy;

public class UnzipAndCheck
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".zip", ".rar"
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<UnzipAndCheck> _logger;

    public UnzipAndCheck(
        ILogger<UnzipAndCheck> logger,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    // Blob: uploads/{examSubjectId}/{examinerId}/{moderatorId}/{name}.zip
    [Function(nameof(UnzipAndCheck))]
    [SignalROutput(
        HubName = "examcheckernotificationshub", // must match your backend hub
        ConnectionStringSetting = "Azure:SignalRSettings:PrimaryConnectionString"
    )]
    public async Task<SignalRMessageAction?> Run(
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
            "Blob trigger activated - Path: uploads/{ExamSubjectId}/{ExaminerId}/{ModeratorId}/{Name}, Size: {Size} bytes",
            examSubjectId, examinerId, moderatorId, name, zipStream.Length);

        //Validate file extension
        var extension = Path.GetExtension(name);
        _logger.LogInformation("File extension detected: {Extension}", extension);

        if (!SupportedExtensions.Contains(extension))
        {
            _logger.LogWarning(
                "Unsupported file type: {Name} with extension {Extension}. Supported types: .zip, .rar",
                name, extension);
            return null;
        }

        _logger.LogInformation("File extension validation passed for {Name}", name);

        _logger.LogInformation("File extension validation passed for {Name}", name);

        //Parse examSubjectId
        _logger.LogInformation("Parsing examSubjectId: {ExamSubjectId}", examSubjectId);
        if (!Guid.TryParse(examSubjectId, out var examSubjectGuid))
        {
            _logger.LogError("Invalid examSubjectId format: {Value}", examSubjectId);
            return null;
        }

        _logger.LogInformation("examSubjectId parsed successfully: {ExamSubjectGuid}", examSubjectGuid);

        //Parse examinerId
        _logger.LogInformation("Parsing examinerId: {ExaminerId}", examinerId);
        if (!Guid.TryParse(examinerId, out var examinerGuid))
        {
            _logger.LogError("Invalid examinerId format: {Value}", examinerId);
            return null;
        }

        _logger.LogInformation("examinerId parsed successfully: {ExaminerGuid}", examinerGuid);

        //Parse moderatorId
        _logger.LogInformation("Parsing moderatorId: {ModeratorId}", moderatorId);
        if (!Guid.TryParse(moderatorId, out var moderatorGuid))
        {
            _logger.LogError("Invalid moderatorId format: {Value}", moderatorId);
            return null;
        }

        _logger.LogInformation("moderatorId parsed successfully: {ModeratorGuid}", moderatorGuid);

        var ct = context.CancellationToken;

        try
        {
            _logger.LogInformation("Starting blob processing for {Name}", name);

            // 3. Copy stream to memory to create form file
            _logger.LogInformation("Copying blob stream to memory for {Name}", name);
            await using var mem = new MemoryStream();
            await zipStream.CopyToAsync(mem, ct);
            mem.Position = 0;
            _logger.LogInformation("Blob stream copied successfully. Memory stream size: {Size} bytes", mem.Length);

            _logger.LogInformation(
                "Calling Exam API with ExamSubjectId: {ExamSubjectId}, ExaminerId: {ExaminerId}, ModeratorId: {ModeratorId}, FileName: {FileName}",
                examSubjectGuid, examinerGuid, moderatorGuid, name);
            await SendToApiAsync(mem, examSubjectGuid, examinerGuid, moderatorGuid, name, extension, ct);
            _logger.LogInformation("Successfully processed blob {Name} for ExamSubject {ExamSubjectId}", name,
                examSubjectId);

            _logger.LogInformation(
                "Preparing SignalR notification for user {ExaminerId} with method 'SubmissionUploaded'",
                examinerId);
            
            var signalRMessage = new SignalRMessageAction("SubmissionUploaded")
            {
                UserId = examinerId,
                Arguments =
                [
                    new { userId = examinerId }
                ]
            };
            
            _logger.LogInformation(
                "Returning SignalR message action - Method: {Method}, UserId: {UserId}, Arguments: {@Arguments}. Azure Functions will now send this to SignalR service.",
                signalRMessage.Target, signalRMessage.UserId, signalRMessage.Arguments);
            
            return signalRMessage;
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
        _logger.LogInformation(
            "SendToApiAsync started - ExamSubjectId: {ExamSubjectId}, ExaminerId: {ExaminerId}, ModeratorId: {ModeratorId}, FileName: {FileName}, Extension: {Extension}, StreamSize: {Size}",
            examSubjectId, examinerId, moderatorId, fileName, extension, archiveStream.Length);

        var httpClient = _httpClientFactory.CreateClient("ExamAPI");
        _logger.LogInformation("HttpClient created with base address: {BaseAddress}", httpClient.BaseAddress);

        using var formData = new MultipartFormDataContent();
        _logger.LogInformation("Creating multipart form data content");

        // Add file content
        var fileContent = new StreamContent(archiveStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(
            extension.ToLowerInvariant() == ".zip"
                ? "application/zip"
                : "application/x-rar-compressed");
        formData.Add(fileContent, "ArchiveFile", fileName);
        _logger.LogInformation("Added file content - Name: {FileName}, ContentType: {ContentType}",
            fileName, fileContent.Headers.ContentType);

        // Add examSubjectId
        formData.Add(new StringContent(examSubjectId.ToString()), "ExamSubjectId");
        _logger.LogInformation("Added ExamSubjectId: {ExamSubjectId}", examSubjectId);

        // Add examinerId
        formData.Add(new StringContent(examinerId.ToString()), "ExaminerId");
        _logger.LogInformation("Added ExaminerId: {ExaminerId}", examinerId);

        // Add moderatorId
        formData.Add(new StringContent(moderatorId.ToString()), "ModeratorId");
        _logger.LogInformation("Added ModeratorId: {ModeratorId}", moderatorId);

        // Call API endpoint
        _logger.LogInformation("Sending POST request to /api/v1/submissions/process-from-blob");
        var response = await httpClient.PostAsync("/api/v1/submissions/process-from-blob", formData, ct);
        _logger.LogInformation(
            "Received response - StatusCode: {StatusCode}, ReasonPhrase: {ReasonPhrase}",
            response.StatusCode, response.ReasonPhrase);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError(
                "API call failed with status {StatusCode}: {Error}",
                response.StatusCode, errorContent);
            throw new HttpRequestException(
                $"API returned {response.StatusCode}: {errorContent}");
        }

        var successContent = await response.Content.ReadAsStringAsync(ct);
        _logger.LogInformation(
            "API call succeeded - StatusCode: {StatusCode}, Response: {Response}",
            response.StatusCode, successContent);
    }
}
