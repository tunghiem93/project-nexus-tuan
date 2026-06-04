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
    public DateTimeOffset? DeletedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
