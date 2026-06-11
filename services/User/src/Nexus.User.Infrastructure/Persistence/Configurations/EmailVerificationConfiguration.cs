using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.User.Domain.Entities;

namespace Nexus.User.Infrastructure.Persistence.Configurations;

public class EmailVerificationConfiguration : IEntityTypeConfiguration<EmailVerification>
{
    public void Configure(EntityTypeBuilder<EmailVerification> builder)
    {
        builder.ToTable("EmailVerification");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("verification_id");
        builder.Property(e => e.UserId).HasColumnName("user_id");
        builder.Property(e => e.VerificationTokenHash).HasColumnName("verification_token_hash").HasMaxLength(255).IsRequired();
        builder.Property(e => e.RequestedAt).HasColumnName("requested_at");
        builder.Property(e => e.ExpiresAt).HasColumnName("expires_at");
        builder.Property(e => e.VerifiedAt).HasColumnName("verified_at");
        builder.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired();

        builder.HasOne(e => e.User)
            .WithMany(u => u.EmailVerifications)
            .HasForeignKey(e => e.UserId);

        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.VerificationTokenHash);
    }
}
