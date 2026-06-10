using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.User.Domain.Entities;

namespace Nexus.User.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLog");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("audit_log_id");
        builder.Property(a => a.ActorUserId).HasColumnName("actor_user_id");
        builder.Property(a => a.Action).HasColumnName("action").HasMaxLength(50).IsRequired();
        builder.Property(a => a.TargetType).HasColumnName("target_type").HasMaxLength(50).IsRequired();
        builder.Property(a => a.TargetRefId).HasColumnName("target_ref_id");
        builder.Property(a => a.DetailJson).HasColumnName("detail_json");
        builder.Property(a => a.IpAddress).HasColumnName("ip_address").HasMaxLength(45);
        builder.Property(a => a.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(a => a.ActorUserId);
        builder.HasIndex(a => new { a.TargetType, a.TargetRefId });
    }
}
