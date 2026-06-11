using Nexus.Abstractions.Primitives;

namespace Nexus.Catalog.Domain.Entities;

public class Category : Entity, IAuditableEntity
{
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public short Level { get; set; }
    public int SortOrder { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
