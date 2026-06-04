using Nexus.Abstractions.Primitives;

namespace Nexus.Catalog.Domain.Entities;

public class Product : Entity, IAuditableEntity
{
    public Guid SellerId { get; set; }
    public string SkuCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public string Currency { get; set; } = "VND";
    public string Status { get; set; } = "DRAFT";
    public bool AuctionEnabled { get; set; } = true;
    public Guid CategoryId { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Category Category { get; set; } = null!;
}
