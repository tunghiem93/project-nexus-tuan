using Microsoft.EntityFrameworkCore;
using Nexus.Abstractions.Outbox;
using Nexus.Persistence.Configurations;

namespace Nexus.Persistence;

public abstract class NexusDbContext : DbContext
{
    protected NexusDbContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
