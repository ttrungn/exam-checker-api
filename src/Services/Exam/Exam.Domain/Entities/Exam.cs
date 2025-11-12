namespace Exam.Domain.Entities;

public class Exam : BaseAuditableEntity<Guid>
{
    public Guid SemesterId { get; set; }
    public string Code { get; set; } = null!;
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }

    public Semester? Semester { get; set; } = null!;
    public ICollection<ExamSubject> ExamSubjects { get; set; } = new List<ExamSubject>();
}
