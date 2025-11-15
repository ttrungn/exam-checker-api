using Exam.Services.Features.Submission.Commands.CreateSubmissionsFromZipCommand;
using Exam.Services.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace UnzipAndCheckPolicy;

public class UnzipAndCheck
{
    private readonly ILogger<UnzipAndCheck> _logger;
    private readonly ISubmissionService _submissionService;

    public UnzipAndCheck(
        ILogger<UnzipAndCheck> logger,
        ISubmissionService submissionService)
    {
        _logger = logger;
        _submissionService = submissionService;
    }

    // Blob: uploads/{examSubjectId}/{name}.zip
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
            "Triggered for blob: uploads/{ExamSubjectId}/{examinerId}/{Name}",
            examSubjectId, examinerId, name);

        // 1. Parse examSubjectId
        if (!Guid.TryParse(examSubjectId, out var examSubjectGuid))
        {
            _logger.LogError("Invalid examSubjectId: {Value}", examSubjectId);
            return;
        }
        // 1. Parse examinerId
        if (!Guid.TryParse(examinerId, out var examinerGuid))
        {
            _logger.LogError("Invalid examinerId: {Value}", examinerId);
            return;
        }
        var ct = context.CancellationToken;

        // 2. Copy stream sang MemoryStream để tạo IFormFile
        await using var mem = new MemoryStream();
        await zipStream.CopyToAsync(mem, ct);
        mem.Position = 0;

        // 3. Tạo IFormFile giả lập từ MemoryStream
        IFormFile formFile = new FormFile(mem, 0, mem.Length, "ZipFile", name)
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/zip"
        };

        // 4. Build command để gọi service
        var command = new CreateSubmissionsFromZipCommand
        {
            ExamSubjectId = examSubjectGuid,
            ExaminerId   = examinerGuid,    
            ZipFile      = formFile
        };

        // 5. Gọi lại service để xử lý
        var result = await _submissionService.CreateSubmissionsFromZipAsync(command, ct);

        if (!result.Success)
        {
            _logger.LogError("Create submissions from zip failed: {Message}", result.Message);
            return;
        }

        _logger.LogInformation(
            "Created {Count} submissions from blob {Name}",
            result.Data?.Count ?? 0,
            name);
    }
}
