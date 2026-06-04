using Nexus.Abstractions.Primitives;

namespace Nexus.Commerce.Domain.Entities;

public class Order : Entity, IAuditableEntity
{
    public Guid UserId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "PENDING";
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "VND";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
