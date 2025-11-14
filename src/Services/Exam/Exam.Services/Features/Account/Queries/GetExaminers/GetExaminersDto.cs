using Exam.Repositories.Repositories.Collections;

namespace Exam.Services.Features.Account.Queries.GetExaminers;

public class GetExaminersDto : PagedList<ExaminerItemDto>
{
    public GetExaminersDto(IEnumerable<ExaminerItemDto> source, int pageIndex, int pageSize, int indexFrom) 
        : base(source, pageIndex, pageSize, indexFrom)
    {
    }
}



