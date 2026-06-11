using Nexus.Abstractions.Primitives;

namespace Nexus.User.Domain.Entities;

public class ReputationAudit : Entity, IAuditableEntity
{
    public Guid UserId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public Guid? TransactionRefId { get; set; }
    public Guid? ViolationRefId { get; set; }
    public decimal? OldScore { get; set; }
    public decimal? NewScore { get; set; }
    public string? DetailJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public UserAccount User { get; set; } = null!;
}
