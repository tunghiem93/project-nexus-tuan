using Nexus.Abstractions.Primitives;

namespace Nexus.Commerce.Domain.Entities;

public class Cart : Entity, IAuditableEntity
{
    public Guid UserId { get; set; }
    public string Status { get; set; } = "ACTIVE";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
