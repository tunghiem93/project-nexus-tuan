using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.User.Domain.Entities;

namespace Nexus.User.Persistence.Configurations;

public sealed class RolePrivilegeConfiguration : IEntityTypeConfiguration<RolePrivilege>
{
    public void Configure(EntityTypeBuilder<RolePrivilege> builder)
    {
        builder.ToTable("RolePrivilege");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("role_privilege_id");
        builder.Property(x => x.RoleId).HasColumnName("role_id").IsRequired();
        builder.Property(x => x.PrivilegeId).HasColumnName("privilege_id").IsRequired();

        builder.HasIndex(x => x.RoleId).HasDatabaseName("IX_RolePrivilege_role");
        builder.HasIndex(x => x.PrivilegeId).HasDatabaseName("IX_RolePrivilege_privilege");

        builder.HasOne(x => x.Role)
            .WithMany(r => r.RolePrivileges)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Privilege)
            .WithMany(p => p.RolePrivileges)
            .HasForeignKey(x => x.PrivilegeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
