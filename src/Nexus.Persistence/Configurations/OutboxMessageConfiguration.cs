using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Abstractions.Outbox;

namespace Nexus.Persistence.Configurations;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_events");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.AggregateType).HasColumnName("aggregate_type").HasMaxLength(50).IsRequired();
        builder.Property(e => e.AggregateId).HasColumnName("aggregate_id");
        builder.Property(e => e.EventType).HasColumnName("event_type").HasMaxLength(100).IsRequired();
        builder.Property(e => e.Payload).HasColumnName("payload").IsRequired();
        builder.Property(e => e.PublishedAt).HasColumnName("published_at");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
    }
}
