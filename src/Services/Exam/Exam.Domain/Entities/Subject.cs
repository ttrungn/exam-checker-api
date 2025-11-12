namespace Exam.Domain.Entities;

public class Subject : BaseAuditableEntity<Guid>
{
    public Guid SemesterId { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;

    public Semester? Semester { get; set; } = null!;
}
