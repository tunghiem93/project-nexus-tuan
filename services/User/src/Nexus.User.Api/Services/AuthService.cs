using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Nexus.User.Application;
using Nexus.User.Application.Services;
using Nexus.User.Contracts.Dtos;
using Nexus.User.Domain.Entities;
using Nexus.User.Infrastructure.Persistence;
using RoleEntity = Nexus.User.Domain.Entities.Role;

namespace Nexus.User.Api.Services;

public class AuthService : IAuthService
{
    private readonly UserDbContext _dbContext;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly StackExchange.Redis.IDatabase _cache;
    private readonly AuthOptions _options;

    public AuthService(
        UserDbContext dbContext,
        IJwtTokenService jwtTokenService,
        StackExchange.Redis.IConnectionMultiplexer redis,
        IOptions<AuthOptions> authOptions)
    {
        _dbContext = dbContext;
        _jwtTokenService = jwtTokenService;
        _cache = redis.GetDatabase();
        _options = authOptions.Value;
    }

    public async Task RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (await _dbContext.Users.AnyAsync(u => u.Email == request.Email || u.PhoneNumber == request.Phone, cancellationToken))
        {
            throw new InvalidOperationException("Email or phone already exists.");
        }

        var user = new UserAccount
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PhoneNumber = request.Phone,
            PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password),
            FullName = request.FullName,
            IdentifyNumber = string.IsNullOrWhiteSpace(request.IdentifyNumber) ? null : request.IdentifyNumber,
            Gender = string.IsNullOrWhiteSpace(request.Gender) ? null : request.Gender,
            Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address,
            DateOfBirth = request.DateOfBirth,
            Status = Domain.Enums.UserStatus.Active,
            IsEmailVerified = false,
            FailedLoginCount = 0,
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var role = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Code == "BUYER", cancellationToken);
        if (role is null)
        {
            role = new RoleEntity
            {
                Id = Guid.NewGuid(),
                Code = "BUYER",
                Name = "Buyer",
                IsDeleted = false,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            _dbContext.Roles.Add(role);
        }

        var userRole = new UserRole
        {
            Id = Guid.NewGuid(),
            User = user,
            Role = role,
            AssignedAt = DateTimeOffset.UtcNow
        };

        _dbContext.Users.Add(user);
        _dbContext.UserRoles.Add(userRole);

        var verificationToken = _jwtTokenService.CreateEmailVerificationToken(user);
        _dbContext.EmailVerifications.Add(new EmailVerification
        {
            Id = Guid.NewGuid(),
            User = user,
            VerificationTokenHash = HashToken(verificationToken),
            RequestedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(_options.ResetPasswordTokenExpiryMinutes),
            Status = "PENDING",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        _dbContext.OutboxMessages.Add(new Nexus.Abstractions.Outbox.OutboxMessage
        {
            Id = Guid.NewGuid(),
            AggregateType = "User",
            AggregateId = user.Id,
            EventType = "email.verify",
            Payload = JsonSerializer.Serialize(new { user.Email, token = verificationToken, userId = user.Id }),
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task VerifyEmailAsync(string token, CancellationToken cancellationToken = default)
    {
        var principal = _jwtTokenService.ValidateAccessToken(token);
        var tokenType = principal.FindFirst("token_type")?.Value;
        if (!string.Equals(tokenType, "email_verify", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Invalid email verification token.");
        }

        var subject = principal.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
        if (!Guid.TryParse(subject, out var userId))
        {
            throw new InvalidOperationException("Invalid token payload.");
        }

        var verificationHash = HashToken(token);
        var verification = await _dbContext.EmailVerifications.FirstOrDefaultAsync(v => v.VerificationTokenHash == verificationHash, cancellationToken)
            ?? throw new InvalidOperationException("Email verification request not found.");

        if (verification.Status != "PENDING" || verification.ExpiresAt < DateTimeOffset.UtcNow)
        {
            throw new InvalidOperationException("Verification token is invalid or expired.");
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new InvalidOperationException("User not found.");

        verification.Status = "VERIFIED";
        verification.VerifiedAt = DateTimeOffset.UtcNow;
        verification.UpdatedAt = DateTimeOffset.UtcNow;

        user.IsEmailVerified = true;
        user.EmailVerifiedAt = DateTimeOffset.UtcNow;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        if (!user.Preferences.Any())
        {
            user.Preferences.Add(new UserPreference
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Key = "default_locale",
                Value = "en-US",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == request.UsernameOrEmail || u.PhoneNumber == request.UsernameOrEmail, cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException("Invalid credentials.");
        }

        if (user.LockedUntil.HasValue && user.LockedUntil > DateTimeOffset.UtcNow)
        {
            throw new InvalidOperationException("Account is locked.");
        }

        if (!BCrypt.Net.BCrypt.EnhancedVerify(request.Password, user.PasswordHash))
        {
            user.FailedLoginCount++;
            user.UpdatedAt = DateTimeOffset.UtcNow;
            if (user.FailedLoginCount >= 5)
            {
                user.Status = Domain.Enums.UserStatus.Locked;
                user.LockedUntil = DateTimeOffset.UtcNow.AddMinutes(15);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            throw new InvalidOperationException("Invalid credentials.");
        }

        user.FailedLoginCount = 0;
        user.LastLoginAt = DateTimeOffset.UtcNow;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        var role = user.UserRoles.Select(u => u.Role.Code).FirstOrDefault() ?? "BUYER";
        var scope = role.ToLowerInvariant();
        var accessToken = _jwtTokenService.CreateAccessToken(user, role, scope, out var accessJti);
        var refreshToken = _jwtTokenService.CreateRefreshToken();
        var accessHash = HashToken(accessToken);
        var refreshHash = HashToken(refreshToken);

        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = accessHash,
            AccessJti = accessJti,
            RefreshTokenHash = refreshHash,
            RefreshExpiresAt = DateTimeOffset.UtcNow.AddDays(_options.RefreshTokenExpiryDays),
            LoginAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(_options.AccessTokenExpiryMinutes),
            Status = "ACTIVE",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.UserSessions.Add(session);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await CacheSessionAsync(session, user.Id, role, scope);

        return new AuthResponse
        {
            AccessToken = accessToken,
            AccessTokenExpiresIn = _options.AccessTokenExpiryMinutes * 60,
            RefreshToken = refreshToken,
            RefreshTokenExpiresIn = _options.RefreshTokenExpiryDays * 24 * 60 * 60
        };
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var refreshHash = HashToken(refreshToken);
        var session = await _dbContext.UserSessions
            .Include(s => s.User)
            .ThenInclude(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(s => s.RefreshTokenHash == refreshHash, cancellationToken);

        if (session is null || session.Status != "ACTIVE" || session.ExpiresAt < DateTimeOffset.UtcNow)
        {
            throw new InvalidOperationException("Refresh token is invalid or expired.");
        }

        var user = session.User;
        var role = user.UserRoles.Select(u => u.Role.Code).FirstOrDefault() ?? "BUYER";
        var scope = role.ToLowerInvariant();
        var accessToken = _jwtTokenService.CreateAccessToken(user, role, scope, out var accessJti);
        var newRefreshToken = _jwtTokenService.CreateRefreshToken();
        var newRefreshHash = HashToken(newRefreshToken);
        var newTokenHash = HashToken(accessToken);

        session.TokenHash = newTokenHash;
        session.AccessJti = accessJti;
        session.RefreshTokenHash = newRefreshHash;
        session.RefreshExpiresAt = DateTimeOffset.UtcNow.AddDays(_options.RefreshTokenExpiryDays);
        session.ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(_options.AccessTokenExpiryMinutes);
        session.UpdatedAt = DateTimeOffset.UtcNow;

        _dbContext.UserSessions.Update(session);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await BlacklistRefreshHashAsync(refreshHash, session.RefreshExpiresAt - DateTimeOffset.UtcNow);
        await CacheSessionAsync(session, user.Id, role, scope);

        return new AuthResponse
        {
            AccessToken = accessToken,
            AccessTokenExpiresIn = _options.AccessTokenExpiryMinutes * 60,
            RefreshToken = newRefreshToken,
            RefreshTokenExpiresIn = _options.RefreshTokenExpiryDays * 24 * 60 * 60
        };
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (user is null)
        {
            return;
        }

        var token = _jwtTokenService.CreateRefreshToken();
        var tokenHash = HashToken(token);
        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenHash,
            RequestedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(_options.ResetPasswordTokenExpiryMinutes),
            Status = "PENDING",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.PasswordResetTokens.Add(resetToken);
        _dbContext.OutboxMessages.Add(new Nexus.Abstractions.Outbox.OutboxMessage
        {
            Id = Guid.NewGuid(),
            AggregateType = "User",
            AggregateId = user.Id,
            EventType = "email.reset-password",
            Payload = JsonSerializer.Serialize(new { user.Email, token, userId = user.Id }),
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(request.Token);
        var resetToken = await _dbContext.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (resetToken is null || resetToken.Status != "PENDING" || resetToken.ExpiresAt < DateTimeOffset.UtcNow)
        {
            throw new InvalidOperationException("Reset token is invalid or expired.");
        }

        var user = resetToken.User;
        user.PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(request.NewPassword);
        user.UpdatedAt = DateTimeOffset.UtcNow;
        resetToken.Status = "USED";
        resetToken.UsedAt = DateTimeOffset.UtcNow;
        resetToken.UpdatedAt = DateTimeOffset.UtcNow;

        var sessions = await _dbContext.UserSessions.Where(s => s.UserId == user.Id && s.Status == "ACTIVE").ToListAsync(cancellationToken);
        foreach (var session in sessions)
        {
            session.Status = "REVOKED";
            session.LogoutAt = DateTimeOffset.UtcNow;
            session.UpdatedAt = DateTimeOffset.UtcNow;
            if (!string.IsNullOrWhiteSpace(session.AccessJti))
            {
                await BlacklistAccessJtiAsync(session.AccessJti, TimeSpan.FromMinutes(_options.AccessTokenExpiryMinutes));
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ChangePasswordAsync(Guid userId, string oldPassword, string newPassword, string accessTokenJti, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
        {
            throw new InvalidOperationException("User not found.");
        }

        if (!BCrypt.Net.BCrypt.EnhancedVerify(oldPassword, user.PasswordHash))
        {
            throw new InvalidOperationException("Old password is incorrect.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(newPassword);
        user.UpdatedAt = DateTimeOffset.UtcNow;

        var sessions = await _dbContext.UserSessions
            .Where(s => s.UserId == user.Id && s.Status == "ACTIVE" && s.AccessJti != accessTokenJti)
            .ToListAsync(cancellationToken);

        foreach (var session in sessions)
        {
            session.Status = "REVOKED";
            session.LogoutAt = DateTimeOffset.UtcNow;
            session.UpdatedAt = DateTimeOffset.UtcNow;
            if (!string.IsNullOrWhiteSpace(session.AccessJti))
            {
                await BlacklistAccessJtiAsync(session.AccessJti, TimeSpan.FromMinutes(_options.AccessTokenExpiryMinutes));
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash);
    }

    private async Task CacheSessionAsync(UserSession session, Guid userId, string role, string scope)
    {
        var cacheKey = GetSessionCacheKey(session.Id);
        var payload = JsonSerializer.Serialize(new { session.UserId, role, scope, session.AccessJti, session.ExpiresAt });
        await _cache.StringSetAsync(cacheKey, payload, session.ExpiresAt - DateTimeOffset.UtcNow);
    }

    private async Task BlacklistRefreshHashAsync(string refreshHash, TimeSpan expiry)
    {
        if (string.IsNullOrWhiteSpace(refreshHash))
        {
            return;
        }

        await _cache.StringSetAsync(GetRefreshBlacklistKey(refreshHash), "1", expiry);
    }

    private async Task BlacklistAccessJtiAsync(string jti, TimeSpan expiry)
    {
        if (string.IsNullOrWhiteSpace(jti))
        {
            return;
        }

        await _cache.StringSetAsync(GetAccessBlacklistKey(jti), "1", expiry);
    }

    private static string GetSessionCacheKey(Guid sessionId) => $"user:session:{sessionId}";
    private static string GetRefreshBlacklistKey(string tokenHash) => $"auth:refresh:blacklist:{tokenHash}";
    private static string GetAccessBlacklistKey(string jti) => $"auth:access:blacklist:{jti}";
}
