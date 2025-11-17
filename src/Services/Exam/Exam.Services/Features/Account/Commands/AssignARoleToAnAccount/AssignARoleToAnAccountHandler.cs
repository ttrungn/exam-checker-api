using Domain.Constants;
using Exam.Services.Exceptions;
using Exam.Services.Interfaces.Services;
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
    private readonly IGraphClientService _graphClientService;
    private readonly ILogger<AssignARoleToAnAccountHandler> _logger;

    public AssignARoleToAnAccountHandler(
        ILogger<AssignARoleToAnAccountHandler> logger,
        IConfiguration configuration,
        GraphServiceClient graphClient,
        IGraphClientService graphClientService)
    {
        _logger = logger;
        _configuration = configuration;
        _graphClient = graphClient;
        _graphClientService = graphClientService;
    }

    public async Task<BaseServiceResponse> Handle(
        AssignARoleToAnAccountCommand request,
        CancellationToken ct)
    {
        _logger.LogInformation("Assign role invoked: UserId={UserId}, AppRoleId={AppRoleId}", request.UserId,
            request.AppRoleId);
        // 1) Get AppRoles using IGraphClientService
        var clientId = _configuration["AzureAD:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
        {
            _logger.LogWarning("Missing AzureAD:ClientId configuration");
            throw new ServiceUnavailableException("Đã có lỗi hệ thống xảy ra, vui lòng liên hệ admin để được hỗ trợ!");
        }

        var appRoles = await _graphClientService.GetAppRolesAsync(clientId, ct);
        var role = appRoles.FirstOrDefault(r => r.Id == request.AppRoleId);
        if (role == null)
        {
            _logger.LogError("App role not found on the application: AppRoleId={AppRoleId}", request.AppRoleId);
            throw new ServiceUnavailableException("Đã có lỗi hệ thống xảy ra, vui lòng liên hệ admin để được hỗ trợ!");
        }

        // 2) Get resource service principal
        var sp = await _graphClientService.GetServicePrincipalByClientIdAsync(clientId, ct);

        if (!Guid.TryParse(sp.Id, out var resourceSpId))
        {
            _logger.LogError("Service principal Id is not a valid GUID: {SpId}", sp.Id);
            throw new ServiceUnavailableException("Đã có lỗi hệ thống xảy ra, vui lòng liên hệ admin để được hỗ trợ!");
        }

        AppRoleAssignment? createdAppRoleAssignment = null;
        var directoryRolesAdded = new List<string>();
        var deletedAssignments = new List<AppRoleAssignment>();
        var deletedDirectoryRoles = new List<string>();
        try
        {
            // 3) Delete existing directory roles first
            var existingDirectoryRoles = await _graphClientService.GetUserDirectoryRolesAsync(request.UserId, ct);
            foreach (var dirRole in existingDirectoryRoles)
            {
                await _graphClient.DirectoryRoles[dirRole.Id!].Members[request.UserId.ToString()].Ref
                    .DeleteAsync(cancellationToken: ct);
                deletedDirectoryRoles.Add(dirRole.Id!);
                _logger.LogInformation(
                    "Deleted existing directory role: UserId={UserId}, RoleId={RoleId}, RoleName={RoleName}",
                    request.UserId, dirRole.Id, dirRole.DisplayName);
            }

            // 4) Delete existing app role assignments before assigning new one
            var existingAssignments = await _graphClientService.GetUserAppRolesAsync(request.UserId, resourceSpId, ct);
            foreach (var existingAssignment in existingAssignments)
            {
                await _graphClient.Users[request.UserId.ToString()].AppRoleAssignments[existingAssignment.Id!]
                    .DeleteAsync(cancellationToken: ct);
                deletedAssignments.Add(existingAssignment);
                _logger.LogInformation(
                    "Deleted existing app role assignment: UserId={UserId}, AppRoleId={AppRoleId}, AssignmentId={AssignmentId}",
                    request.UserId, existingAssignment.AppRoleId, existingAssignment.Id);
            }

            // 5) Create the appRoleAssignment
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
                throw new ServiceUnavailableException(
                    "Đã có lỗi hệ thống xảy ra, vui lòng liên hệ admin để được hỗ trợ!");
            }

            if (role.Value != null && role.Value.Equals(Roles.Admin, StringComparison.OrdinalIgnoreCase))
            {
                var requiredRoles = await _graphClientService.GetDirectoryRolesByNamesAsync(AdminDirectoryRoles, ct);
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
            else if (role.Value != null && role.Value.Equals(Roles.Manager, StringComparison.OrdinalIgnoreCase))
            {
            }
            else if (role.Value != null && role.Value.Equals(Roles.Moderator, StringComparison.OrdinalIgnoreCase))
            {
            }
            else if (role.Value != null && role.Value.Equals(Roles.Examiner, StringComparison.OrdinalIgnoreCase))
            {
            }

            _logger.LogInformation("Assigned role to user: UserId={UserId}, AppRoleId={AppRoleId}", request.UserId,
                request.AppRoleId);
            return new BaseServiceResponse { Success = true, Message = "Thêm vai trò cho người dùng thành công!" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failure during assignment/DirectoryRoles; starting compensation. UserId={UserId}, AppRoleId={AppRoleId}",
                request.UserId, request.AppRoleId);
            await CompensateAsync(request.UserId, createdAppRoleAssignment, directoryRolesAdded, deletedAssignments,
                deletedDirectoryRoles, resourceSpId, ct);
            throw new ServiceUnavailableException("Đã có lỗi hệ thống xảy ra, vui lòng liên hệ admin để được hỗ trợ!");
        }
    }


    private async Task CompensateAsync(Guid userId, AppRoleAssignment? createdAssignment,
        List<string> directoryRolesAdded, List<AppRoleAssignment> deletedAssignments,
        List<string> deletedDirectoryRoles, Guid resourceSpId,
        CancellationToken ct)
    {
        // Remove any directory roles that were added during the assignment
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

        // Delete the newly created app role assignment
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

        // Restore previously deleted app role assignments
        foreach (var deletedAssignment in deletedAssignments)
        {
            try
            {
                var restoredAssignment = new AppRoleAssignment
                {
                    PrincipalId = userId, ResourceId = resourceSpId, AppRoleId = deletedAssignment.AppRoleId
                };

                await _graphClient.Users[userId.ToString()].AppRoleAssignments
                    .PostAsync(restoredAssignment, cancellationToken: ct);
                _logger.LogInformation(
                    "Compensation: restored app role assignment for user {UserId}, AppRoleId={AppRoleId}",
                    userId, deletedAssignment.AppRoleId);
            }
            catch (Exception restoreEx)
            {
                _logger.LogWarning(restoreEx,
                    "Compensation warning: could not restore app role assignment for user {UserId}, AppRoleId={AppRoleId}",
                    userId, deletedAssignment.AppRoleId);
            }
        }

        // Restore previously deleted directory roles
        foreach (var deletedRoleId in deletedDirectoryRoles)
        {
            try
            {
                await _graphClient.DirectoryRoles[deletedRoleId].Members.Ref.PostAsync(
                    new ReferenceCreate { OdataId = $"https://graph.microsoft.com/v1.0/users/{userId}" },
                    cancellationToken: ct);
                _logger.LogInformation(
                    "Compensation: restored directory role for user {UserId}, RoleId={RoleId}",
                    userId, deletedRoleId);
            }
            catch (Exception restoreEx)
            {
                _logger.LogWarning(restoreEx,
                    "Compensation warning: could not restore directory role for user {UserId}, RoleId={RoleId}",
                    userId, deletedRoleId);
            }
        }
    }
}
