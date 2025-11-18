using Exam.Domain.Entities;
using Exam.Services.Features.Submissions.Queries.GetSubmissionByUser;
using Exam.Services.Features.Submissions.Queries.GetSubmissions;

namespace Exam.Services.Mappers;

public static class SubmissionMapper
{
    // Dùng cho màn hình manager: xem 1 submission + list tất cả assessments
    public static SubmissionItemDto ToManagerSubmissionDto(
        this Submission submission)
    {
        var dto = new SubmissionItemDto
        {
            Id            = submission.Id,
            ExamSubjectId = submission.ExamSubjectId,
            ExamId        = submission.ExamSubject?.ExamId ?? Guid.Empty,
            ExamCode      = submission.ExamSubject?.Exam?.Code,
            SubjectId     = submission.ExamSubject?.SubjectId ?? Guid.Empty,
            SubjectIdCode = submission.ExamSubject?.Subject?.Code,
            ExaminerId    = submission.ExaminerId,
            ExaminerEmail = null, // sẽ fill sau bằng Graph
            ModeratorId   = submission.ModeratorId,
            ModeratorEmail = null,
            AssignAt      = submission.AssignAt,
            Status        = submission.Status,
            GradeStatus   = submission.GradeStatus,
            FileUrl       = submission.FileUrl,
            CreatedAt     = submission.CreatedAt,
            UpdatedAt     = submission.UpdatedAt,
            IsActive      = submission.IsActive,
            Assessments   = new List<AssessmentSummary>()
        };

        // Map tất cả assessments của submission này
        if (submission.Assessments.Count > 0)
        {
            dto.Assessments = submission.Assessments
                .OrderBy(a => a.CreatedAt)
                .Select(a => new AssessmentSummary
                {
                    Id             = a.Id,
                    ExaminerId     = a.ExaminerId,
                    ExaminerEmail  = null, // sẽ fill sau bằng Graph trong handler
                    SubmissionName = a.SubmissionName!,
                    Status         = a.Status,
                    Score          = a.Score,
                    GradedAt       = a.GradedAt
                })
                .ToList();
        }

        return dto;
    }
    
    public static SubmissionUserItemDto ToUserSubmissionDto(
        this Submission submission,
        Guid currentUserId)
    {
       
        var myAssessment = submission.Assessments
            .Where(a => a.ExaminerId == currentUserId && a.SubmissionId == submission.Id)
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefault();

        return new SubmissionUserItemDto
        {
            Id            = submission.Id,
            ExamSubjectId = submission.ExamSubjectId,
            ExamId        = submission.ExamSubject?.ExamId ?? Guid.Empty,
            ExamCode      = submission.ExamSubject?.Exam?.Code,
            SubjectId     = submission.ExamSubject?.SubjectId ?? Guid.Empty,
            SubjectIdCode = submission.ExamSubject?.Subject?.Code,
            AssignAt      = submission.AssignAt,
            Status        = submission.Status,
            GradeStatus   = submission.GradeStatus,
            FileUrl       = submission.FileUrl,

            AssessmentId     = myAssessment?.Id,
            SubmissionName   = myAssessment?.SubmissionName,
            AssessmentStatus = myAssessment?.Status,
            MyScore          = myAssessment?.Score
        };
    }
}
