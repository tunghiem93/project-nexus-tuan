using Nexus.Abstractions.Primitives;

namespace Nexus.Commerce.Domain.Entities;

public class Cart : Entity, IAuditableEntity
{
    public Guid UserId { get; set; }
    public string Status { get; set; } = "ACTIVE";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
