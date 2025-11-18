using Exam.Services.Interfaces.Services;
using Exam.Services.Models.Responses;
using MediatR;

namespace Exam.Services.Features.ExamSubjects.Queries.GetExamSubjectById;

public record GetExamSubjectByIdQuery(Guid Id) : IRequest<BaseServiceResponse>;

public class GetExamSubjectByIdQueryHandler : IRequestHandler<GetExamSubjectByIdQuery, BaseServiceResponse>
{
    private readonly IExamSubjectService _examSubjectService;

    public GetExamSubjectByIdQueryHandler(IExamSubjectService examSubjectService)
    {
        _examSubjectService = examSubjectService;
    }

    public async Task<BaseServiceResponse> Handle(GetExamSubjectByIdQuery request, CancellationToken cancellationToken)
    {
        return await _examSubjectService.GetExamSubjectByIdAsync(request.Id);
    }
}
