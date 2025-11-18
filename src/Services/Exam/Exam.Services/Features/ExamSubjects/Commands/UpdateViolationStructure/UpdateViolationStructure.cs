using Exam.Services.Interfaces.Services;
using Exam.Services.Models.Responses;
using Exam.Services.Models.Validations;
using FluentValidation;
using MediatR;

namespace Exam.Services.Features.ExamSubjects.Commands.UpdateViolationStructure;

public record UpdateViolationStructureCommand : IRequest<BaseServiceResponse>
{
    public Guid ExamSubjectId { get; init; }
    public ValidationRules Rules { get; init; } = null!;
}

public class UpdateViolationStructureCommandValidator : AbstractValidator<UpdateViolationStructureCommand>
{
    public UpdateViolationStructureCommandValidator()
    {
        RuleFor(x => x.ExamSubjectId)
            .NotEmpty().WithMessage("Vui lòng nhập ExamSubjectId!");

        RuleFor(x => x.Rules)
            .NotNull().WithMessage("Vui lòng cung cấp cấu hình vi phạm!");
    }
}

public class UpdateViolationStructureCommandHandler : IRequestHandler<UpdateViolationStructureCommand, BaseServiceResponse>
{
    private readonly IExamSubjectService _examSubjectService;

    public UpdateViolationStructureCommandHandler(IExamSubjectService examSubjectService)
    {
        _examSubjectService = examSubjectService;
    }

    public async Task<BaseServiceResponse> Handle(UpdateViolationStructureCommand request, CancellationToken cancellationToken)
    {
        return await _examSubjectService.UpdateViolationStructureAsync(request.ExamSubjectId, request.Rules);
    }
}
