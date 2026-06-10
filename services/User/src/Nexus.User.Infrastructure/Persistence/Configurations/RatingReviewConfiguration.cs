using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.User.Domain.Entities;

namespace Nexus.User.Infrastructure.Persistence.Configurations;

public class RatingReviewConfiguration : IEntityTypeConfiguration<RatingReview>
{
    public void Configure(EntityTypeBuilder<RatingReview> builder)
    {
        builder.ToTable("RatingReview");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("rating_id");
        builder.Property(r => r.TransactionRefId).HasColumnName("transaction_ref_id");
        builder.Property(r => r.TransactionType).HasColumnName("transaction_type").HasMaxLength(20).IsRequired();
        builder.Property(r => r.RaterUserId).HasColumnName("rater_user_id");
        builder.Property(r => r.RatedUserId).HasColumnName("rated_user_id");
        builder.Property(r => r.FeedbackType).HasColumnName("feedback_type").HasMaxLength(10).IsRequired();
        builder.Property(r => r.Score).HasColumnName("score");
        builder.Property(r => r.Comment).HasColumnName("comment");
        builder.Property(r => r.IsDisputed).HasColumnName("is_disputed").IsRequired();
        builder.Property(r => r.SubmittedAt).HasColumnName("submitted_at").IsRequired();

        builder.HasOne(r => r.RaterUser).WithMany(u => u.RatingsGiven).HasForeignKey(r => r.RaterUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(r => r.RatedUser).WithMany(u => u.RatingsReceived).HasForeignKey(r => r.RatedUserId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.RatedUserId);
        builder.HasIndex(r => new { r.TransactionRefId, r.RaterUserId }).IsUnique();
        builder.HasCheckConstraint("CK_RatingReview_type", "transaction_type IN ('ORDER','AUCTION')");
        builder.HasCheckConstraint("CK_RatingReview_feedback", "feedback_type IN ('POSITIVE','NEUTRAL','NEGATIVE')");
        builder.HasCheckConstraint("CK_RatingReview_notself", "rater_user_id <> rated_user_id");
    }
}
