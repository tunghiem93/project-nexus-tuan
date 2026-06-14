using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Nexus.Abstractions.Primitives;

namespace Nexus.User.Domain.Entities;

public class UserAccount : Entity, IAuditableEntity
{
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? IdentifyNumber { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string Status { get; set; } = Enums.UserStatus.Active;
    public bool IsEmailVerified { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }
    public int FailedLoginCount { get; set; }
    public DateTime? LockedUntil { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
    public ICollection<UserSession> Sessions { get; set; } = new HashSet<UserSession>();
    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new HashSet<PasswordResetToken>();
    public ICollection<UserPreference> Preferences { get; set; } = new HashSet<UserPreference>();
    public ICollection<EmailVerification> EmailVerifications { get; set; } = new HashSet<EmailVerification>();
    public ICollection<RatingReview> RatingsGiven { get; set; } = new HashSet<RatingReview>();
    public ICollection<RatingReview> RatingsReceived { get; set; } = new HashSet<RatingReview>();
    public ICollection<PenaltyViolation> PenaltyViolations { get; set; } = new HashSet<PenaltyViolation>();
    public ICollection<ReputationProfile> ReputationProfiles { get; set; } = new HashSet<ReputationProfile>();
    public ICollection<ReputationAudit> ReputationAudits { get; set; } = new HashSet<ReputationAudit>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new HashSet<AuditLog>();

    // Domain helpers
    public void VerifyEmail(DateTime at)
    {
        IsEmailVerified = true;
        EmailVerifiedAt = at;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementFailedLogin()
    {
        FailedLoginCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ResetFailedLogin()
    {
        FailedLoginCount = 0;
        UpdatedAt = DateTime.UtcNow;
    }

    public void LockUntilDate(DateTime until)
    {
        LockedUntil = until;
        Status = Enums.UserStatus.Locked;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateLastLogin(DateTime at)
    {
        LastLoginAt = at;
        ResetFailedLogin();
        UpdatedAt = DateTime.UtcNow;
    }
}
