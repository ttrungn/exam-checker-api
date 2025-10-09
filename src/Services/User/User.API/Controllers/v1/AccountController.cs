using Asp.Versioning;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using User.API.Mappers;
using User.Services.Features.Account.Commands.CreateAnAccount;

namespace User.API.Controllers.v1;

[ApiVersion(1)]
[ApiController]
[Route("api/v{v:apiVersion}/accounts")]
public class AccountController : ControllerBase
{
    private readonly ISender _sender;

    public AccountController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> CreateAnAccountAsync([FromBody] CreateAnAccountCommand command)
    {
        var result = await _sender.Send(command);
        if (!result.Success)
        {
            return BadRequest(result.ToBaseApiResponse());
        }

        return Ok(result.ToBaseApiResponse());
    }
}
