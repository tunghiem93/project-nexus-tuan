using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.User.Domain.Entities;

namespace Nexus.User.Infrastructure.Persistence.Configurations;

public class ReputationProfileConfiguration : IEntityTypeConfiguration<ReputationProfile>
{
    public void Configure(EntityTypeBuilder<ReputationProfile> builder)
    {
        builder.ToTable("ReputationProfile");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("reputation_id");
        builder.Property(r => r.UserId).HasColumnName("user_id");
        builder.Property(r => r.ReputationScore).HasColumnName("reputation_score").HasPrecision(6, 2).IsRequired();
        builder.Property(r => r.TrustLevel).HasColumnName("trust_level").HasMaxLength(20).IsRequired();
        builder.Property(r => r.SuccessfulTransactionCount).HasColumnName("successful_transaction_count").IsRequired();
        builder.Property(r => r.FailedActivityCount).HasColumnName("failed_activity_count").IsRequired();
        builder.Property(r => r.AuctionWinCount).HasColumnName("auction_win_count").IsRequired();
        builder.Property(r => r.AuctionFailCount).HasColumnName("auction_fail_count").IsRequired();
        builder.Property(r => r.UpdatedAt).HasColumnName("updated_at");
        builder.Property(r => r.CreatedAt).HasColumnName("created_at");

        builder.HasOne(r => r.User).WithMany(u => u.ReputationProfiles).HasForeignKey(r => r.UserId);
        builder.HasIndex(r => r.UserId).IsUnique();
    }
}
