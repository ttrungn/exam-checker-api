using Asp.Versioning;
using Exam.API.Mappers;
using Exam.Services.Features.Subjects.Commands.CreateSubject;
using Exam.Services.Features.Subjects.Commands.DeleteSubject;
using Exam.Services.Features.Subjects.Commands.UpdateSubject;
using Exam.Services.Features.Subjects.Queries.GetSubjectById;
using Exam.Services.Mappers;
using Exam.Services.Models.Requests.Subjects;
using Exam.Services.Models.Responses;
using Exam.Services.Models.Responses.Subjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Exam.API.Controllers.v1;

[ApiVersion("1")]
[ApiController]
[Route("api/v{v:apiVersion}/[controller]")]
public class SubjectsController : ControllerBase
{
    private readonly ISender _sender;

    public SubjectsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IResult> CreateAsync(
        CreateSubjectCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(command, cancellationToken);
        if (result.Success && result is DataServiceResponse<Guid> dataResponse)
        {
            return TypedResults.Created($"/api/v1/subjects/{dataResponse.Data}",
                dataResponse.ToDataApiResponse());
        }

        return TypedResults.BadRequest(result.ToBaseApiResponse());
    }

    [HttpPut("{id:guid}")]
    public async Task<IResult> UpdateAsync(
        [FromRoute] Guid id,
        [FromBody] SubjectRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateSubjectCommand()
        {
            Id = id, Name = request.Name, Code = request.Code, SemesterId = request.SemesterId
        };
        var result = await _sender.Send(command, cancellationToken);
        if (result.Success)
        {
            return TypedResults.NoContent();
        }

        return TypedResults.BadRequest(result.ToBaseApiResponse());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IResult> DeleteAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteSubjectCommand(id);
        var result = await _sender.Send(command, cancellationToken);
        if (result.Success)
        {
            return TypedResults.NoContent();
        }

        return TypedResults.BadRequest(result.ToBaseApiResponse());
    }

    [HttpGet("{id:guid}")]
    public async Task<IResult> GetByIdAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSubjectByIdQuery(id);
        var result = await _sender.Send(query, cancellationToken);
        if (result.Success && result is DataServiceResponse<SubjectResponse> dataResponse)
        {
            return TypedResults.Ok(dataResponse.ToDataApiResponse());
        }

        return TypedResults.BadRequest(result.ToBaseApiResponse());
    }

    [HttpGet]
    public async Task<IResult> GetAsync(
        [FromQuery] SubjectGetRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(request.ToGetSubjectsQuery(), cancellationToken);
        if (result.Success && result is PaginationServiceResponse<SubjectResponse> paginationResponse)
        {
            return TypedResults.Ok(paginationResponse.ToPaginationApiResponse());
        }

        return TypedResults.BadRequest(result.ToBaseApiResponse());
    }
}
