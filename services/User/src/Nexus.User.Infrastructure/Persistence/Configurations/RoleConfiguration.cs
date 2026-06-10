using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.User.Domain.Entities;

namespace Nexus.User.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Role");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("role_id");
        builder.Property(r => r.Code).HasColumnName("role_code").HasMaxLength(50).IsRequired();
        builder.Property(r => r.Name).HasColumnName("role_name").HasMaxLength(100).IsRequired();
        builder.Property(r => r.Description).HasColumnName("role_description");
        builder.Property(r => r.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.Property(r => r.CreatedAt).HasColumnName("created_at");
        builder.Property(r => r.UpdatedAt).HasColumnName("updated_at");
        builder.HasIndex(r => r.Code).IsUnique();
    }
}
