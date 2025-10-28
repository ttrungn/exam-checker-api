namespace Exam.Domain.Entities;

public class BaseEntity<TId> where TId : notnull
{
    public TId Id { get; set; } = default!;
}
