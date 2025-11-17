using Exam.Services.Features.Submission.Queries.GetSubmissions;

namespace Exam.Services.Mappers;

public static class SubmissionMapper
{
    public static SubmissionItemDto ToSubmissionItemDto(this Domain.Entities.Submission submission)
    {
        return new SubmissionItemDto
        {
            Id = submission.Id,
            ExamSubjectId = submission.ExamSubjectId,
            ExamId = submission.ExamSubject?.ExamId ?? Guid.Empty,
            ExamCode = submission.ExamSubject?.Exam?.Code,
            SubjectId = submission.ExamSubject?.SubjectId ?? Guid.Empty,
            SubjectIdCode = submission.ExamSubject?.Subject?.Code,
            ExaminerId = submission.ExaminerId,
            ExaminerEmail = null, 
            ModeratorId = submission.ModeratorId,
            ModeratorEmail = null,
            AssignAt = submission.AssignAt,
            Status = submission.Status,
            FileUrl = submission.FileUrl,
            CreatedAt = submission.CreatedAt,
            UpdatedAt = submission.UpdatedAt,
            IsActive = submission.IsActive
        };
    }
}
