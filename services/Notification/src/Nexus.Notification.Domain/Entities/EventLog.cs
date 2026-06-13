using Nexus.Abstractions.Primitives;

namespace Nexus.Notification.Domain.Entities;

public class EventLog : Entity
{
    public string EventType { get; set; } = string.Empty;
    public string SourceService { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; }
}
