using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Notification.Domain.Entities;

namespace Nexus.Notification.Infrastructure.Persistence.Configurations;

public class EventLogConfiguration : IEntityTypeConfiguration<EventLog>
{
    public void Configure(EntityTypeBuilder<EventLog> builder)
    {
        builder.ToTable("event_logs");
        builder.HasKey(e => e.Id);
    }
}
