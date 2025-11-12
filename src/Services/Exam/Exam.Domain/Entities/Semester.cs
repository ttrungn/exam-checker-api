namespace Exam.Domain.Entities;

public class Semester : BaseAuditableEntity<Guid>
{
    public string Name { get; set; } = null!;

    public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
    public ICollection<Exam> Exams { get; set; } = new List<Exam>();
}
