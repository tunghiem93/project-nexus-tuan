using Nexus.Abstractions.Primitives;

namespace Nexus.User.Domain.Entities;

public class Privilege : Entity
{
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<RolePrivilege> RolePrivileges { get; set; } = [];
}
