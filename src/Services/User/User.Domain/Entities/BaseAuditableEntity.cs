using User.Domain.Interfaces.Entities;

namespace User.Domain.Entities;

public class BaseAuditableEntity<TId> :
    BaseEntity<TId>, IBaseAuditableEntity
    where TId : notnull
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsActive { get; set; } = true;
}
