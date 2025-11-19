using System.Security.Claims;
using Asp.Versioning;
using Domain.Constants;
using Exam.API.Mappers;
using Exam.Services.Features.Submissions.Commands.ApproveAssessment;
using Exam.Services.Features.Submissions.Commands.AssignAssessment;
using Exam.Services.Features.Submissions.Commands.CreateSubmissionsFromZipCommand;
using Exam.Services.Features.Submissions.Commands.UpdateToModeratorValidated;
using Exam.Services.Features.Submissions.Commands.UpdateToModeratorViolated;
using Exam.Services.Features.Submissions.Commands.UploadSubmissionFromZipCommand;
using Exam.Services.Features.Submissions.Queries.GetSubmissionById;
using Exam.Services.Features.Submissions.Queries.GetSubmissionByUser;
using Exam.Services.Features.Submissions.Queries.GetSubmissions;
using Exam.Services.Interfaces.Services;
using Exam.Services.Models.Requests.Submissions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Exam.API.Controllers.v1;

[ApiVersion(1)]
[ApiController]
[Route("api/v{v:apiVersion}/submissions")]
public class SubmissionsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ISubmissionService _submissionService;

    public SubmissionsController(ISender sender, ISubmissionService submissionService)
    {
        _sender = sender;
        _submissionService = submissionService;
    }

    /// <summary>
    ///     Upload zip file to blob storage for background processing
    ///     User-facing endpoint
    /// </summary>
    [HttpPost("upload")]
    [Authorize(Roles = $"{Roles.Manager}")]
    public async Task<IResult> UploadZipAsync(
        [FromForm] UploadSubmissionFromZipCommand command,
        CancellationToken cancellationToken = default)
    {
        var result =
            await _submissionService.UploadZipForProcessingAsync(command, cancellationToken);
        if (!result.Success)
        {
            return Results.BadRequest(result);
        }

        return Results.Accepted(null, result);
    }

    /// <summary>
    ///     Process zip file from blob storage and create submissions
    ///     Internal endpoint
    /// </summary>
    [HttpPost("process-from-blob")]
    public async Task<IResult> ProcessFromBlobAsync(
        [FromForm] CreateSubmissionsFromZipCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(command, cancellationToken);
        if (!result.Success)
        {
            return Results.BadRequest(result.ToDataApiResponse());
        }

        return Results.Ok(result.ToDataApiResponse());
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
    [Authorize(Roles = $"{Roles.Examiner},{Roles.Moderator},{Roles.Manager}")]
    public async Task<IActionResult> GetSubmissionById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSubmissionByIdQuery { Id = id };
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result.ToDataApiResponse());
    }

    [HttpGet("user")]
    [Authorize(Roles = $"{Roles.Examiner},{Roles.Moderator}")]
    public async Task<IActionResult> GetSubmissionsByUser(
        [FromQuery] SubmissionRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId =
            Guid.Parse(
                User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
                !);
        var query = new GetSubmissionByUserQuery()
        {
            UserId = userId,
            PageIndex = request.PageIndex,
            PageSize = request.PageSize,
            IndexFrom = request.IndexFrom,
            Role = request.Role,
            ExamCode = request.ExamCode,
            SubjectCode = request.SubjectCode,
            Status = request.Status,
            SubmissionName = request.SubmissionName,
            AssessmentStatus = request.AssessmentStatus
        };    
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result.ToDataApiResponse());
    }

    [HttpPut]
    [Route("{id:guid}/to-moderator-validated")]
    [Authorize(Roles = $"{Roles.Moderator}")]
    public async Task<IResult> UpdateToModeratorValidated(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateToModeratorValidatedCommand(id);
        var result = await _sender.Send(command, cancellationToken);
        if (result.Success)
        {
            return TypedResults.NoContent();
        }

        return Results.BadRequest(result.ToBaseApiResponse());
    }


    [HttpPut]
    [Route("{id:guid}/to-moderator-violated")]
    [Authorize(Roles = $"{Roles.Moderator}")]
    public async Task<IResult> UpdateToModeratorViolated(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateToModeratorViolatedCommand(id);
        var result = await _sender.Send(command, cancellationToken);
        if (result.Success)
        {
            return TypedResults.NoContent();
        }

        return Results.BadRequest(result.ToBaseApiResponse());
    }

    [HttpPost]
    [Route("assessments/approve")]
    [Authorize(Roles = $"{Roles.Manager}")]
    public async Task<IActionResult> ApproveAssessment(
        [FromBody] ApproveAssessmentCommand assessmentCommand,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(assessmentCommand, cancellationToken);
        return Ok(result.ToBaseApiResponse());
    }

    [HttpPost]
    [Route("assign")]
    [Authorize(Roles = $"{Roles.Manager}")]
    public async Task<IActionResult> AssignSubmission(
        [FromBody] AssignSubmissionCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result.ToBaseApiResponse());
    }
}
