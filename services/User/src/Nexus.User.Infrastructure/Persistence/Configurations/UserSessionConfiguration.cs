using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.User.Domain.Entities;

namespace Nexus.User.Infrastructure.Persistence.Configurations;

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("UserSession");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("session_id");
        builder.Property(s => s.UserId).HasColumnName("user_id");
        builder.Property(s => s.TokenHash).HasColumnName("token_hash").HasMaxLength(255).IsRequired();
        builder.Property(s => s.AccessJti).HasColumnName("access_jti").HasMaxLength(64);
        builder.Property(s => s.RefreshTokenHash).HasColumnName("refresh_token_hash").HasMaxLength(255);
        builder.Property(s => s.RefreshExpiresAt).HasColumnName("refresh_expires_at");
        builder.Property(s => s.IpAddress).HasColumnName("ip_address").HasMaxLength(45);
        builder.Property(s => s.UserAgent).HasColumnName("user_agent").HasMaxLength(500);
        builder.Property(s => s.LoginAt).HasColumnName("login_at");
        builder.Property(s => s.ExpiresAt).HasColumnName("expires_at");
        builder.Property(s => s.LogoutAt).HasColumnName("logout_at");
        builder.Property(s => s.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
        builder.Property(s => s.CreatedAt).HasColumnName("created_at");
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(s => s.User)
            .WithMany(u => u.Sessions)
            .HasForeignKey(s => s.UserId);

        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.TokenHash);
    }
}
