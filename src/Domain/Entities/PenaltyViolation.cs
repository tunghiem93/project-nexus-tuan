using Nexus.Abstractions.Primitives;

namespace Nexus.User.Domain.Entities;

public class PenaltyViolation : Entity, IAuditableEntity
{
    public Guid UserId { get; set; }
    public Guid? RelatedRefId { get; set; }
    public string ViolationType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public decimal PenaltyPoints { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public UserAccount User { get; set; } = null!;
}
