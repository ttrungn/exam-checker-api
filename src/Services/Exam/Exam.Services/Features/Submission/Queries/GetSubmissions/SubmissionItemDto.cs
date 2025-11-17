using Exam.Domain.Enums;

namespace Exam.Services.Features.Submission.Queries.GetSubmissions;

public class SubmissionItemDto
{
    public Guid Id { get; set; }
    public Guid ExamSubjectId { get; set; }
    public Guid ExamId { get; set; }
    public string? ExamCode { get; set; }
    public Guid SubjectId { get; set; }
    public string? SubjectIdCode { get; set; }
    public Guid? ExaminerId { get; set; }
    public string? ExaminerEmail { get; set; }
    public Guid? ModeratorId { get; set; }
    public string? ModeratorEmail { get; set; }
    public DateTimeOffset AssignAt { get; set; }
    public SubmissionStatus Status { get; set; }
    public string? FileUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}
