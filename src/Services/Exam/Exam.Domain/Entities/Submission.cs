using Exam.Domain.Enums;

namespace Exam.Domain.Entities;

public class Submission : BaseAuditableEntity<Guid>
{
    public Guid ExamSubjectId { get; set; }
    public Guid? ExaminerId { get; set; }
    public Guid? ModeratorId { get; set; }
    public DateTimeOffset AssignAt { get; set; }
    public SubmissionStatus Status { get; set; } = SubmissionStatus.Pending;
    public string? FileUrl { get; set; }
    public ExamSubject? ExamSubject { get; set; }
}
