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
    public DateOnly DateOfBirth { get; set; }
    public string Status { get; set; } = Enums.UserStatus.Active;
    public bool IsEmailVerified { get; set; }
    public DateTimeOffset? EmailVerifiedAt { get; set; }
    public int FailedLoginCount { get; set; }
    public DateTimeOffset? LockedUntil { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = [];
    public ICollection<UserSession> Sessions { get; set; } = [];
    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = [];
    public ICollection<UserPreference> Preferences { get; set; } = [];
    public ICollection<EmailVerification> EmailVerifications { get; set; } = [];
    public ICollection<RatingReview> RatingsGiven { get; set; } = [];
    public ICollection<RatingReview> RatingsReceived { get; set; } = [];
    public ICollection<PenaltyViolation> PenaltyViolations { get; set; } = [];
    public ICollection<ReputationProfile> ReputationProfiles { get; set; } = [];
    public ICollection<ReputationAudit> ReputationAudits { get; set; } = [];
    public ICollection<AuditLog> AuditLogs { get; set; } = [];
}
