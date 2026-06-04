using Microsoft.EntityFrameworkCore;
using Nexus.Persistence;
using Nexus.Commerce.Domain.Entities;

namespace Nexus.Commerce.Infrastructure.Persistence;

public class CommerceDbContext(DbContextOptions<CommerceDbContext> options) : NexusDbContext(options)
{
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CommerceDbContext).Assembly);
    }
}
