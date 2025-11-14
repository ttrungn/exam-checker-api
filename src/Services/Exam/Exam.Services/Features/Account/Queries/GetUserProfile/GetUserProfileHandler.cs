using Exam.Services.Exceptions;
using Exam.Services.Interfaces.Services;
using Exam.Services.Mappers;
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
    private readonly IGraphClientService _graphClientService;
    private readonly ILogger<GetUserProfileHandler> _logger;

    public GetUserProfileHandler(
        IConfiguration configuration,
        ILogger<GetUserProfileHandler> logger,
        GraphServiceClient graphClient,
        IGraphClientService graphClientService
    )
    {
        _configuration = configuration;
        _logger = logger;
        _graphClient = graphClient;
        _graphClientService = graphClientService;
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
            _logger.LogInformation("User {UserId} not found", request.UserId);
            throw new NotFoundException("User not found");
        }

        DirectoryObject? manager = null;
        try
        {
            manager = await _graphClient.Users[user.Id!].Manager.GetAsync(q =>
            {
                q.QueryParameters.Select = ["id", "displayName", "mail", "userPrincipalName"];
            }, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving manager for user {UserId}", request.UserId);
        }

        var clientId = _configuration["AzureAD:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
        {
            _logger.LogWarning("Missing AzureAD:ClientId configuration");
            throw new ServiceUnavailableException("Cannot get user profile right now, please contact admin for help.");
        }

        var appRoles = await _graphClientService.GetAppRolesAsync(clientId, ct);
        try
        {
            var assignmentsResp = await _graphClient.Users[user.Id!].AppRoleAssignments.GetAsync(
                r =>
                {
                    r.QueryParameters.Select = ["appRoleId"];
                }, ct);

            user.AppRoleAssignments = assignmentsResp?.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting app role assignments for user {UserId}", request.UserId);
            throw new ServiceUnavailableException("Cannot get user profile right now, please contact admin for help.");
        }

        var dto = user.ToUserProfileDto(appRoles, manager);

        _logger.LogInformation("GetUserProfile (slim) success: UserId={UserId}", request.UserId);
        return new DataServiceResponse<UserProfileDto>
        {
            Success = true, Message = "Fetched user profile successfully.", Data = dto
        };
    }
}
