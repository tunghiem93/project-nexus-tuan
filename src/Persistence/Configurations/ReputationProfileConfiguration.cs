using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.User.Domain.Entities;

namespace Nexus.User.Persistence.Configurations;

public sealed class ReputationProfileConfiguration : IEntityTypeConfiguration<ReputationProfile>
{
    public void Configure(EntityTypeBuilder<ReputationProfile> builder)
    {
        builder.ToTable("ReputationProfile");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("reputation_id");
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.ReputationScore).HasColumnName("reputation_score").HasPrecision(6, 2).IsRequired();
        builder.Property(x => x.TrustLevel).HasColumnName("trust_level").HasMaxLength(20).IsRequired();
        builder.Property(x => x.SuccessfulTransactionCount).HasColumnName("successful_transaction_count").IsRequired();
        builder.Property(x => x.FailedActivityCount).HasColumnName("failed_activity_count").IsRequired();
        builder.Property(x => x.AuctionWinCount).HasColumnName("auction_win_count").IsRequired();
        builder.Property(x => x.AuctionFailCount).HasColumnName("auction_fail_count").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(x => x.UserId).HasDatabaseName("UQ_ReputationProfile_user");

        builder.HasOne(x => x.User)
            .WithMany(u => u.ReputationProfiles)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
