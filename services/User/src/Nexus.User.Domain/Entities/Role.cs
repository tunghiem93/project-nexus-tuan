using Nexus.Abstractions.Primitives;

namespace Nexus.User.Domain.Entities;

public class Role : Entity, IAuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<RolePrivilege> RolePrivileges { get; set; } = [];
    public ICollection<UserRole> UserRoles { get; set; } = [];
}
