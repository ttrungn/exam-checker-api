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
}
