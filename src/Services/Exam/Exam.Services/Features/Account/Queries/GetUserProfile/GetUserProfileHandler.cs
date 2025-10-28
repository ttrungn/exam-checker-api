using Exam.Services.Exceptions;
using Exam.Services.Models.Responses;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace Exam.Services.Features.Account.Queries.GetUserProfile;

public class GetUserProfileHandler
    : IRequestHandler<GetUserProfileQuery, DataServiceResponse<UserProfileDto>>
{
    private readonly IConfiguration _configuration;
    private readonly GraphServiceClient _graphClient;
    private readonly ILogger<GetUserProfileHandler> _logger;

    public GetUserProfileHandler(
        IConfiguration configuration,
        ILogger<GetUserProfileHandler> logger,
        GraphServiceClient graphClient)
    {
        _configuration = configuration;
        _logger = logger;
        _graphClient = graphClient;
    }

    public async Task<DataServiceResponse<UserProfileDto>> Handle(GetUserProfileQuery request, CancellationToken ct)
    {
        _logger.LogInformation("GetUserProfile (slim) invoked: UserId={UserId}", request.UserId);

        User? user;
        try
        {
            user = await _graphClient.Users[request.UserId.ToString()].GetAsync(q =>
            {
                q.QueryParameters.Select =
                [
                    "id", "identities", "givenName", "surname", "userType", "authorizationInfo", "jobTitle",
                    "companyName", "department", "employeeId", "employeeType", "employeeHireDate", "officeLocation",
                    "streetAddress", "city", "state", "postalCode", "country", "businessPhones", "mobilePhone",
                    "mail", "otherMails", "faxNumber", "ageGroup", "consentProvidedForMinor", "usageLocation"
                ];
            }, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user {UserId}", request.UserId);
            throw new ServiceUnavailableException("Cannot get user profile right now, please contact admin for help.");
        }

        if (user is null)
        {
            throw new NotFoundException("User not found");
        }

        ManagerDto? managerDto = null;
        try
        {
            var managerFull = await _graphClient.Users[user.Id!].Manager.GetAsync(q =>
            {
                q.QueryParameters.Select = ["id", "displayName", "mail", "userPrincipalName"];
            }, ct);

            if (managerFull is User mu)
            {
                managerDto = new ManagerDto
                {
                    Id = mu.Id,
                    DisplayName = mu.DisplayName,
                    Mail = mu.Mail,
                    UserPrincipalName = mu.UserPrincipalName
                };
            }
            else if (managerFull is OrgContact oc)
            {
                managerDto = new ManagerDto
                {
                    Id = oc.Id, DisplayName = oc.DisplayName, Mail = oc.Mail, UserPrincipalName = null
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving manager for user {UserId}", request.UserId);
        }

        List<AppRoleDto> currentAppRoles = [];
        var clientId = _configuration["AzureAD:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
        {
            _logger.LogWarning("Missing AzureAD:ClientId configuration");
            throw new ServiceUnavailableException("Cannot get user profile right now, please contact admin for help.");
        }

        try
        {
            // 1) Find the service principal representing *your* application (resource)
            var sps = await _graphClient.ServicePrincipals.GetAsync(req =>
            {
                req.QueryParameters.Filter = $"appId eq '{clientId}'";
                req.QueryParameters.Select = ["id", "displayName", "appId", "appRoles"];
                req.QueryParameters.Top = 1;
            }, ct);

            var resourceSp = sps?.Value?.FirstOrDefault();
            if (resourceSp?.Id is not null)
            {
                // 2) Get the user's app role assignments *to this resource*
                var assignmentsResp = await _graphClient.Users[user.Id!].AppRoleAssignments.GetAsync(q =>
                {
                    q.QueryParameters.Filter = $"resourceId eq {resourceSp.Id}";
                    q.QueryParameters.Select = ["id", "appRoleId", "resourceId"];
                }, ct);

                var appRoles = resourceSp.AppRoles ?? [];
                foreach (var a in assignmentsResp?.Value ?? Enumerable.Empty<AppRoleAssignment>())
                {
                    if (a.AppRoleId is not { } roleId)
                    {
                        continue;
                    }

                    var role = appRoles.FirstOrDefault(r => r.Id == roleId);
                    if (role is not null)
                    {
                        currentAppRoles.Add(new AppRoleDto
                        {
                            Id = role.Id, Value = role.Value, DisplayName = role.DisplayName
                        });
                    }
                }
            }
        }
        catch
        {
            throw new ServiceUnavailableException("Cannot get user profile right now, please contact admin for help.");
        }

        var dto = new UserProfileDto
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
            AuthorizationInfo =
                user.AuthorizationInfo is null
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

        _logger.LogInformation("GetUserProfile (slim) success: UserId={UserId}", request.UserId);
        return new DataServiceResponse<UserProfileDto>
        {
            Success = true, Message = "Fetched user profile successfully.", Data = dto
        };
    }
}
