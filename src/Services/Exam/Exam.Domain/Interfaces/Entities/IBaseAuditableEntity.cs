namespace Exam.Domain.Interfaces.Entities;

public interface IBaseAuditableEntity
{
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
    DateTime? DeletedAt { get; set; }
    bool IsActive { get; set; }
}
