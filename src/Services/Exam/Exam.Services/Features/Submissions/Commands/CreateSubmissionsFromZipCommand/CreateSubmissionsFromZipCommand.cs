using Exam.Services.Models.Responses;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Exam.Services.Features.Submissions.Commands.CreateSubmissionsFromZipCommand ;

public record CreateSubmissionsFromZipCommand  : IRequest<DataServiceResponse<List<Guid>>>
{
    public Guid ExaminerId { get; set; }
    public Guid ExamSubjectId { get; set; }
    public Guid ModeratorId { get; set; }
    public IFormFile ArchiveFile { get; set; } = null!;
}

public class CreateSubmissionsFromZipCommandValidator : AbstractValidator<CreateSubmissionsFromZipCommand >
{
    private static readonly string[] SupportedExtensions= [".zip", ".rar"];
    public CreateSubmissionsFromZipCommandValidator()
    {
        RuleFor(x => x.ExaminerId)
            .NotEmpty().WithMessage("ExaminerId is required.");
        RuleFor(x => x.ExamSubjectId)
            .NotEmpty().WithMessage("ExamSubjectId is required.");
        RuleFor(x => x.ModeratorId)
            .NotEmpty().WithMessage("ModeratorId is required.");
        RuleFor(x => x.ArchiveFile)
            .NotEmpty().WithMessage("ArchiveFile is required.")
            .Must(file => file != null && SupportedExtensions.Any(ext => 
                file.FileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
            .WithMessage("File must be a .zip or .rar archive.");
    }
}
