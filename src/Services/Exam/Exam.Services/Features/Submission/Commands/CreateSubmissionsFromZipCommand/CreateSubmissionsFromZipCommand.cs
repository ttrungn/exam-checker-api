
using Exam.Services.Models.Responses;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Exam.Services.Features.Submission.Commands.CreateSubmissionsFromZipCommand ;

public record CreateSubmissionsFromZipCommand  : IRequest<DataServiceResponse<List<Guid>>>
{
    public Guid? ExaminerId { get; set; }
    public Guid ExamSubjectId { get; set; }
    public Guid? ModeratorId { get; set; }
    public IFormFile? ZipFile { get; set; } = null!;
}

public class CreateSubmissionsFromZipCommandValidator : AbstractValidator<CreateSubmissionsFromZipCommand >
{
    public CreateSubmissionsFromZipCommandValidator()
    {
        RuleFor(x => x.ExamSubjectId)
            .NotEmpty().WithMessage("ExamSubjectId is required.");
        // RuleFor(x => x.FileUrl)
        //     .NotEmpty().WithMessage("FileUrl is required.");
    }
}
