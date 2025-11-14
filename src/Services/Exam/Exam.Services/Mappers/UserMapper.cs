using Exam.Services.Features.Account.Queries.GetExaminers;
using Exam.Services.Features.Account.Queries.GetUsers;
using Microsoft.Graph.Models;

namespace Exam.Services.Mappers;

public static class UserMapper
{
    public static UserItemDto ToUserItemDto(this User user, List<AppRole> appRoles)
    {
        var roleValues = new List<string>();

        if (user.AppRoleAssignments != null && appRoles.Count > 0)
        {
            var roleLookup = appRoles.ToDictionary(r => r.Id!.Value, r => r.Value ?? string.Empty);

            roleValues.AddRange(
                user.AppRoleAssignments
                    .Where(x => x.AppRoleId.HasValue &&
                                x.AppRoleId.Value != Guid.Empty &&
                                roleLookup.ContainsKey(x.AppRoleId.Value))
                    .Select(x => roleLookup[x.AppRoleId!.Value])
                    .Where(value => !string.IsNullOrEmpty(value))
                    .Distinct()
            );
        }

        return new UserItemDto
        {
            Id = user.Id ?? string.Empty,
            Email = user.Mail ?? user.UserPrincipalName ?? string.Empty,
            UserPrincipalName = user.UserPrincipalName ?? string.Empty,
            DisplayName = user.DisplayName ?? string.Empty,
            GivenName = user.GivenName ?? string.Empty,
            Surname = user.Surname ?? string.Empty,
            JobTitle = user.JobTitle ?? string.Empty,
            Roles = roleValues
        };
    }

    public static ExaminerItemDto ToExaminerItemDto(this User user)
    {
        return new ExaminerItemDto
        {
            Id = user.Id ?? string.Empty,
            Email = user.Mail ?? user.UserPrincipalName ?? string.Empty,
            UserPrincipalName = user.UserPrincipalName ?? string.Empty,
            DisplayName = user.DisplayName ?? string.Empty,
            GivenName = user.GivenName ?? string.Empty,
            Surname = user.Surname ?? string.Empty,
            JobTitle = user.JobTitle ?? string.Empty
        };
    }
}
