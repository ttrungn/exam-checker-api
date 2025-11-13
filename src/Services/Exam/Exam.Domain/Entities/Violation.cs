

namespace Exam.Domain.Entities;

public class Violation : BaseAuditableEntity<Guid>
{

    public string? Description { get; set; }
    public bool IsResolved { get; set; } = false;
    public DateTimeOffset? ResolvedAt { get; set; }
    public Guid SubmissionId { get; set; }
    public Submission? Submission { get; set; }
}
