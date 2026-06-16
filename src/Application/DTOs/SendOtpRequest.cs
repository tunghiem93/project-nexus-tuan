namespace Nexus.User.Application.Dtos;

public sealed class SendOtpRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
}
