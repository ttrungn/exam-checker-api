using System.Security.Claims;
using Asp.Versioning;
using Domain.Constants;
using Exam.API.Hubs;
using Exam.API.Mappers;
using Exam.Services.Features.Account.Commands.AssignARoleToAnAccount;
using Exam.Services.Features.Account.Commands.CreateAnAccount;
using Exam.Services.Features.Account.Queries.GetAppRoles;
using Exam.Services.Features.Account.Queries.GetExaminers;
using Exam.Services.Features.Account.Queries.GetModerators;
using Exam.Services.Features.Account.Queries.GetUserProfile;
using Exam.Services.Features.Account.Queries.GetUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Exam.API.Controllers.v1;

[ApiVersion(1)]
[ApiController]
[Route("api/v{v:apiVersion}/accounts")]
public class AccountController : ControllerBase
{
    private readonly IHubContext<AccountNotificationsHub> _accountNotificationHub;
    private readonly ISender _sender;

    public AccountController(ISender sender, IHubContext<AccountNotificationsHub> accountNotificationHub)
    {
        _sender = sender;
        _accountNotificationHub = accountNotificationHub;
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> CreateAnAccountAsync([FromBody] CreateAnAccountCommand command)
    {
        var result = await _sender.Send(command);
        return Ok(result.ToBaseApiResponse());
    }

    [HttpPost("{userId:guid}/roles/{appRoleId:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> AssignARoleToAnAccountAsync(Guid userId, Guid appRoleId)
    {
        var command = new AssignARoleToAnAccountCommand() { UserId = userId, AppRoleId = appRoleId };
        var result = await _sender.Send(command);
        await _accountNotificationHub.Clients.User(userId.ToString())
            .SendAsync("AccountUpdated", new { userId = userId.ToString() });
        return Ok(result.ToBaseApiResponse());
    }

    [HttpGet("{userId:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetUserProfileById(Guid userId)
    {
        var query = new GetUserProfileQuery() { UserId = userId };
        var result = await _sender.Send(query);
        return Ok(result.ToDataApiResponse());
    }

    [HttpGet]
    [Route("roles")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetAppRoles([FromQuery] GetAppRolesQuery request)
    {
        var result = await _sender.Send(request);
        return Ok(result.ToDataApiResponse());
    }

    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsersQuery request)
    {
        var result = await _sender.Send(request);
        return Ok(result.ToDataApiResponse());
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetUserProfile()
    {
        var userId = Guid.Parse(User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")!);
        var query = new GetUserProfileQuery() { UserId = userId };
        var result = await _sender.Send(query);
        return Ok(result.ToDataApiResponse());
    }

    [HttpGet("examiners")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.Moderator}")]
    public async Task<IActionResult> GetExaminers([FromQuery] GetExaminersQuery request)
    {
        var result = await _sender.Send(request);
        return Ok(result.ToDataApiResponse());
    }
    [HttpGet("moderators")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager},{Roles.Moderator}")]
    public async Task<IActionResult> GetModerator([FromQuery] GetModeratorsQuery request)
    {
        var result = await _sender.Send(request);
        return Ok(result.ToDataApiResponse());
    }
}
