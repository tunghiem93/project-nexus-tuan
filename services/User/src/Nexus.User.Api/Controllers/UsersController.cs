using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexus.User.Api.Services;
using Nexus.User.Application.Commands;
using Nexus.User.Application.Services;
using Nexus.User.Contracts.Dtos;

namespace Nexus.User.Api.Controllers;

[Route("api/users")]
[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly CurrentUser _currentUser;
    private readonly IMediator _mediator;

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

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new DeleteUserCommand(id),
            cancellationToken);

        return NoContent();
    }
}
