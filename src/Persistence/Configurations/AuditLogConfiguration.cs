using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.User.Domain.Entities;

namespace Nexus.User.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLog");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("audit_log_id");
        builder.Property(x => x.ActorUserId).HasColumnName("actor_user_id").IsRequired(false);
        builder.Property(x => x.Action).HasColumnName("action").HasMaxLength(50).IsRequired();
        builder.Property(x => x.TargetType).HasColumnName("target_type").HasMaxLength(50).IsRequired();
        builder.Property(x => x.TargetRefId).HasColumnName("target_ref_id").IsRequired(false);
        builder.Property(x => x.DetailJson).HasColumnName("detail_json").IsRequired(false);
        builder.Property(x => x.IpAddress).HasColumnName("ip_address").HasMaxLength(45).IsRequired(false);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
    }
}
