namespace Nexus.User.Application.Dtos;

public sealed class LoginOtpRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
}
