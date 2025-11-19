using Exam.Repositories.Repositories.Collections;

namespace Exam.Services.Features.Submissions.Queries.GetSubmissions;

public class GetSubmissionsDto : PagedList<SubmissionItemDto>
{
    public GetSubmissionsDto()
    {
    }

    public GetSubmissionsDto(IEnumerable<SubmissionItemDto> source, int pageIndex, int pageSize, int indexFrom)
        : base(source, pageIndex, pageSize, indexFrom)
    {
    }
}
