using Exam.Services.Models.Responses;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Exam.Services.Features.Submission.Commands.CreateSubmissionsFromZipCommand ;

public record CreateSubmissionsFromZipCommand  : IRequest<DataServiceResponse<List<Guid>>>
{
    public Guid ExaminerId { get; set; }
    public Guid ExamSubjectId { get; set; }
    public IFormFile ZipFile { get; set; } = null!;
}

public class CreateSubmissionsFromZipCommandValidator : AbstractValidator<CreateSubmissionsFromZipCommand >
{
    public CreateSubmissionsFromZipCommandValidator()
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
