using Asp.Versioning;
using Exam.Services.Features.Violation.Commands.SaveViolationAndUpdateSubmission;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Exam.API.Controllers.v1;

[ApiVersion(1)]
[ApiController]
[Route("api/v{v:apiVersion}/violations")]
public class ViolationController : ControllerBase
{
    private readonly ISender _sender;

    public ViolationController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("save")]
    public async Task<IResult> SaveViolationsAsync(
        SaveViolationAndUpdateSubmissionCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(command, cancellationToken);
        if (result.Success)
        {
            return TypedResults.Ok(result);
        }
        return TypedResults.BadRequest(result);
    }
}
