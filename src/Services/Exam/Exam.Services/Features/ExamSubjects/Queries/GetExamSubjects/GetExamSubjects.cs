using Exam.Services.Interfaces.Services;
using Exam.Services.Models.Responses;
using FluentValidation;
using MediatR;

namespace Exam.Services.Features.ExamSubjects.Queries.GetExamSubjects;

public record GetExamSubjectsQuery : IRequest<BaseServiceResponse>
{
    public string? ExamCode { get; init; }
    
    public string? SubjectCode { get; init; }
    public bool? IsActive { get; init; }
    public int PageIndex { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetExamSubjectsQueryValidator : AbstractValidator<GetExamSubjectsQuery>
{
    public GetExamSubjectsQueryValidator()
    {
        RuleFor(x => x.PageIndex)
            .GreaterThanOrEqualTo(1).WithMessage("Vui lòng nhập số trang lớn hơn hoặc bằng 1!");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Vui lòng nhập kích thước trang lớn hơn 0!");
    }
}

public class GetExamSubjectsQueryHandler : IRequestHandler<GetExamSubjectsQuery, BaseServiceResponse>
{
    private readonly IExamSubjectService _examSubjectService;

    public GetExamSubjectsQueryHandler(IExamSubjectService examSubjectService)
    {
        _examSubjectService = examSubjectService;
    }

    public async Task<BaseServiceResponse> Handle(GetExamSubjectsQuery request, CancellationToken cancellationToken)
    {
        return await _examSubjectService.GetExamSubjectsAsync(request);
    }
}
