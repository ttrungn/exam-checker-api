using Domain.Constants;
using Exam.Services.Models.Responses;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace Exam.Services.Features.Account.Commands.AssignARoleToAnAccount;

public class AssignARoleToAnAccountHandler
    : IRequestHandler<AssignARoleToAnAccountCommand, BaseServiceResponse>
{
    private static readonly string[] AdminDirectoryRoles = ["User Administrator", "Groups Administrator"];
    private readonly IConfiguration _configuration;
    private readonly GraphServiceClient _graphClient;
    private readonly ILogger<AssignARoleToAnAccountHandler> _logger;

    public AssignARoleToAnAccountHandler(
        ILogger<AssignARoleToAnAccountHandler> logger,
        IConfiguration configuration,
        GraphServiceClient graphClient)
    {
        _logger = logger;
        _configuration = configuration;
        _graphClient = graphClient;
    }

    public async Task<BaseServiceResponse> Handle(
        AssignARoleToAnAccountCommand request,
        CancellationToken ct)
    {
        _logger.LogInformation("Assign role invoked: UserId={UserId}, AppRoleId={AppRoleId}", request.UserId,
            request.AppRoleId);
        // 1) Resolve the resource service principal id
        var clientId = _configuration["AzureAD:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
        {
            _logger.LogWarning("Missing AzureAD:ClientId configuration");
            return new BaseServiceResponse
            {
                Success = false, Message = "Server configuration error: missing AzureAD:ClientId."
            };
        }

        var sps = await _graphClient.ServicePrincipals
            .GetAsync(q =>
            {
                q.QueryParameters.Filter = $"appId eq '{clientId}'";
                q.QueryParameters.Top = 1;
            }, ct);

        var sp = sps?.Value?.FirstOrDefault();
        if (sp == null)
        {
            _logger.LogWarning("Resource service principal not found for configured client id");
            return new BaseServiceResponse
            {
                Success = false, Message = "Resource service principal not found for the configured client id."
            };
        }

        var role = sp.AppRoles?.FirstOrDefault(r => r.Id == request.AppRoleId);
        if (role == null)
        {
            _logger.LogWarning("App role not found on the application: AppRoleId={AppRoleId}", request.AppRoleId);
            return new BaseServiceResponse { Success = false, Message = "App role not found on this application." };
        }

        if (!Guid.TryParse(sp.Id, out var resourceSpId))
        {
            _logger.LogError("Service principal Id is not a valid GUID: {SpId}", sp.Id);
            return new BaseServiceResponse { Success = false, Message = "Invalid service principal id." };
        }

        // 2) Idempotency: skip if the assignment already exists
        var existingForResource = await _graphClient
            .Users[request.UserId.ToString()]
            .AppRoleAssignments
            .GetAsync(q => { q.QueryParameters.Filter = $"resourceId eq {resourceSpId}"; }, ct);

        if (existingForResource?.Value?.Any(a => a.AppRoleId == request.AppRoleId) == true)
        {
            // Essential outcome log
            _logger.LogInformation("Role already assigned: UserId={UserId}, AppRoleId={AppRoleId}", request.UserId,
                request.AppRoleId);
            return new BaseServiceResponse { Success = true, Message = "Role already assigned to the Exam." };
        }

        // 3) Create the appRoleAssignment
        AppRoleAssignment? createdAppRoleAssignment = null;
        var directoryRolesAdded = new List<string>();
        try
        {
            var assignment = new AppRoleAssignment
            {
                PrincipalId = request.UserId, ResourceId = resourceSpId, AppRoleId = request.AppRoleId
            };

            createdAppRoleAssignment = await _graphClient.Users[request.UserId.ToString()].AppRoleAssignments
                .PostAsync(assignment, cancellationToken: ct);
            if (createdAppRoleAssignment == null)
            {
                _logger.LogError("Failed to assign role to user: UserId={UserId}, AppRoleId={AppRoleId}",
                    request.UserId,
                    request.AppRoleId);
                return new BaseServiceResponse { Success = false, Message = "Failed to assign role to Exam." };
            }

            if (role.Value != null && role.Value.Equals(Roles.Admin, StringComparison.OrdinalIgnoreCase))
            {
                var requiredRoles = await GetDirectoryRolesOrThrow(AdminDirectoryRoles, ct);

                foreach (var dirRole in requiredRoles)
                {
                    await _graphClient.DirectoryRoles[dirRole.Id!].Members.Ref.PostAsync(
                        new ReferenceCreate { OdataId = $"https://graph.microsoft.com/v1.0/users/{request.UserId}" },
                        cancellationToken: ct);

                    directoryRolesAdded.Add(dirRole.Id!);
                    _logger.LogInformation("Added user {UserId} to directory role {RoleName} ({RoleId})",
                        request.UserId, dirRole.DisplayName, dirRole.Id);
                }
            }

            _logger.LogInformation("Assigned role to user: UserId={UserId}, AppRoleId={AppRoleId}", request.UserId,
                request.AppRoleId);
            return new BaseServiceResponse { Success = true, Message = "Assigned role to user successfully." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failure during assignment/DirectoryRoles; starting compensation. UserId={UserId}, AppRoleId={AppRoleId}",
                request.UserId, request.AppRoleId);
            await CompensateAsync(request.UserId, createdAppRoleAssignment, directoryRolesAdded, ct);
            return new BaseServiceResponse
            {
                Success = false, Message = "Failed to assign role; all changes were rolled back."
            };
        }
    }

    // READ-ONLY: fetch directory roles by displayName; if any are missing, throw to trigger compensation.
    private async Task<List<DirectoryRole>> GetDirectoryRolesOrThrow(string[] names, CancellationToken ct)
    {
        var results = new List<DirectoryRole>();

        foreach (var name in names)
        {
            var resp = await _graphClient.DirectoryRoles.GetAsync(q =>
            {
                q.QueryParameters.Select = ["id", "displayName"];
                q.QueryParameters.Filter = $"displayName eq '{name}'";
            }, ct);

            var role = resp?.Value?.FirstOrDefault();
            if (role is null)
            {
                throw new InvalidOperationException($"Required directory role is not active: {name}");
            }

            results.Add(role);
        }

        return results;
    }

    private async Task CompensateAsync(Guid userId, AppRoleAssignment? createdAssignment,
        List<string> directoryRolesAdded, CancellationToken ct)
    {
        foreach (var roleId in directoryRolesAdded.AsEnumerable().Reverse())
        {
            try
            {
                await _graphClient.DirectoryRoles[roleId].Members[userId.ToString()].Ref
                    .DeleteAsync(cancellationToken: ct);
                _logger.LogInformation("Compensation: removed user {UserId} from directory role {RoleId}", userId,
                    roleId);
            }
            catch (Exception remEx)
            {
                _logger.LogWarning(remEx,
                    "Compensation warning: could not remove user {UserId} from directory role {RoleId}", userId,
                    roleId);
            }
        }

        if (createdAssignment?.Id is not null)
        {
            try
            {
                await _graphClient.Users[userId.ToString()].AppRoleAssignments[createdAssignment.Id]
                    .DeleteAsync(cancellationToken: ct);
                _logger.LogInformation("Compensation: deleted app role assignment {AssignmentId} for user {UserId}",
                    createdAssignment.Id, userId);
            }
            catch (Exception delEx)
            {
                _logger.LogWarning(delEx,
                    "Compensation warning: could not delete app role assignment {AssignmentId} for user {UserId}",
                    createdAssignment.Id, userId);
            }
        }
    }
}
