using Exam.Services.Interfaces.Services;
using Exam.Services.Models.Responses;
using MediatR;

namespace Exam.Services.Features.ExamSubjects.Queries.GetExamSubjectById;

public record GetExamSubjectByIdWithViolationStructureQuery(Guid Id) : IRequest<BaseServiceResponse>;

public class GetExamSubjectByIdWithViolationStructureQueryHandler : IRequestHandler<GetExamSubjectByIdWithViolationStructureQuery, BaseServiceResponse>
{
    private readonly IExamSubjectService _examSubjectService;

    public GetExamSubjectByIdWithViolationStructureQueryHandler(IExamSubjectService examSubjectService)
    {
        _examSubjectService = examSubjectService;
    }

    public async Task<BaseServiceResponse> Handle(GetExamSubjectByIdWithViolationStructureQuery request, CancellationToken cancellationToken)
    {
        return await _examSubjectService.GetExamSubjectByIdAsync(request.Id);
    }
}
