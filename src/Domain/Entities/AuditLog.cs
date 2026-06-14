using Nexus.Abstractions.Primitives;

namespace Nexus.User.Domain.Entities;

public class AuditLog : Entity
{
    public Guid? ActorUserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public Guid? TargetRefId { get; set; }
    public string? DetailJson { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
}
