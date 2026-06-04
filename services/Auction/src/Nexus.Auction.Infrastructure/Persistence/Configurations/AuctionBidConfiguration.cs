using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Auction.Domain.Entities;

namespace Nexus.Auction.Infrastructure.Persistence.Configurations;

public class AuctionBidConfiguration : IEntityTypeConfiguration<AuctionBid>
{
    public void Configure(EntityTypeBuilder<AuctionBid> builder)
    {
        builder.ToTable("auction_bids");
        builder.HasKey(e => e.Id);
    }
}
