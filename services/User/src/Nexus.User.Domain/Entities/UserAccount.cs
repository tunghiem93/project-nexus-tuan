using Nexus.Abstractions.Primitives;

namespace Nexus.User.Domain.Entities;

public class UserAccount : Entity, IAuditableEntity
{
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string IdentifyNumber { get; set; } = string.Empty;
    public string Gender { get; set; } = Enums.Gender.Unspecified;
    public string Address { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string Status { get; set; } = Enums.UserStatus.Active;
    public DateTimeOffset? EmailVerifiedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = [];
    public ICollection<UserSession> Sessions { get; set; } = [];
    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = [];
    public ICollection<UserPreference> Preferences { get; set; } = [];
}
