using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.User.Domain.Entities;

namespace Nexus.User.Infrastructure.Persistence.Configurations;

public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("PasswordReset");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("reset_id");
        builder.Property(t => t.UserId).HasColumnName("user_id");
        builder.Property(t => t.TokenHash).HasColumnName("reset_token_hash").HasMaxLength(255).IsRequired();
        builder.Property(t => t.RequestedAt).HasColumnName("requested_at");
        builder.Property(t => t.ExpiresAt).HasColumnName("expires_at");
        builder.Property(t => t.UsedAt).HasColumnName("used_at");
        builder.Property(t => t.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
        builder.Property(t => t.CreatedAt).HasColumnName("created_at");
        builder.Property(t => t.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(t => t.User)
            .WithMany(u => u.PasswordResetTokens)
            .HasForeignKey(t => t.UserId);

        builder.HasIndex(t => t.UserId);
        builder.HasIndex(t => t.TokenHash);
    }
}
