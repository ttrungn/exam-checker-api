using Asp.Versioning;
using Domain.Constants;
using Exam.API.Mappers;
using Exam.Services.Features.Exams.Commands.CreateExam;
using Exam.Services.Features.Exams.Commands.DeleteExam;
using Exam.Services.Features.Exams.Commands.UpdateExam;
using Exam.Services.Features.Exams.Queries.GetExamById;
using Exam.Services.Mappers;
using Exam.Services.Models.Requests.Exams;
using Exam.Services.Models.Responses;
using Exam.Services.Models.Responses.Exams;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Exam.API.Controllers.v1;

[ApiVersion("1")]
[ApiController]
[Route("api/v{v:apiVersion}/[controller]")]
public class ExamsController : ControllerBase
{
    private readonly ISender _sender;

    public ExamsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin}")]
    public async Task<IResult> CreateAsync(
        [FromBody] CreateExamCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(command, cancellationToken);
        if (result.Success && result is DataServiceResponse<Guid> dataResponse)
        {
            return TypedResults.Created($"/api/v1/exams/{dataResponse.Data}",
                dataResponse.ToDataApiResponse());
        }

        return TypedResults.BadRequest(result.ToBaseApiResponse());
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{Roles.Admin}")]
    public async Task<IResult> UpdateAsync(
        [FromRoute] Guid id,
        [FromBody] CreateExamCommand request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateExamCommand()
        {
            Id = id,
            SemesterId = request.SemesterId,
            Code = request.Code,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };
        var result = await _sender.Send(command, cancellationToken);
        if (result.Success)
        {
            return TypedResults.NoContent();
        }

        return TypedResults.BadRequest(result.ToBaseApiResponse());
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = $"{Roles.Admin}")]
    public async Task<IResult> DeleteAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteExamCommand(id);
        var result = await _sender.Send(command, cancellationToken);
        if (result.Success)
        {
            return TypedResults.NoContent();
        }

        return TypedResults.BadRequest(result.ToBaseApiResponse());
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = $"{Roles.Admin}")]
    public async Task<IResult> GetByIdAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetExamByIdQuery(id);
        var result = await _sender.Send(query, cancellationToken);
        if (result.Success && result is DataServiceResponse<ExamResponse> dataResponse)
        {
            return TypedResults.Ok(dataResponse.ToDataApiResponse());
        }

        return TypedResults.BadRequest(result.ToBaseApiResponse());
    }

    [HttpGet]
    [Authorize(Roles = $"{Roles.Admin}")]
    public async Task<IResult> GetExamsAsync(
        [FromQuery] ExamGetRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(request.ToGetExamsQuery(), cancellationToken);
        if (result.Success && result is PaginationServiceResponse<ExamResponse> dataResponse)
        {
            return TypedResults.Ok(dataResponse.ToPaginationApiResponse());
        }

        return TypedResults.BadRequest(result.ToBaseApiResponse());
    }
}
