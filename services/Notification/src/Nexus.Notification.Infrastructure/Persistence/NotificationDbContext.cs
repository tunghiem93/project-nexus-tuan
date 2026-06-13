using Microsoft.EntityFrameworkCore;
using Nexus.Persistence;
using Nexus.Notification.Domain.Entities;

namespace Nexus.Notification.Infrastructure.Persistence;

public class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : NexusDbContext(options)
{
    public DbSet<EventLog> EventLogs => Set<EventLog>();
    public DbSet<NotificationMessage> Notifications => Set<NotificationMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);
    }
}
