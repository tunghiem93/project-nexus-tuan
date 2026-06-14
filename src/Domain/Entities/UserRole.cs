namespace Nexus.User.Domain.Entities;

public class UserRole
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime AssignedAt { get; set; }
    public Guid? AssignedBy { get; set; }

    public UserAccount User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
