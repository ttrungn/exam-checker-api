using Microsoft.AspNetCore.SignalR;

namespace Exam.API.Hubs;

public class ExamCheckerNotificationsHub : Hub
{
    private readonly ILogger<ExamCheckerNotificationsHub> _logger;

    public ExamCheckerNotificationsHub(ILogger<ExamCheckerNotificationsHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;
        var userId = Context.UserIdentifier;
        var httpContext = Context.GetHttpContext();
        var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();
        var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();

        _logger.LogInformation(
            "SignalR Client Connected - ConnectionId: {ConnectionId}, UserId: {UserId}, IP: {IpAddress}, UserAgent: {UserAgent}",
            connectionId, userId ?? "Anonymous", ipAddress, userAgent);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        var userId = Context.UserIdentifier;

        if (exception != null)
        {
            _logger.LogWarning(exception,
                "SignalR Client Disconnected with Error - ConnectionId: {ConnectionId}, UserId: {UserId}",
                connectionId, userId ?? "Anonymous");
        }
        else
        {
            _logger.LogInformation(
                "SignalR Client Disconnected - ConnectionId: {ConnectionId}, UserId: {UserId}",
                connectionId, userId ?? "Anonymous");
        }

        await base.OnDisconnectedAsync(exception);
    }
}
