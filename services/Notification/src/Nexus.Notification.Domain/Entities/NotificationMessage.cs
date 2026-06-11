using Nexus.Abstractions.Primitives;

namespace Nexus.Notification.Domain.Entities;

public class NotificationMessage : Entity
{
    public Guid UserId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Status { get; set; } = "PENDING";
    public DateTime CreatedAt { get; set; }
}
