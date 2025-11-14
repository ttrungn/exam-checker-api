using Exam.Services.Features.Account.Queries.GetExaminers;
using Exam.Services.Features.Account.Queries.GetUserProfile;
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

    public static UserProfileDto ToUserProfileDto(
        this User user,
        List<AppRole> appRoles,
        DirectoryObject? manager = null)
    {
        var currentAppRoles = new List<AppRoleDto>();

        if (user.AppRoleAssignments != null)
        {
            foreach (var assignment in user.AppRoleAssignments)
            {
                if (!assignment.AppRoleId.HasValue)
                {
                    continue;
                }

                var roleId = assignment.AppRoleId.Value;
                var role = appRoles.FirstOrDefault(r => r.Id == roleId);
                if (role != null)
                {
                    currentAppRoles.Add(new AppRoleDto
                    {
                        Id = roleId, Value = role.Value, DisplayName = role.DisplayName
                    });
                }
            }
        }

        ManagerDto? managerDto = null;
        if (manager is User mu)
        {
            managerDto = new ManagerDto
            {
                Id = mu.Id, DisplayName = mu.DisplayName, Mail = mu.Mail, UserPrincipalName = mu.UserPrincipalName
            };
        }
        else if (manager is OrgContact oc)
        {
            managerDto = new ManagerDto
            {
                Id = oc.Id, DisplayName = oc.DisplayName, Mail = oc.Mail, UserPrincipalName = null
            };
        }

        return new UserProfileDto
        {
            Id = user.Id,
            Identities =
                user.Identities?.Select(i => new ObjectIdentityDto
                {
                    SignInType = i.SignInType, Issuer = i.Issuer, IssuerAssignedId = i.IssuerAssignedId
                }).ToList() ?? [],
            GivenName = user.GivenName,
            Surname = user.Surname,
            UserType = user.UserType,
            AuthorizationInfo = user.AuthorizationInfo == null
                ? null
                : new AuthorizationInfoDto
                {
                    CertificateUserIds = user.AuthorizationInfo.CertificateUserIds?.ToList() ?? []
                },
            JobTitle = user.JobTitle,
            CompanyName = user.CompanyName,
            Department = user.Department,
            EmployeeId = user.EmployeeId,
            EmployeeType = user.EmployeeType,
            EmployeeHireDate = user.EmployeeHireDate,
            OfficeLocation = user.OfficeLocation,
            Manager = managerDto,
            StreetAddress = user.StreetAddress,
            City = user.City,
            State = user.State,
            PostalCode = user.PostalCode,
            Country = user.Country,
            BusinessPhones = user.BusinessPhones?.ToList() ?? [],
            MobilePhone = user.MobilePhone,
            Email = user.Mail,
            OtherEmails = user.OtherMails?.ToList() ?? [],
            FaxNumber = user.FaxNumber,
            AgeGroup = user.AgeGroup,
            ConsentProvidedForMinor = user.ConsentProvidedForMinor,
            UsageLocation = user.UsageLocation,
            Roles = currentAppRoles
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
