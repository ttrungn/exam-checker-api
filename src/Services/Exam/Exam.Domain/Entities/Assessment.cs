using Exam.Domain.Enums;

namespace Exam.Domain.Entities;

public class Assessment : BaseAuditableEntity<Guid>
{
    public Guid SubmissionId { get; set; }
    public Guid ExaminerId { get; set; }
    public string? StudentCode { get; set; }
    public string? SubmissionName { get; set; }
    public decimal? Score { get; set; }
    public string? ScoreDetails { get; set; }
    public string? Comment { get; set; }
    public DateTimeOffset? GradedAt { get; set; }
    public AssessmentStatus Status { get; set; } = AssessmentStatus.Pending;
    public Submission? Submission { get; set; }
}
