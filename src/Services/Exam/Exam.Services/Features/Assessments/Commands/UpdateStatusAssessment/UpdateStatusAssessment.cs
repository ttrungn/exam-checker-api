using Exam.Domain.Entities;
using Exam.Domain.Enums;
using Exam.Repositories.Interfaces.Repositories;
using Exam.Services.Exceptions;
using Exam.Services.Models.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Exam.Services.Features.Assessments.Commands.UpdateStatusAssessment;

public record UpdateStatusAssessmentCommand : IRequest<BaseServiceResponse>
{
    public Guid Id { get; init; }
    public AssessmentStatus Status { get; init; }
}

public class UpdateStatusAssessmentCommandHandler 
    : IRequestHandler<UpdateStatusAssessmentCommand, BaseServiceResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateStatusAssessmentCommandHandler> _logger;

    public UpdateStatusAssessmentCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateStatusAssessmentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger     = logger;
    }

    public async Task<BaseServiceResponse> Handle(
        UpdateStatusAssessmentCommand request,
        CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.GetRepository<Assessment>();

        var assessment = await repo.Query()
            .Include(a => a.Submission)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (assessment == null)
        {
            _logger.LogWarning("Assessment with ID {AssessmentId} not found.", request.Id);
            throw new NotFoundException("Assessment not found.");
        }

        var oldStatus = assessment.Status;
        assessment.Status = request.Status;

        await repo.UpdateAsync(assessment, cancellationToken);

        // Check if we need to update Submission GradeStatus
        if (request.Status == AssessmentStatus.Complete && assessment.SubmissionId != Guid.Empty)
        {
            await UpdateSubmissionGradeStatusIfNeeded(assessment.SubmissionId, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Assessment with ID {AssessmentId} status updated from {OldStatus} to {NewStatus}.", 
            request.Id, oldStatus, request.Status);

        return new BaseServiceResponse
        {
            Success = true,
            Message = "Assessment status updated successfully."
        };
    }

    private async Task UpdateSubmissionGradeStatusIfNeeded(Guid submissionId, CancellationToken cancellationToken)
    {
        var submissionRepo = _unitOfWork.GetRepository<Domain.Entities.Submission>();
        
        var submission = await submissionRepo.Query()
            .Include(s => s.Assessments)
            .FirstOrDefaultAsync(s => s.Id == submissionId, cancellationToken);

        if (submission == null)
        {
            _logger.LogWarning("Submission with ID {SubmissionId} not found.", submissionId);
            return;
        }

        // Count total assessments for this submission
        var totalAssessmentsCount = submission.Assessments.Count;

        _logger.LogInformation(
            "Submission {SubmissionId} has {TotalCount} assessment(s).", 
            submissionId, totalAssessmentsCount);

        // Only auto-update GradeStatus if submission has exactly 1 assessment
        if (totalAssessmentsCount == 1)
        {
            if (submission.GradeStatus != GradeStatus.Graded)
            {
                submission.GradeStatus = GradeStatus.Graded;
                await submissionRepo.UpdateAsync(submission, cancellationToken);
                
                _logger.LogInformation(
                    "Submission {SubmissionId} GradeStatus updated to Graded (has 1 assessment and it's complete).", 
                    submissionId);
            }
        }
        // If submission has >= 2 assessments, don't auto-update GradeStatus
        else if (totalAssessmentsCount >= 2)
        {
            _logger.LogInformation(
                "Submission {SubmissionId} has {Count} assessments. GradeStatus will not be auto-updated (requires manual review).", 
                submissionId, totalAssessmentsCount);
        }
    }
}
