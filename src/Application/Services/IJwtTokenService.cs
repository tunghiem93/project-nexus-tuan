using System.Security.Claims;
using Nexus.User.Domain.Entities;

namespace Nexus.User.Application.Services;

public interface IJwtTokenService
{
    string CreateAccessToken(UserAccount user, string role, string scope, out string jti);
    string CreateEmailVerificationToken(UserAccount user);
    ClaimsPrincipal ValidateAccessToken(string token);
    string CreateRefreshToken();
}
