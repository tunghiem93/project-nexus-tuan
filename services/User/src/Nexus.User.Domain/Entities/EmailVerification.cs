using Nexus.Abstractions.Primitives;

namespace Nexus.User.Domain.Entities;

public class EmailVerification : Entity
{
    public Guid UserId { get; set; }
    public string VerificationTokenHash { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string Status { get; set; } = "PENDING";

    public UserAccount User { get; set; } = null!;
}
