using Asp.Versioning;
using Exam.API.Mappers;
using Exam.Services.Features.Semesters.Commands.CreateSemester;
using Exam.Services.Features.Semesters.Commands.DeleteSemester;
using Exam.Services.Features.Semesters.Commands.UpdateSemester;
using Exam.Services.Features.Semesters.Queries.GetSemesterById;
using Exam.Services.Features.Semesters.Queries.GetSemesters;
using Exam.Services.Models.Requests.Semesters;
using Exam.Services.Models.Responses;
using Exam.Services.Models.Responses.Semesters;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Exam.API.Controllers.v1;

[ApiVersion(1)]
[ApiController]
[Route("api/v{v:apiVersion}/[controller]")]
public class SemestersController : ControllerBase
{
    private readonly ISender _sender;

    public SemestersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IResult> CreateAsync(
        CreateSemesterCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(command, cancellationToken);
        if (result.Success && result is DataServiceResponse<Guid> dataResponse)
        {
            return TypedResults.Created($"/api/v1/semesters/{dataResponse.Data}", dataResponse.ToDataApiResponse());
        }
        return TypedResults.BadRequest(result.ToBaseApiResponse());
    }

    [HttpPut("{id:guid}")]
    public async Task<IResult> UpdateAsync(
        [FromRoute] Guid id,
        [FromBody] SemesterRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateSemesterCommand() { Id = id, Name = request.Name };
        var result = await _sender.Send(command, cancellationToken);
        if (result.Success)
        {
            return TypedResults.NoContent();
        }
        return TypedResults.NotFound(result.ToBaseApiResponse());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IResult> DeleteAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteSemesterCommand(id);
        var result = await _sender.Send(command, cancellationToken);
        if (result.Success)
        {
            return TypedResults.NoContent();
        }
        return TypedResults.NotFound(result.ToBaseApiResponse());
    }

    [HttpGet("{id:guid}")]
    public async Task<IResult> GetByIdAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSemesterByIdQuery(id);
        var result = await _sender.Send(query, cancellationToken);
        if (result.Success && result is DataServiceResponse<SemesterResponse> dataResponse)
        {
            return TypedResults.Ok(dataResponse.ToDataApiResponse());
        }
        return TypedResults.NotFound(result.ToBaseApiResponse());
    }

    [HttpGet]
    public async Task<IResult> GetSemestersAsync(
        [FromQuery] SemesterGetRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSemestersQuery()
        {
            Name = request.Name,
            IsActive = request.IsActive,
            PageIndex = request.PageIndex ?? 1,
            PageSize = request.PageSize ?? 8
        };
        var result = await _sender.Send(query, cancellationToken);
        if (result.Success && result is PaginationServiceResponse<SemesterResponse> paginationResponse)
        {
            return TypedResults.Ok(paginationResponse.ToPaginationApiResponse());
        }
        return TypedResults.BadRequest(result.ToBaseApiResponse());
    }
}
