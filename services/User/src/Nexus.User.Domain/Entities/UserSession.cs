using Nexus.Abstractions.Primitives;

namespace Nexus.User.Domain.Entities;

public class UserSession : Entity, IAuditableEntity
{
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public string? AccessJti { get; set; }
    public string? RefreshTokenHash { get; set; }
    public DateTimeOffset? RefreshExpiresAt { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTimeOffset LoginAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? LogoutAt { get; set; }
    public string Status { get; set; } = "ACTIVE";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public UserAccount User { get; set; } = null!;
}
