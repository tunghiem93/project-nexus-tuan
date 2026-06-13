using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Fulfillment.Domain.Entities;

namespace Nexus.Fulfillment.Infrastructure.Persistence.Configurations;

public class InventoryRecordConfiguration : IEntityTypeConfiguration<InventoryRecord>
{
    public void Configure(EntityTypeBuilder<InventoryRecord> builder)
    {
        builder.ToTable("inventory_records");
        builder.HasKey(e => e.Id);
    }
}
