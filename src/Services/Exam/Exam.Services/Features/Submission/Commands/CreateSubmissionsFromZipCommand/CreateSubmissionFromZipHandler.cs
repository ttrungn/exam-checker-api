using Exam.Services.Interfaces.Services;
using Exam.Services.Models.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Exam.Services.Features.Submission.Commands.CreateSubmissionsFromZipCommand ;

public class CreateSubmissionFromZipHandler : IRequestHandler<CreateSubmissionsFromZipCommand , DataServiceResponse<List<Guid>>>
{
    private readonly ISubmissionService _service;
    private readonly ILogger<CreateSubmissionFromZipHandler> _logger;
    
    public CreateSubmissionFromZipHandler(ISubmissionService service, ILogger<CreateSubmissionFromZipHandler> logger)
    {
        _service = service;
        _logger = logger;
    }
    
    public async Task<DataServiceResponse<List<Guid>>> Handle(CreateSubmissionsFromZipCommand  request, CancellationToken cancellationToken)
    {
        var response = await _service.CreateSubmissionsFromZipAsync(request, cancellationToken);
        if (!response.Success)
        {
            _logger.LogError("Create submissions from zip failed: {Message}", response.Message);
            return response;
        }
        _logger.LogInformation("Created {Count} submissions from zip", response.Data);
        return response;
    }
}
