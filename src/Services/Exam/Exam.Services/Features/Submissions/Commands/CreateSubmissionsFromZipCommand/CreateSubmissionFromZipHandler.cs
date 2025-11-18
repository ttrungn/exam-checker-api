using Exam.Services.Interfaces.Services;
using Exam.Services.Models.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Exam.Services.Features.Submissions.Commands.CreateSubmissionsFromZipCommand ;

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
        var uploadResult = await _service.CreateSubmissionsFromZipAsync(request, cancellationToken);
        if (!uploadResult.Success)
        {
            return new()
            {
                Success = false,
                Message = uploadResult.Message
            };
        }

        return new()
        {
            Success = true,
            Message = "Submissions created successfully.",
            Data = uploadResult.Data
        };
    }
}
