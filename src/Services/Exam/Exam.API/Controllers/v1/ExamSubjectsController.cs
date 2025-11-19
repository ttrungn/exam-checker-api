using Asp.Versioning;
using Domain.Constants;
using Exam.API.Mappers;
using Exam.Services.Features.ExamSubjects.Commands;
using Exam.Services.Models.Requests.ExamSubjects;
using Exam.Services.Models.Responses;
using Exam.Services.Models.Responses.ExamSubjects;
using Exam.Services.Models.ScoreStructureJson.ExamSubjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Exam.Services.Models.Validations;
using Exam.Services.Features.ExamSubjects.Commands.UpdateViolationStructure;
using Exam.Services.Features.ExamSubjects.Queries.GetExamSubjectById;
using Exam.Services.Features.ExamSubjects.Queries.GetExamSubjects;
using Microsoft.AspNetCore.Authorization;

namespace Exam.API.Controllers.v1;

[ApiVersion("1")]
[ApiController]
[Route("api/v{v:apiVersion}/[controller]")]
public class ExamSubjectsController : ControllerBase
{
    private readonly ISender _sender;

    public ExamSubjectsController(ISender sender)
    {
        _sender = sender;
    }
    
    [HttpPut("{examSubjectId:guid}/violation-structure")]
    public async Task<IResult> UpdateViolationStructureAsync(
        [FromRoute] Guid examSubjectId,
        [FromBody] ValidationRules rules,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateViolationStructureCommand
        {
            ExamSubjectId = examSubjectId,
            Rules = rules
        };
        var result = await _sender.Send(command, cancellationToken);
        if (result.Success)
        {
            return TypedResults.NoContent();
        }

        return TypedResults.BadRequest(result.ToBaseApiResponse());
    }
    [HttpPost("{id:guid}/score-structure/import")]
    [Authorize(Roles = $"{Roles.Manager}")]
    public async Task<IResult> ImportScoreStructureAsync(
        [FromRoute] Guid id,
        [FromForm] ImportScoreStructureRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new ImportScoreStructureFromExcelCommand
        {
            ExamSubjectId = id,
            File = request.File
        };

        var result = await _sender.Send(command, cancellationToken);

        if (result.Success && result is DataServiceResponse<ScoreStructure> dataResponse)
        {
            return TypedResults.Ok(dataResponse.ToDataApiResponse());
        }

        return TypedResults.BadRequest(result.ToBaseApiResponse());
    }

    [HttpGet]
    [Authorize(Roles = $"{Roles.Manager},{Roles.Admin}")]
    public async Task<IResult> GetExamSubjectsAsync(
        [FromQuery] GetExamSubjectsQuery request,
        CancellationToken token = default)
    {
        var result = await _sender.Send(request, token);

        if (result.Success && result is PaginationServiceResponse<ExamSubjectResponse> paginationResponse)
        {
            return TypedResults.Ok(paginationResponse.ToPaginationApiResponse());
        }
        return TypedResults.BadRequest(result.ToBaseApiResponse());
    }

    [HttpGet("{examSubjectId:guid}")]
    public async Task<IResult> GetExamSubjectByIdAsync(
        [FromRoute] Guid examSubjectId,
        CancellationToken token = default
    )
    {
        var query = new GetExamSubjectByIdWithViolationStructureQuery(examSubjectId);
        var result = await _sender.Send(query, token);

        if (result.Success && result is DataServiceResponse<ExamSubjectResponse> dataResponse)
        {
            return TypedResults.Ok(dataResponse.ToDataApiResponse());
        }
        return TypedResults.BadRequest(result.ToBaseApiResponse());
    }
    

}
