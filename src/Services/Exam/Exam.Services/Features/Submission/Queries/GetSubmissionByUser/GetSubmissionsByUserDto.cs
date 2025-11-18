using Exam.Repositories.Repositories.Collections;

namespace Exam.Services.Features.Submission.Queries.GetSubmissionByUser;

public class GetSubmissionsByUserDto: PagedList<SubmissionUserItemDto>
{
    public GetSubmissionsByUserDto(IEnumerable<SubmissionUserItemDto> source, int pageIndex, int pageSize, int indexFrom) 
        : base(source, pageIndex, pageSize, indexFrom)
    {
    }
}
