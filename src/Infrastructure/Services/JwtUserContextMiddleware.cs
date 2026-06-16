using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Nexus.User.Infrastructure.Services;

public class JwtUserContextMiddleware
{
    private readonly RequestDelegate _next;

    public JwtUserContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, CurrentUser currentUser)
    {
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var subject = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                       ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = context.User.FindFirst(ClaimTypes.Role)?.Value;
            var scope = context.User.FindFirst("scope")?.Value;
            var jti = context.User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (Guid.TryParse(subject, out var userId))
            {
                currentUser.Id = userId;
            }

            currentUser.Role = role;
            currentUser.Scope = scope;
            currentUser.AccessTokenJti = jti;
        }

        await _next(context);
    }
}
