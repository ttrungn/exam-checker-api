using Exam.Services.Exceptions;
using Exam.Services.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace Exam.Services.Services;

public class GraphClientService : IGraphClientService
{
    private readonly GraphServiceClient _graphClient;
    private readonly ILogger<GraphClientService> _logger;

    public GraphClientService(
        GraphServiceClient graphClient,
        ILogger<GraphClientService> logger)
    {
        _graphClient = graphClient;
        _logger = logger;
    }

    public async Task<List<AppRole>> GetAppRolesAsync(string clientId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching application roles from Microsoft Graph.");

        try
        {
            var apps = await _graphClient.Applications.GetAsync(r =>
            {
                r.QueryParameters.Filter = $"appId eq '{clientId}'";
                r.QueryParameters.Select = ["id", "displayName", "appId", "appRoles"];
            }, cancellationToken);

            var application = apps?.Value?.FirstOrDefault();
            if (application is null)
            {
                _logger.LogError("Application (appId: {ClientId}) not found in the current tenant.", clientId);
                throw new ServiceUnavailableException(
                    "Đã có lỗi hệ thống xảy ra, vui lòng liên hệ admin để được hỗ trợ!");
            }

            var roles = (application.AppRoles ?? [])
                .Where(r => r.IsEnabled == true && r.Id.HasValue)
                .ToList();

            _logger.LogInformation("Loaded {Count} app roles.", roles.Count);
            return roles;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to query Applications by appId.");
            throw new ServiceUnavailableException("Đã có lỗi hệ thống xảy ra, vui lòng liên hệ admin để được hỗ trợ!");
        }
    }

    public async Task<List<DirectoryRole>> GetDirectoryRolesByNamesAsync(string[] roleNames,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching directory roles by names: {RoleNames}", string.Join(", ", roleNames));

        var results = new List<DirectoryRole>();

        foreach (var name in roleNames)
        {
            try
            {
                var resp = await _graphClient.DirectoryRoles.GetAsync(q =>
                {
                    q.QueryParameters.Select = ["id", "displayName"];
                    q.QueryParameters.Filter = $"displayName eq '{name}'";
                }, cancellationToken);

                var role = resp?.Value?.FirstOrDefault();
                if (role is null)
                {
                    _logger.LogError("Required directory role '{RoleName}' is not active or not found.", name);
                    throw new InvalidOperationException($"Required directory role is not active: {name}");
                }

                results.Add(role);
                _logger.LogInformation("Found directory role: {RoleName} ({RoleId})", role.DisplayName, role.Id);
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                _logger.LogError(ex, "Failed to fetch directory role: {RoleName}", name);
                throw new ServiceUnavailableException(
                    "Đã có lỗi hệ thống xảy ra, vui lòng liên hệ admin để được hỗ trợ!");
            }
        }

        _logger.LogInformation("Successfully loaded {Count} directory roles.", results.Count);
        return results;
    }

    public async Task<ServicePrincipal> GetServicePrincipalByClientIdAsync(string clientId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching service principal for clientId: {ClientId}", clientId);

        try
        {
            var sps = await _graphClient.ServicePrincipals.GetAsync(q =>
            {
                q.QueryParameters.Filter = $"appId eq '{clientId}'";
                q.QueryParameters.Select = ["id", "appId", "displayName"];
                q.QueryParameters.Top = 1;
            }, cancellationToken);

            var sp = sps?.Value?.FirstOrDefault();
            if (sp is null)
            {
                _logger.LogError("Service principal not found for clientId: {ClientId}", clientId);
                throw new ServiceUnavailableException(
                    "Đã có lỗi hệ thống xảy ra, vui lòng liên hệ admin để được hỗ trợ!");
            }

            _logger.LogInformation("Found service principal: {SpId} for clientId: {ClientId}", sp.Id, clientId);
            return sp;
        }
        catch (Exception ex) when (ex is not ServiceUnavailableException)
        {
            _logger.LogError(ex, "Failed to fetch service principal for clientId: {ClientId}", clientId);
            throw new ServiceUnavailableException("Đã có lỗi hệ thống xảy ra, vui lòng liên hệ admin để được hỗ trợ!");
        }
    }

    public async Task<List<AppRoleAssignment>> GetUserAppRolesAsync(Guid userId, Guid? resourceId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching app role assignments for user: {UserId}, ResourceId: {ResourceId}",
            userId, resourceId?.ToString() ?? "All");

        try
        {
            var response = await _graphClient.Users[userId.ToString()].AppRoleAssignments.GetAsync(q =>
            {
                q.QueryParameters.Select = ["id", "appRoleId", "resourceId"];
                if (resourceId.HasValue)
                {
                    q.QueryParameters.Filter = $"resourceId eq {resourceId.Value}";
                }
            }, cancellationToken);

            var assignments = response?.Value ?? [];
            _logger.LogInformation("Loaded {Count} app role assignments for user {UserId}.", assignments.Count, userId);
            return assignments.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch app role assignments for user {UserId}.", userId);
            throw new ServiceUnavailableException("Đã có lỗi hệ thống xảy ra, vui lòng liên hệ admin để được hỗ trợ!");
        }
    }

    public async Task<List<DirectoryRole>> GetUserDirectoryRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching directory roles for user: {UserId}", userId);

        try
        {
            var response = await _graphClient
                .Users[userId.ToString()]
                .MemberOf
                .GraphDirectoryRole
                .GetAsync(q =>
                {
                    q.QueryParameters.Select = ["id", "displayName"];
                }, cancellationToken);

            var directoryRoles = response?.Value?.ToList() ?? [];
            _logger.LogInformation("Loaded {Count} directory roles for user {UserId}.", directoryRoles.Count, userId);

            return directoryRoles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch directory roles for user {UserId}.", userId);
            throw new ServiceUnavailableException(
                "Đã có lỗi hệ thống xảy ra, vui lòng liên hệ admin để được hỗ trợ!");
        }
    }
}
