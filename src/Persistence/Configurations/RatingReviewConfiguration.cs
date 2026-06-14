using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.User.Domain.Entities;

namespace Nexus.User.Persistence.Configurations;

public sealed class RatingReviewConfiguration : IEntityTypeConfiguration<RatingReview>
{
    public void Configure(EntityTypeBuilder<RatingReview> builder)
    {
        builder.ToTable("RatingReview");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("rating_id");
        builder.Property(x => x.TransactionRefId).HasColumnName("transaction_ref_id").IsRequired();
        builder.Property(x => x.TransactionType).HasColumnName("transaction_type").HasMaxLength(20).IsRequired();
        builder.Property(x => x.RaterUserId).HasColumnName("rater_user_id").IsRequired();
        builder.Property(x => x.RatedUserId).HasColumnName("rated_user_id").IsRequired();
        builder.Property(x => x.FeedbackType).HasColumnName("feedback_type").HasMaxLength(10).IsRequired();
        builder.Property(x => x.Score).HasColumnName("score");
        builder.Property(x => x.Comment).HasColumnName("comment").IsRequired(false);
        builder.Property(x => x.IsDisputed).HasColumnName("is_disputed").IsRequired();
        builder.Property(x => x.SubmittedAt).HasColumnName("submitted_at").IsRequired();

        builder.HasIndex(x => x.RatedUserId).HasDatabaseName("IX_RatingReview_rated");

        builder.HasOne(x => x.RaterUser)
            .WithMany(u => u.RatingsGiven)
            .HasForeignKey(x => x.RaterUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.RatedUser)
            .WithMany(u => u.RatingsReceived)
            .HasForeignKey(x => x.RatedUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
