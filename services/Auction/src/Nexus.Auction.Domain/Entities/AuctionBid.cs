using Nexus.Abstractions.Primitives;

namespace Nexus.Auction.Domain.Entities;

public class AuctionBid : Entity
{
    public Guid AuctionId { get; set; }
    public Guid BidderId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PlacedAt { get; set; }
    public AuctionLot Auction { get; set; } = null!;
}
