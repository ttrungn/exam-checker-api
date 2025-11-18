using Asp.Versioning;
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
    

}
