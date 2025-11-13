using Exam.Services.Features.Account.Queries.GetAppRoles;
using Microsoft.Graph.Models;

namespace Exam.Services.Mappers;

public static class AppRoleMapper
{
    public static AppRoleItemDto ToAppRoleItemDto(this AppRole appRole)
    {
        return new AppRoleItemDto()
        {
            Id = appRole.Id,
            DisplayName = appRole.DisplayName,
            Value = appRole.Value,
            Description = appRole.Description
        };
    }
}
