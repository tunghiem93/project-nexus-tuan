using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.User.Domain.Entities;

namespace Nexus.User.Infrastructure.Persistence.Configurations;

public class PrivilegeConfiguration : IEntityTypeConfiguration<Privilege>
{
    public void Configure(EntityTypeBuilder<Privilege> builder)
    {
        builder.ToTable("Privilege");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("privilege_id");
        builder.Property(p => p.Code).HasColumnName("privilege_code").HasMaxLength(100).IsRequired();
        builder.Property(p => p.Name).HasColumnName("privilege_name").HasMaxLength(150).IsRequired();
        builder.Property(p => p.Description).HasColumnName("description");
        builder.HasIndex(p => p.Code).IsUnique();
    }
}
