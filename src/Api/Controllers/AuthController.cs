using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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
        await _authService.RegisterAsync(request);
        return NoContent();
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        await _authService.VerifyEmailAsync(request.Code);
        return NoContent();
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        catch (InvalidOperationException ex) when (ex.Message == "Account is locked.")
        {
            return StatusCode(423, new { error = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Too many attempts"))
        {
            return StatusCode(429, new { error = ex.Message });
        }
        catch (InvalidOperationException)
        {
            return Unauthorized();
        }
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var response = await _authService.RefreshTokenAsync(request.RefreshToken);
        return Ok(response);
    }

    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
    {
        var response = await _authService.SendLoginOtpAsync(request);
        return Ok(response);
        //return NoContent();
    }

    [HttpPost("login-otp")]
    public async Task<ActionResult<AuthResponse>> LoginWithOtp([FromBody] LoginOtpRequest request)
    {
        var response = await _authService.LoginWithOtpAsync(request);
        return Ok(response);
    }

    [HttpPost("oauth")]
    public async Task<ActionResult<AuthResponse>> OAuthLogin([FromBody] OAuthLoginRequest request)
    {
        var response = await _authService.OAuthLoginAsync(request);
        return Ok(response);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await _authService.ForgotPasswordAsync(request);
        return NoContent();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        await _authService.ResetPasswordAsync(request);
        return NoContent();
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        await _authService.LogoutAsync(request.RefreshToken);
        return NoContent();
    }
}
