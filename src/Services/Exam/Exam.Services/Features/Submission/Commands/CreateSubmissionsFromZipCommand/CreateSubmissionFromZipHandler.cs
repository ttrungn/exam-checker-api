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
        var uploadResult = await _service.UploadZipForProcessingAsync(request, cancellationToken);
        if (!uploadResult.Success)
        {
            return new()
            {
                Success = false,
                Message = uploadResult.Message
            };
        }

        // vì xử lý async bằng Function, ở đây chưa có list Guid
        // tuỳ bạn: trả về Data = null hoặc đổi sang response type khác
        return new()
        {
            Success = true,
            Message = "File đã upload, submissions sẽ được tạo bởi background Function.",
            Data    = new List<Guid>() // hoặc null
        };
    }
}
