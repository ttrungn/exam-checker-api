using Asp.Versioning;
using Exam.Services.Features.Dashboard.Query.GetDashboardSummaryQuery;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Exam.API.Controllers.v1;


[ApiVersion("1")]
[ApiController]
[Route("api/v{v:apiVersion}/[controller]")]
public class DashboardController : ODataController
{
    private readonly ISender _sender;

    public DashboardController(ISender sender)
    {
        _sender = sender;
    }
    [HttpGet("summary")]
    [EnableQuery]
    public async Task<IActionResult> GetSummaryAsync()
    {
        var result = await _sender.Send(new GetDashboardSummaryQuery());
        return Ok(result);
    }
    
}
