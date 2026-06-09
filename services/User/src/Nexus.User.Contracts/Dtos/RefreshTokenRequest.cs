namespace Nexus.User.Contracts.Dtos;

public sealed class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
