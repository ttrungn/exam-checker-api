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

    /// <summary>
    ///     Gets directory roles by their display names from Microsoft Graph
    /// </summary>
    /// <param name="roleNames">Array of directory role display names to fetch</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of DirectoryRole objects matching the specified names</returns>
    /// <exception cref="InvalidOperationException">Thrown when a required directory role is not found or not active</exception>
    Task<List<DirectoryRole>> GetDirectoryRolesByNamesAsync(string[] roleNames,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets service principal by client ID
    /// </summary>
    /// <param name="clientId">The Azure AD client ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>ServicePrincipal object matching the client ID</returns>
    Task<ServicePrincipal> GetServicePrincipalByClientIdAsync(string clientId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets app role assignments for a specific user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="resourceId">Optional resource service principal ID to filter assignments</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of AppRoleAssignment objects for the user</returns>
    Task<List<AppRoleAssignment>> GetUserAppRolesAsync(Guid userId, Guid? resourceId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets directory roles assigned to a specific user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of DirectoryRole objects assigned to the user</returns>
    Task<List<DirectoryRole>> GetUserDirectoryRolesAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets app role assignments for a specific user for a specific application
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="clientId">The Azure AD client ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of AppRole objects assigned to the user for the application</returns>
    Task<List<AppRole>> GetUserAppRolesForApplicationAsync(
        Guid userId,
        string clientId,
        CancellationToken cancellationToken = default);
}
