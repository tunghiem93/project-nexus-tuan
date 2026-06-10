using Nexus.Abstractions.Primitives;

namespace Nexus.User.Domain.Entities;

public class EmailVerification : Entity, IAuditableEntity
{
    public Guid UserId { get; set; }
    public string VerificationTokenHash { get; set; } = string.Empty;
    public DateTimeOffset RequestedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? VerifiedAt { get; set; }
    public string Status { get; set; } = "PENDING";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public UserAccount User { get; set; } = null!;
}
