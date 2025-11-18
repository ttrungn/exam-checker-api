using Asp.Versioning;
using Exam.API.Mappers;
using Exam.Domain.Enums;
using Exam.Services.Features.Assessments.Commands.GradeAssessment;
using Exam.Services.Features.Assessments.Commands.UpdateStatusAssessment;
using Exam.Services.Features.Assessments.Queries;
using Exam.Services.Models.Requests.Assessments;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Exam.API.Controllers.v1;

[ApiVersion("1")]
[ApiController]
[Route("api/v{v:apiVersion}/[controller]")]
public class AssessmentsController : ControllerBase
{
    private readonly ISender _sender;

    public AssessmentsController(ISender sender)
    {
        _sender = sender;
    }

    // Grade an assessment
    [HttpPut("{id:guid}/grade")]
    public async Task<IResult> GradeAsync(
        [FromRoute] Guid id,
        [FromBody] GradeAssessmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new GradeAssessmentCommand
        {
            AssessmentId = id, ScoreDetail  = request.ScoreDetail, Comment      = request.Comment
        };

        var result = await _sender.Send(command, cancellationToken);

        if (!result.Success)
            return Results.BadRequest(result.ToBaseApiResponse());

        return Results.Ok(result.ToBaseApiResponse());
    }
    
    // Get assessment detail
    [HttpGet("{id:guid}")]
    public async Task<IResult> GetDetailAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAssessmentDetailQuery { Id = id };
        var result = await _sender.Send(query, cancellationToken);

        if (!result.Success)
            return Results.BadRequest(result.ToBaseApiResponse());

        return Results.Ok(result.ToDataApiResponse());
    }
    
    // Complete an assessment
    [HttpPut("{id:guid}/complete")]
    public async Task<IResult> CompleteGradeAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new UpdateStatusAssessmentCommand() { Id = id, Status = AssessmentStatus.Complete};
        var result = await _sender.Send(query, cancellationToken);

        if (!result.Success)
        {
            return TypedResults.BadRequest(result.ToBaseApiResponse());
        }
        return TypedResults.NoContent();
    }
    
    // Cancel an assessment
    [HttpPut("{id:guid}/cancel")]
    public async Task<IResult> CancelGradeAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new UpdateStatusAssessmentCommand() { Id = id, Status = AssessmentStatus.Cancelled};
        var result = await _sender.Send(query, cancellationToken);

        if (!result.Success)
        {
            return TypedResults.BadRequest(result.ToBaseApiResponse());
        }
        return TypedResults.NoContent();
    }
    
}
