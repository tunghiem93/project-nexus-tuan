namespace Nexus.User.Application.Dtos;

public sealed class OAuthLoginRequest
{
    public string Provider { get; set; } = string.Empty;
    public string IdToken { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}
