using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.User.Domain.Entities;

namespace Nexus.User.Persistence.Configurations;

public sealed class EmailVerificationConfiguration : IEntityTypeConfiguration<EmailVerification>
{
    public void Configure(EntityTypeBuilder<EmailVerification> builder)
    {
        builder.ToTable("EmailVerification");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("verification_id");
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.VerificationTokenHash).HasColumnName("verification_token_hash").HasMaxLength(255).IsRequired();
        builder.Property(x => x.RequestedAt).HasColumnName("requested_at").IsRequired();
        builder.Property(x => x.ExpiresAt).HasColumnName("expires_at").IsRequired();
        builder.Property(x => x.VerifiedAt).HasColumnName("verified_at");
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).IsRequired();

        builder.HasIndex(x => x.UserId).HasDatabaseName("IX_EmailVerification_user");
        builder.HasIndex(x => x.VerificationTokenHash).HasDatabaseName("IX_EmailVerification_token");

        builder.HasOne(x => x.User)
            .WithMany(u => u.EmailVerifications)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
