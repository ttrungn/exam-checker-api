using Exam.Domain.Entities;
using Exam.Domain.Enums;
using Exam.Repositories.Interfaces.Repositories;
using Exam.Services.Exceptions;
using Exam.Services.Models.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Exam.Services.Features.Submissions.Commands.ApproveAssessment;

public class ApproveAssessmentCommand : IRequest<BaseServiceResponse>
{
    public Guid SubmissionId { get; set; }
    public Guid AssessmentId { get; set; }
}

public class ApproveAssessmentHandler : IRequestHandler<ApproveAssessmentCommand, BaseServiceResponse>
{
    private readonly ILogger<ApproveAssessmentHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public ApproveAssessmentHandler(
        ILogger<ApproveAssessmentHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseServiceResponse> Handle(ApproveAssessmentCommand request, CancellationToken cancellationToken)
    {
        var submissionRepo = _unitOfWork.GetRepository<Submission>();

        var submission = await submissionRepo.GetFirstOrDefaultAsync(
            predicate: s => s.Id == request.SubmissionId,
            include: s => s.Include(s => s.Assessments),
            disableTracking: false);

        if (submission == null)
        {
            _logger.LogError("Submission {SubmissionId} not found", request.SubmissionId);
            throw new NotFoundException($"Không tìm thấy bài nộp với id: {request.SubmissionId}");
        }

        if (submission.Assessments.Count == 0)
        {
            _logger.LogError("Submission {SubmissionId} has no assessments", request.SubmissionId);
            throw new NotFoundException(
                $"Bài nộp với id {request.SubmissionId} chưa có bài chấm nào.");
        }

        var targetAssessment = submission.Assessments
            .FirstOrDefault(a => a.Id == request.AssessmentId);

        if (targetAssessment == null)
        {
            _logger.LogError(
                "Assessment {AssessmentId} not found in submission {SubmissionId}",
                request.AssessmentId, request.SubmissionId);

            throw new NotFoundException(
                $"Không tìm thấy bài chấm với id: {request.AssessmentId}");
        }

        if (targetAssessment.Status != AssessmentStatus.Complete)
        {
            _logger.LogError(
                "Assessment {AssessmentId} is not completed. Current status: {Status}",
                request.AssessmentId, targetAssessment.Status);

            throw new InvalidOperationException(
                "Chỉ có thể phê duyệt bài chấm đã hoàn thành.");
        }

        // Mark submission as approved (chosen assessment is considered the final one)
        submission.GradeStatus = GradeStatus.Approved;

        // Cancel all other assessments
        foreach (var assessment in submission.Assessments)
        {
            if (assessment.Id != request.AssessmentId)
            {
                assessment.Status = AssessmentStatus.Cancelled;
            }
        }

        await _unitOfWork.SaveChangesAsync();

        return new BaseServiceResponse
        {
            Success = true, Message = "Bài chấm đã được phê duyệt thành công và đã huỷ các bài chấm khác!"
        };
    }
}
