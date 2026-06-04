using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Notification.Domain.Entities;

namespace Nexus.Notification.Infrastructure.Persistence.Configurations;

public class NotificationMessageConfiguration : IEntityTypeConfiguration<NotificationMessage>
{
    public void Configure(EntityTypeBuilder<NotificationMessage> builder)
    {
        builder.ToTable("notifications");
        builder.HasKey(e => e.Id);
    }
}
