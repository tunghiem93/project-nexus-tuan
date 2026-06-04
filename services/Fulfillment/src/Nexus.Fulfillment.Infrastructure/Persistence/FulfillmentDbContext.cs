using Microsoft.EntityFrameworkCore;
using Nexus.Persistence;
using Nexus.Fulfillment.Domain.Entities;

namespace Nexus.Fulfillment.Infrastructure.Persistence;

public class FulfillmentDbContext(DbContextOptions<FulfillmentDbContext> options) : NexusDbContext(options)
{
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<InventoryRecord> InventoryRecords => Set<InventoryRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FulfillmentDbContext).Assembly);
    }
}
