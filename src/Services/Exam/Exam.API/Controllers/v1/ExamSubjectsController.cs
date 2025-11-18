using Asp.Versioning;
using Exam.API.Mappers;
using Exam.Services.Features.ExamSubjects.Commands;
using Exam.Services.Models.Requests.ExamSubjects;
using Exam.Services.Models.Responses;
using Exam.Services.Models.ScoreStructureJson.ExamSubjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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
    [HttpPost("{id:guid}/score-structure/import")]
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
}
