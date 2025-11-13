using Asp.Versioning;
using Exam.API.Mappers;
using Exam.Services.Features.Submission.Commands.CreateSubmissionsFromZipCommand;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Exam.API.Controllers.v1;

[ApiVersion(1)]
[ApiController]
[Route("api/v{v:apiVersion}/submissions")]
public class SubmissionController : ControllerBase
{
    private readonly ISender _sender;
    public SubmissionController(ISender sender)
    {
        _sender = sender;
    }
    [HttpPost]
    public async Task<IResult> CreateAsync([FromForm] CreateSubmissionsFromZipCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(command, cancellationToken);
        if (!result.Success) return Results.BadRequest(result.ToDataApiResponse());
        var ver = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1";
        var ids = string.Join(",", result.Data);
        var location = $"/api/v{ver}/submissions?ids={ids}";

        return Results.Created(location, result.ToDataApiResponse());
    }
}
