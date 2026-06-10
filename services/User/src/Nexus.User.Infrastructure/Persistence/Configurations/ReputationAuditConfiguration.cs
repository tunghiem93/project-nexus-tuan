using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.User.Domain.Entities;

namespace Nexus.User.Infrastructure.Persistence.Configurations;

public class ReputationAuditConfiguration : IEntityTypeConfiguration<ReputationAudit>
{
    public void Configure(EntityTypeBuilder<ReputationAudit> builder)
    {
        builder.ToTable("ReputationAudit");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("audit_id");
        builder.Property(r => r.UserId).HasColumnName("user_id");
        builder.Property(r => r.ActionType).HasColumnName("action_type").HasMaxLength(30).IsRequired();
        builder.Property(r => r.TransactionRefId).HasColumnName("transaction_ref_id");
        builder.Property(r => r.ViolationRefId).HasColumnName("violation_ref_id");
        builder.Property(r => r.OldScore).HasColumnName("old_score").HasPrecision(6, 2);
        builder.Property(r => r.NewScore).HasColumnName("new_score").HasPrecision(6, 2);
        builder.Property(r => r.DetailJson).HasColumnName("detail_json");
        builder.Property(r => r.CreatedAt).HasColumnName("created_at");
        builder.Property(r => r.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(r => r.User).WithMany(u => u.ReputationAudits).HasForeignKey(r => r.UserId);
        builder.HasIndex(r => r.UserId);
    }
}
