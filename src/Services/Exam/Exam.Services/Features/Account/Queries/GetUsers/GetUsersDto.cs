using Exam.Repositories.Repositories.Collections;

namespace Exam.Services.Features.Account.Queries.GetUsers;

public class GetUsersDto : PagedList<UserItemDto>
{
    public GetUsersDto(IEnumerable<UserItemDto> source, int pageIndex, int pageSize, int indexFrom) : base(source,
        pageIndex, pageSize, indexFrom)
    {
    }
}
