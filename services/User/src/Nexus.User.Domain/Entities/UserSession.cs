using Nexus.Abstractions.Primitives;

namespace Nexus.User.Domain.Entities;

public class UserSession : Entity, IAuditableEntity
{
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public string? AccessJti { get; set; }
    public string? RefreshTokenHash { get; set; }
    public DateTime? RefreshExpiresAt { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime LoginAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? LogoutAt { get; set; }
    public string Status { get; set; } = "ACTIVE";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public UserAccount User { get; set; } = null!;
}
