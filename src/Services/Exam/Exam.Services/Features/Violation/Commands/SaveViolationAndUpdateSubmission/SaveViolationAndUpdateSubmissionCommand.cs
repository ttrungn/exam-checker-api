using Exam.Domain.Enums;
using Exam.Services.Models.Responses;
using FluentValidation;
using MediatR;

namespace Exam.Services.Features.Violation.Commands.SaveViolationAndUpdateSubmission;

public record SaveViolationAndUpdateSubmissionCommand : IRequest<BaseServiceResponse>
{
    public Guid SubmissionId { get; init; }
    public List<ViolationDto> Violations { get; init; } = new();
}

public record ViolationDto
{
    public ViolationPolicy ViolationType { get; init; }
    public string Description { get; init; } = string.Empty;
}

public class SaveViolationAndUpdateSubmissionCommandValidator : AbstractValidator<SaveViolationAndUpdateSubmissionCommand>
{
    public SaveViolationAndUpdateSubmissionCommandValidator()
    {
        RuleFor(x => x.SubmissionId)
            .NotEmpty().WithMessage("SubmissionId is required.");

        RuleFor(x => x.Violations)
            .NotNull().WithMessage("Violations list cannot be null.");
    }
}
