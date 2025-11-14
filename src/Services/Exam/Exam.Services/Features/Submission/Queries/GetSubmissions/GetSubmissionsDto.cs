using Exam.Repositories.Repositories.Collections;

namespace Exam.Services.Features.Submission.Queries.GetSubmissions;

public class GetSubmissionsDto : PagedList<SubmissionItemDto>
{
    public GetSubmissionsDto(IEnumerable<SubmissionItemDto> source, int pageIndex, int pageSize, int indexFrom) 
        : base(source, pageIndex, pageSize, indexFrom)
    {
    }
}
