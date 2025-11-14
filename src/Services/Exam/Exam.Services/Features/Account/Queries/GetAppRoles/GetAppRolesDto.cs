using Exam.Repositories.Repositories.Collections;

namespace Exam.Services.Features.Account.Queries.GetAppRoles;

public class GetAppRolesDto : PagedList<AppRoleItemDto>
{
    public GetAppRolesDto(IEnumerable<AppRoleItemDto> source, int pageIndex, int pageSize, int indexFrom) : base(source,
        pageIndex, pageSize, indexFrom)
    {
    }
}
