namespace Nexus.User.Infrastructure.Services;

public sealed class CurrentUser
{
    public Guid? Id { get; set; }
    public string? Role { get; set; }
    public string? Scope { get; set; }
    public string? AccessTokenJti { get; set; }
}
