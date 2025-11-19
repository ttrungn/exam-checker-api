using Domain.Constants;
using Exam.Domain.Entities;
using Exam.Domain.Enums;
using Exam.Repositories.Interfaces.Repositories;
using Exam.Services.Exceptions;
using Exam.Services.Interfaces.Services;
using Exam.Services.Models.Responses;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Exam.Services.Features.Submissions.Commands.AssignAssessment;

public class AssignSubmissionCommand : IRequest<BaseServiceResponse>
{
    public Guid SubmissionId { get; set; }
    public Guid ExaminerId { get; set; }
}

public class AssignSubmissionValidator : AbstractValidator<AssignSubmissionCommand>
{
    public AssignSubmissionValidator()
    {
        RuleFor(x => x.SubmissionId).NotEmpty();
        RuleFor(x => x.ExaminerId).NotEmpty();
    }
}

public class AssignSubmissionHandler : IRequestHandler<AssignSubmissionCommand, BaseServiceResponse>
{
    private readonly IConfiguration _configuration;
    private readonly IGraphClientService _graphClientService;
    private readonly ILogger<AssignSubmissionHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public AssignSubmissionHandler(
        IConfiguration configuration,
        IGraphClientService graphClientService,
        ILogger<AssignSubmissionHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _configuration = configuration;
        _graphClientService = graphClientService;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseServiceResponse> Handle(AssignSubmissionCommand request, CancellationToken cancellationToken)
    {
        var clientId = _configuration["AzureAd:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
        {
            _logger.LogWarning("Missing AzureAd:ClientId configuration");
            throw new ServiceUnavailableException("Đã có lỗi hệ thống xảy ra, vui lòng liên hệ admin để được hỗ trợ!");
        }

        var userRoles = await _graphClientService.GetUserAppRolesForApplicationAsync(
            request.ExaminerId,
            clientId,
            cancellationToken);

        if (!userRoles.Any(r =>
                !string.IsNullOrEmpty(r.Value) &&
                string.Equals(r.Value, Roles.Examiner, StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogWarning("User {UserId} is not an examiner", request.ExaminerId);
            throw new InvalidOperationException("Người bạn chọn không phải là người chấm bài!");
        }

        var submissionRepo = _unitOfWork.GetRepository<Submission>();

        var submission = await submissionRepo.GetFirstOrDefaultAsync(
            predicate: x => x.Id == request.SubmissionId,
            disableTracking: false
        );

        if (submission == null)
        {
            _logger.LogError("Submission {SubmissionId} not found", request.SubmissionId);
            throw new NotFoundException($"Không tìm thấy bài nộp với id: {request.SubmissionId}");
        }

        submission.GradeStatus = GradeStatus.ReAssigned;

        if (string.IsNullOrWhiteSpace(submission.FileUrl))
        {
            _logger.LogError("Submission {SubmissionId} has no file url", request.SubmissionId);
            throw new InvalidDataException($"Không tìm thấy đường dẫn file của bài nộp với id: {request.SubmissionId}");
        }

        var topLevel = Path.GetFileNameWithoutExtension(submission.FileUrl);

        if (string.IsNullOrWhiteSpace(topLevel))
        {
            _logger.LogError("Submission {SubmissionId} has no file url", request.SubmissionId);
            throw new InvalidDataException($"Không tìm thấy tên file gốc của bài nộp với id: {request.SubmissionId}");
        }

        var assessmentRepo = _unitOfWork.GetRepository<Assessment>();

        var newAssessment = new Assessment
        {
            SubmissionId = submission.Id,
            ExaminerId = request.ExaminerId,
            StudentCode = topLevel,
            SubmissionName = topLevel,
            Status = AssessmentStatus.Pending
        };

        assessmentRepo.Insert(newAssessment);
        await _unitOfWork.SaveChangesAsync();

        return new BaseServiceResponse { Success = true, Message = "Đã giao bài cho người chấm mới thành công!" };
    }
}
