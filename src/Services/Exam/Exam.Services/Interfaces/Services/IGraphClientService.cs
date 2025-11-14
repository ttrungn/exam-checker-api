using Microsoft.Graph.Models;

namespace Exam.Services.Interfaces.Services;

public interface IGraphClientService
{
    /// <summary>
    ///     Gets the application roles from Microsoft Graph
    /// </summary>
    /// <param name="clientId">The Azure AD client ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of AppRole objects from Microsoft Graph</returns>
    Task<List<AppRole>> GetAppRolesAsync(string clientId, CancellationToken cancellationToken = default);
}
