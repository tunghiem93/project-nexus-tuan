using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.User.Domain.Entities;

namespace Nexus.User.Infrastructure.Persistence.Configurations;

public class RolePrivilegeConfiguration : IEntityTypeConfiguration<RolePrivilege>
{
    public void Configure(EntityTypeBuilder<RolePrivilege> builder)
    {
        builder.ToTable("RolePrivilege");
        builder.HasKey(rp => rp.Id);
        builder.Property(rp => rp.Id).HasColumnName("role_privilege_id");
        builder.Property(rp => rp.RoleId).HasColumnName("role_id");
        builder.Property(rp => rp.PrivilegeId).HasColumnName("privilege_id");

        builder.HasOne(rp => rp.Role).WithMany(r => r.RolePrivileges).HasForeignKey(rp => rp.RoleId);
        builder.HasOne(rp => rp.Privilege).WithMany(p => p.RolePrivileges).HasForeignKey(rp => rp.PrivilegeId);
        builder.HasIndex(rp => new { rp.RoleId, rp.PrivilegeId }).IsUnique();
    }
}
