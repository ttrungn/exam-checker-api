using Exam.Repositories.Interfaces.Repositories;
using Exam.Services.Exceptions;
using Exam.Services.Features.Submissions.Queries.GetSubmissions;
using Exam.Services.Mappers;
using Exam.Services.Models.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace Exam.Services.Features.Submissions.Queries.GetSubmissionById;

public record GetSubmissionByIdQuery : IRequest<DataServiceResponse<SubmissionItemDto>>
{
    public Guid Id { get; init; }
}

public class GetSubmissionByIdHandler
    : IRequestHandler<GetSubmissionByIdQuery, DataServiceResponse<SubmissionItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetSubmissionByIdHandler> _logger;
    private readonly GraphServiceClient _graphClient;

    public GetSubmissionByIdHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetSubmissionByIdHandler> logger,
        GraphServiceClient graphClient)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _graphClient = graphClient;
    }

    public async Task<DataServiceResponse<SubmissionItemDto>> Handle(
        GetSubmissionByIdQuery request,
        CancellationToken ct)
    {
        _logger.LogInformation("GetSubmissionById invoked. Id={Id}", request.Id);

        try
        {
            var repository = _unitOfWork.GetRepository<Domain.Entities.Submission>();

            var submission = await repository.Query()
                .Include(s => s.ExamSubject)
                    .ThenInclude(es => es!.Exam)
                .Include(s => s.ExamSubject)
                    .ThenInclude(es => es!.Subject)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == request.Id, ct);

            if (submission == null)
            {
                _logger.LogWarning("Submission with Id={Id} not found", request.Id);
                throw new NotFoundException($"Không tìm thấy submission với ID: {request.Id}");
            }

            // Use mapper to convert entity to DTO
            var dto = submission.ToManagerSubmissionDto();

            // Fetch Examiner email from Microsoft Graph
            if (submission.ExaminerId.HasValue && submission.ExaminerId.Value != Guid.Empty)
            {
                try
                {
                    var examiner = await _graphClient.Users[submission.ExaminerId.Value.ToString()]
                        .GetAsync(r =>
                        {
                            r.QueryParameters.Select = ["id", "mail", "userPrincipalName"];
                        }, ct);

                    if (examiner != null)
                    {
                        dto.ExaminerEmail = examiner.Mail ?? examiner.UserPrincipalName;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch examiner info for ID={ExaminerId}", submission.ExaminerId);
                }
            }

            // Fetch Moderator email from Microsoft Graph
            if (submission.ModeratorId.HasValue && submission.ModeratorId.Value != Guid.Empty)
            {
                try
                {
                    var moderator = await _graphClient.Users[submission.ModeratorId.Value.ToString()]
                        .GetAsync(r =>
                        {
                            r.QueryParameters.Select = ["id", "mail", "userPrincipalName"];
                        }, ct);

                    if (moderator != null)
                    {
                        dto.ModeratorEmail = moderator.Mail ?? moderator.UserPrincipalName;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch moderator info for ID={ModeratorId}", submission.ModeratorId);
                }
            }

            return new DataServiceResponse<SubmissionItemDto>
            {
                Success = true,
                Message = "Lấy thông tin submission thành công",
                Data = dto
            };
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetSubmissionById failed for Id={Id}", request.Id);
            throw new ServiceUnavailableException("Đã có lỗi hệ thống xảy ra, vui lòng liên hệ admin để được hỗ trợ!");
        }
    }
}
