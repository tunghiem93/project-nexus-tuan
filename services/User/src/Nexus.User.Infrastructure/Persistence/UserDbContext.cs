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
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<EmailVerification> EmailVerifications => Set<EmailVerification>();
    public DbSet<RatingReview> RatingReviews => Set<RatingReview>();
    public DbSet<ReputationProfile> ReputationProfiles => Set<ReputationProfile>();
    public DbSet<PenaltyViolation> PenaltyViolations => Set<PenaltyViolation>();
    public DbSet<ReputationAudit> ReputationAudits => Set<ReputationAudit>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserDbContext).Assembly);
    }
}
