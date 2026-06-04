using Microsoft.EntityFrameworkCore;
using Nexus.Persistence;
using Nexus.Auction.Domain.Entities;

namespace Nexus.Auction.Infrastructure.Persistence;

public class AuctionDbContext(DbContextOptions<AuctionDbContext> options) : NexusDbContext(options)
{
    public DbSet<AuctionLot> Auctions => Set<AuctionLot>();
    public DbSet<AuctionBid> AuctionBids => Set<AuctionBid>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuctionDbContext).Assembly);
    }
}
