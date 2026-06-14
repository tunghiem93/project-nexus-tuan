using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.User.Domain.Entities;

namespace Nexus.User.Persistence.Configurations;

public sealed class PenaltyViolationConfiguration : IEntityTypeConfiguration<PenaltyViolation>
{
    public void Configure(EntityTypeBuilder<PenaltyViolation> builder)
    {
        builder.ToTable("PenaltyViolation");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("penalty_id");
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.RelatedRefId).HasColumnName("related_ref_id").IsRequired(false);
        builder.Property(x => x.ViolationType).HasColumnName("violation_type").HasMaxLength(30).IsRequired();
        builder.Property(x => x.Severity).HasColumnName("severity").HasMaxLength(20).IsRequired();
        builder.Property(x => x.PenaltyPoints).HasColumnName("penalty_points").HasPrecision(6, 2).IsRequired();
        builder.Property(x => x.Reason).HasColumnName("reason").IsRequired(false);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(x => x.UserId).HasDatabaseName("IX_PenaltyViolation_user");

        builder.HasOne(x => x.User)
            .WithMany(u => u.PenaltyViolations)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
