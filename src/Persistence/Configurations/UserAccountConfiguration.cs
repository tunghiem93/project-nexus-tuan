using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.User.Domain.Entities;

namespace Nexus.User.Persistence.Configurations;

public sealed class UserAccountConfiguration : IEntityTypeConfiguration<UserAccount>
{
    public void Configure(EntityTypeBuilder<UserAccount> builder)
    {
        builder.ToTable("User");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("user_id");
        builder.Property(u => u.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
        builder.Property(u => u.PhoneNumber).HasColumnName("phone_number").HasMaxLength(20).IsRequired(false);
        builder.Property(u => u.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
        builder.Property(u => u.FullName).HasColumnName("full_name").HasMaxLength(150).IsRequired();
        builder.Property(u => u.IdentifyNumber).HasColumnName("identify_number").HasMaxLength(20).IsRequired(false);
        builder.Property(u => u.Gender).HasColumnName("gender").HasMaxLength(10).IsRequired(false);
        builder.Property(u => u.Address).HasColumnName("address").IsRequired(false);
        builder.Property(u => u.DateOfBirth).HasColumnName("date_of_birth").IsRequired(false);
        builder.Property(u => u.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
        builder.Property(u => u.IsEmailVerified).HasColumnName("is_email_verified").IsRequired();
        builder.Property(u => u.EmailVerifiedAt).HasColumnName("email_verified_at");
        builder.Property(u => u.FailedLoginCount).HasColumnName("failed_login_count").IsRequired();
        builder.Property(u => u.LockedUntil).HasColumnName("locked_until");
        builder.Property(u => u.LastLoginAt).HasColumnName("last_login_at");
        builder.Property(u => u.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.Property(u => u.OAuthProvider).HasColumnName("oauth_provider").HasMaxLength(50).IsRequired(false);
        builder.Property(u => u.OAuthProviderId).HasColumnName("oauth_provider_id").HasMaxLength(100).IsRequired(false);
        builder.Property(u => u.CreatedAt).HasColumnName("created_at");
        builder.Property(u => u.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.OAuthProvider);
        builder.HasIndex(u => u.OAuthProviderId);
        builder.HasIndex(u => u.Status);

        builder.ToTable(b =>
        {
            b.HasCheckConstraint("CK_User_status", "status IN ('ACTIVE','LOCKED','INACTIVE')");
            b.HasCheckConstraint("CK_User_gender", "gender IS NULL OR gender IN ('MALE','FEMALE','OTHER')");
            b.HasCheckConstraint("CK_User_failed_login", "failed_login_count >= 0");
        });
    }
}
