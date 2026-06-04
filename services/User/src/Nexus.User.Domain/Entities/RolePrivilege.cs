namespace Nexus.User.Domain.Entities;

public class RolePrivilege
{
    public Guid RoleId { get; set; }
    public Guid PrivilegeId { get; set; }

    public Role Role { get; set; } = null!;
    public Privilege Privilege { get; set; } = null!;
}
