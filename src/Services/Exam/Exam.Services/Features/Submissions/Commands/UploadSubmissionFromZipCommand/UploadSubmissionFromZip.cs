using Exam.Services.Interfaces.Services;
using Exam.Services.Models.Responses;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Exam.Services.Features.Submissions.Commands.UploadSubmissionFromZipCommand;




public record UploadSubmissionFromZipCommand  : IRequest<DataServiceResponse<List<Guid>>>
{
    public Guid ExaminerId { get; set; }
    public Guid ExamSubjectId { get; set; }
    public IFormFile ZipFile { get; set; } = null!;
}

public class UploadSubmissionFromZipCommandValidator : AbstractValidator<UploadSubmissionFromZipCommand >
{
    public UploadSubmissionFromZipCommandValidator()
    {
        RuleFor(x => x.ExaminerId)
            .NotEmpty().WithMessage("ExaminerId is required.");
        RuleFor(x => x.ExamSubjectId)
            .NotEmpty().WithMessage("ExamSubjectId is required.");
        RuleFor(x => x.ZipFile)
            .NotEmpty().WithMessage("ZipFile is required.")
            .Must(file => file != null &&
                          file.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            .WithMessage("File must be a .zip archive.");
    }
}

public class UploadSubmissionFromZipCommandHandler : IRequestHandler<UploadSubmissionFromZipCommand , DataServiceResponse<List<Guid>>>
{
    private readonly ISubmissionService _service;
    private readonly ILogger<UploadSubmissionFromZipCommandHandler> _logger;

    public UploadSubmissionFromZipCommandHandler(ISubmissionService service, ILogger<UploadSubmissionFromZipCommandHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<DataServiceResponse<List<Guid>>> Handle(UploadSubmissionFromZipCommand  request, CancellationToken cancellationToken)
    {
        var uploadResult = await _service.UploadZipForProcessingAsync(request, cancellationToken);
        if (!uploadResult.Success)
        {
            return new()
            {
                Success = false,
                Message = uploadResult.Message
            };
        }

        return new()
        {
            Success = true,
            Message = "File đã upload, submissions sẽ được tạo bởi background Function.",
        };
    }
}
