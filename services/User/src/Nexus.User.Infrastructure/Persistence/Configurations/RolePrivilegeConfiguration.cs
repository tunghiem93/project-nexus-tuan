using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.User.Domain.Entities;

namespace Nexus.User.Infrastructure.Persistence.Configurations;

public class RolePrivilegeConfiguration : IEntityTypeConfiguration<RolePrivilege>
{
    public void Configure(EntityTypeBuilder<RolePrivilege> builder)
    {
        builder.ToTable("role_privileges");
        builder.HasKey(rp => new { rp.RoleId, rp.PrivilegeId });
        builder.Property(rp => rp.RoleId).HasColumnName("role_id");
        builder.Property(rp => rp.PrivilegeId).HasColumnName("privilege_id");
        builder.HasOne(rp => rp.Role).WithMany(r => r.RolePrivileges).HasForeignKey(rp => rp.RoleId);
        builder.HasOne(rp => rp.Privilege).WithMany(p => p.RolePrivileges).HasForeignKey(rp => rp.PrivilegeId);
    }
}
