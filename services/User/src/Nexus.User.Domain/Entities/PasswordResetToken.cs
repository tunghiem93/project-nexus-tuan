using Nexus.Abstractions.Primitives;

namespace Nexus.User.Domain.Entities;

public class PasswordResetToken : Entity, IAuditableEntity
{
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? UsedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public UserAccount User { get; set; } = null!;
}
