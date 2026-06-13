using Nexus.Abstractions.Primitives;

namespace Nexus.Fulfillment.Domain.Entities;

public class Warehouse : Entity, IAuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
