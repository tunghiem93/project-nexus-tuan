namespace Nexus.User.Contracts.Dtos;

public sealed class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}
