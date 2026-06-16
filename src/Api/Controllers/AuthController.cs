using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Nexus.User.Api.Models;
using Nexus.User.Application.Services;
using Nexus.User.Application.Dtos;
using Nexus.User.Application.DTOs.Response;

namespace Nexus.User.Api.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var userId = await _authService.RegisterAsync(request);
        return StatusCode(201, ApiResponse<object>.Success(new { userId }, 201, "Registration successful, please check your email to verify"));
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        await _authService.VerifyEmailAsync(request.Code);
        return Ok(ApiResponse<object>.Success(null, 200, "Email verified successfully"));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(ApiResponse<AuthResponse>.Success(response, 200, "Login successful"));
        }
        catch (InvalidOperationException ex) when (ex.Message == "Account is locked.")
        {
            return StatusCode(423, ApiResponse<object>.Failure(ex.Message, 423));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Too many attempts"))
        {
            return StatusCode(429, ApiResponse<object>.Failure(ex.Message, 429));
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(ApiResponse<object>.Failure("Incorrect email or password.", 401));
        }
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var response = await _authService.RefreshTokenAsync(request.RefreshToken);
        return Ok(ApiResponse<AuthResponse>.Success(response, 200, "Token refreshed successfully"));
    }

    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
    {
        var response = await _authService.SendLoginOtpAsync(request);
        return Ok(ApiResponse<SendOtpResponse>.Success(response, 200, "OTP sent successfully"));
    }

    [HttpPost("login-otp")]
    public async Task<IActionResult> LoginWithOtp([FromBody] LoginOtpRequest request)
    {
        var response = await _authService.LoginWithOtpAsync(request);
        return Ok(ApiResponse<AuthResponse>.Success(response, 200, "Login successful"));
    }

    [HttpPost("oauth")]
    public async Task<IActionResult> OAuthLogin([FromBody] OAuthLoginRequest request)
    {
        var response = await _authService.OAuthLoginAsync(request);
        return Ok(ApiResponse<AuthResponse>.Success(response, 200, "Login successful"));
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await _authService.ForgotPasswordAsync(request);
        return Ok(ApiResponse<object>.Success(null, 200, "If the email exists, a reset link has been sent"));
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        await _authService.ResetPasswordAsync(request);
        return Ok(ApiResponse<object>.Success(null, 200, "Password reset successful"));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        await _authService.LogoutAsync(request.RefreshToken);
        return Ok(ApiResponse<object>.Success(null, 200, "Logout successful"));
    }
}
