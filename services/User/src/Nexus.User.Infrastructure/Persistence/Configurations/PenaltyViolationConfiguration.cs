using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.User.Domain.Entities;

namespace Nexus.User.Infrastructure.Persistence.Configurations;

public class PenaltyViolationConfiguration : IEntityTypeConfiguration<PenaltyViolation>
{
    public void Configure(EntityTypeBuilder<PenaltyViolation> builder)
    {
        builder.ToTable("PenaltyViolation");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("penalty_id");
        builder.Property(p => p.UserId).HasColumnName("user_id");
        builder.Property(p => p.RelatedRefId).HasColumnName("related_ref_id");
        builder.Property(p => p.ViolationType).HasColumnName("violation_type").HasMaxLength(30).IsRequired();
        builder.Property(p => p.Severity).HasColumnName("severity").HasMaxLength(20).IsRequired();
        builder.Property(p => p.PenaltyPoints).HasColumnName("penalty_points").HasPrecision(6, 2).IsRequired();
        builder.Property(p => p.Reason).HasColumnName("reason");
        builder.Property(p => p.CreatedAt).HasColumnName("created_at");
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(p => p.User).WithMany(u => u.PenaltyViolations).HasForeignKey(p => p.UserId);
        builder.HasIndex(p => p.UserId);
    }
}
