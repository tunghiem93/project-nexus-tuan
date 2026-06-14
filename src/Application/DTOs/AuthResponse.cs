namespace Nexus.User.Application.Dtos;

public sealed class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public int AccessTokenExpiresIn { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public int RefreshTokenExpiresIn { get; set; }
}
