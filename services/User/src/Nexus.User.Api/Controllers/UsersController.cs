using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexus.User.Application.Services;
using Nexus.User.Api.Services;
using Nexus.User.Contracts.Dtos;

namespace Nexus.User.Api.Controllers;

[Route("api/users")]
[ApiController]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly CurrentUser _currentUser;

    public UsersController(IAuthService authService, CurrentUser currentUser)
    {
        _authService = authService;
        _currentUser = currentUser;
    }

    [HttpPut("me/password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (_currentUser.Id is null)
        {
            return Forbid();
        }

        await _authService.ChangePasswordAsync(_currentUser.Id.Value, request.OldPassword, request.NewPassword, _currentUser.AccessTokenJti ?? string.Empty);
        return NoContent();
    }
}
