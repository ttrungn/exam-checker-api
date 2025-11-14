using Asp.Versioning;
using Exam.API.Mappers;
using Exam.Services.Features.Submission.Commands.CreateSubmissionsFromZipCommand;
using Exam.Services.Features.Submission.Queries.GetSubmissionById;
using Exam.Services.Features.Submission.Queries.GetSubmissions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Exam.API.Controllers.v1;

[ApiVersion(1)]
[ApiController]
[Route("api/v{v:apiVersion}/submissions")]
public class SubmissionsController : ControllerBase
{
    private readonly ISender _sender;
    public SubmissionsController(ISender sender)
    {
        _sender = sender;
    }
    [HttpPost]
    public async Task<IResult> CreateAsync(
        [FromForm] CreateSubmissionsFromZipCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(command, cancellationToken);
        if (!result.Success) return Results.BadRequest(result.ToDataApiResponse());

        return Results.Accepted(null, result.ToDataApiResponse());
    }
    [HttpGet]
    public async Task<IActionResult> GetSubmissions(
        [FromQuery] GetSubmissionsQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(request, cancellationToken);
        return Ok(result.ToDataApiResponse());
    }
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSubmissionById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSubmissionByIdQuery { Id = id };
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result.ToDataApiResponse());
    }
}
