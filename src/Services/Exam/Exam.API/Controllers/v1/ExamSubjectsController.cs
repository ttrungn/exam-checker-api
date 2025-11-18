using Asp.Versioning;
using Exam.API.Mappers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Exam.Services.Models.Validations;
using Exam.Services.Models.Responses;
using Exam.Services.Features.ExamSubjects.Commands.UpdateViolationStructure;

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
    
    
}
