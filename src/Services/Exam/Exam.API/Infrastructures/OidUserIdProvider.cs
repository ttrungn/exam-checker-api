using Microsoft.AspNetCore.SignalR;

namespace Exam.API.Infrastructures;

public class OidUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User?.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
    }
}
