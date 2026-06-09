namespace Nexus.User.Contracts.Dtos;

public sealed class LoginRequest
{
    public string UsernameOrEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
