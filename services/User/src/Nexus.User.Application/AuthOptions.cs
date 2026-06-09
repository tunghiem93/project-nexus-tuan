namespace Nexus.User.Application;

public sealed class AuthOptions
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public int AccessTokenExpiryMinutes { get; set; } = 15;
    public int RefreshTokenExpiryDays { get; set; } = 7;
    public int EmailVerifyTokenExpiryMinutes { get; set; } = 60 * 24;
    public int ResetPasswordTokenExpiryMinutes { get; set; } = 60;
}
