using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.User.Domain.Entities;

namespace Nexus.User.Persistence.Configurations;

public sealed class ReputationAuditConfiguration : IEntityTypeConfiguration<ReputationAudit>
{
    public void Configure(EntityTypeBuilder<ReputationAudit> builder)
    {
        builder.ToTable("ReputationAudit");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("audit_id");
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.ActionType).HasColumnName("action_type").HasMaxLength(30).IsRequired();
        builder.Property(x => x.TransactionRefId).HasColumnName("transaction_ref_id").IsRequired(false);
        builder.Property(x => x.ViolationRefId).HasColumnName("violation_ref_id").IsRequired(false);
        builder.Property(x => x.OldScore).HasColumnName("old_score").HasPrecision(6, 2).IsRequired(false);
        builder.Property(x => x.NewScore).HasColumnName("new_score").HasPrecision(6, 2).IsRequired(false);
        builder.Property(x => x.DetailJson).HasColumnName("detail_json").IsRequired(false);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(x => x.UserId).HasDatabaseName("IX_ReputationAudit_user");

        builder.HasOne(x => x.User)
            .WithMany(u => u.ReputationAudits)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
