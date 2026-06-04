using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.User.Domain.Entities;

namespace Nexus.User.Infrastructure.Persistence.Configurations;

public class PrivilegeConfiguration : IEntityTypeConfiguration<Privilege>
{
    public void Configure(EntityTypeBuilder<Privilege> builder)
    {
        builder.ToTable("privileges");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Code).HasColumnName("code").HasMaxLength(100).IsRequired();
        builder.Property(p => p.Description).HasColumnName("description");
        builder.Property(p => p.CreatedAt).HasColumnName("created_at");
        builder.HasIndex(p => p.Code).IsUnique();
    }
}
