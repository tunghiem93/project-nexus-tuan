using Nexus.User.Application.Dtos;
using Nexus.User.Application.DTOs.Response;

namespace Nexus.User.Application.Services;

public interface IAuthService
{
    Task<Guid> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task VerifyEmailAsync(string code, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<SendOtpResponse> SendLoginOtpAsync(SendOtpRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginWithOtpAsync(LoginOtpRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> OAuthLoginAsync(OAuthLoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(Guid userId, string oldPassword, string newPassword, string accessTokenJti, CancellationToken cancellationToken = default);
    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
}
