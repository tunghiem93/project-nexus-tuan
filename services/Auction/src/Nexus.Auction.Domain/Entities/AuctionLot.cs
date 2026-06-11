using Nexus.Abstractions.Primitives;

namespace Nexus.Auction.Domain.Entities;

public class AuctionLot : Entity, IAuditableEntity
{
    public Guid ProductId { get; set; }
    public Guid SellerId { get; set; }
    public string Status { get; set; } = "SCHEDULED";
    public decimal StartingPrice { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
