using Nexus.Abstractions.Primitives;

namespace Nexus.Auction.Domain.Entities;

public class AuctionLot : Entity, IAuditableEntity
{
    public Guid ProductId { get; set; }
    public Guid SellerId { get; set; }
    public string Status { get; set; } = "SCHEDULED";
    public decimal StartingPrice { get; set; }
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset EndsAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
