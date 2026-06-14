using Microsoft.EntityFrameworkCore;
using Nexus.Abstractions.Outbox;
using Nexus.User.Persistence;
using Nexus.User.Domain.Entities;

namespace Nexus.User.Infrastructure.Persistence;

public sealed class UserDbContext : NexusDbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserAccount> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<UserRole> UserRoles { get; set; } = null!;
    public DbSet<EmailVerification> EmailVerifications { get; set; } = null!;
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;
    public DbSet<UserSession> UserSessions { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;
}
