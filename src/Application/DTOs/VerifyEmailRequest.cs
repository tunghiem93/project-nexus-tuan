namespace Nexus.User.Application.Dtos;

public sealed class VerifyEmailRequest
{
    public string Code { get; set; } = string.Empty;
}
