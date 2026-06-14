using Nexus.Abstractions.Primitives;

namespace Nexus.User.Domain.Entities;

public class UserPreference : Entity, IAuditableEntity
{
    public Guid UserId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public UserAccount User { get; set; } = null!;
}
