using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Auction.Domain.Entities;

namespace Nexus.Auction.Infrastructure.Persistence.Configurations;

public class AuctionLotConfiguration : IEntityTypeConfiguration<AuctionLot>
{
    public void Configure(EntityTypeBuilder<AuctionLot> builder)
    {
        builder.ToTable("auctions");
        builder.HasKey(e => e.Id);
    }
}
