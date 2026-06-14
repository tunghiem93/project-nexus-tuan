using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Nexus.User.Application;
using Nexus.User.Application.Services;
using Nexus.User.Domain.Entities;

namespace Nexus.User.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly AuthOptions _options;
    private readonly SigningCredentials _signingCredentials;
    private readonly TokenValidationParameters _tokenValidationParameters;

    public JwtTokenService(IOptions<AuthOptions> options)
    {
        _options = options.Value;
        var secretValue = _options.Secret ?? throw new InvalidOperationException("Auth:Secret is required.");
        var secretBytes = Encoding.UTF8.GetBytes(secretValue);
        if (secretBytes.Length < 32)
        {
            throw new InvalidOperationException("Auth:Secret must be at least 32 bytes when UTF-8 encoded.");
        }

        var key = new SymmetricSecurityKey(secretBytes);
        _signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        _tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _options.Issuer,
            ValidAudience = _options.Audience,
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    }

    public string CreateAccessToken(UserAccount user, string role, string scope, out string jti)
    {
        jti = Guid.NewGuid().ToString();

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, role),
            new Claim("scope", scope),
            new Claim(JwtRegisteredClaimNames.Jti, jti)
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_options.AccessTokenExpiryMinutes),
            signingCredentials: _signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal ValidateAccessToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(token, _tokenValidationParameters, out var validatedToken);

        if (validatedToken is not JwtSecurityToken jwt || !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.Ordinal))
        {
            throw new SecurityTokenException("Invalid token signature.");
        }

        return principal;
    }

    public string CreateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}
