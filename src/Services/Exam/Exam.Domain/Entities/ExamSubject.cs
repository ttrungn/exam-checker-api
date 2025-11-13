namespace Exam.Domain.Entities;

public class ExamSubject : BaseAuditableEntity<Guid>
{
    public Guid ExamId { get; set; }
    public Guid SubjectId { get; set; } 
    public string? ScoreStructure { get; set; }
    public string? ViolationStructure { get; set; } 
    public Subject Subject { get; set; } = null!;
    public Exam Exam { get; set; } = null!;
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
