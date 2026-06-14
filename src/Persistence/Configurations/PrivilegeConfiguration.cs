using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.User.Domain.Entities;

namespace Nexus.User.Persistence.Configurations;

public sealed class PrivilegeConfiguration : IEntityTypeConfiguration<Privilege>
{
    public void Configure(EntityTypeBuilder<Privilege> builder)
    {
        builder.ToTable("Privilege");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("privilege_id");
        builder.Property(x => x.Code).HasColumnName("privilege_code").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Name).HasColumnName("privilege_name").HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").IsRequired(false);

        builder.HasIndex(x => x.Code).IsUnique();
    }
}
