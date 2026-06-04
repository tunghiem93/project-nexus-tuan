using Microsoft.EntityFrameworkCore;
using Nexus.Persistence;
using Nexus.User.Domain.Entities;

namespace Nexus.User.Infrastructure.Persistence;

public class UserDbContext(DbContextOptions<UserDbContext> options) : NexusDbContext(options)
{
    public DbSet<UserAccount> Users => Set<UserAccount>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Privilege> Privileges => Set<Privilege>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePrivilege> RolePrivileges => Set<RolePrivilege>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserDbContext).Assembly);
    }
}
