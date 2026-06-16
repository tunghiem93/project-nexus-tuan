using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Nexus.User.Application;
using Nexus.User.Application.Services;
using Nexus.User.Application.Dtos;
using Nexus.User.Domain.Entities;
using Nexus.User.Infrastructure.Persistence;
using Nexus.User.Infrastructure.Services;
using RoleEntity = Nexus.User.Domain.Entities.Role;

namespace Nexus.User.Api.Services;

public class AuthService : IAuthService
{
    private readonly UserDbContext _dbContext;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUserCache _cache;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AuthOptions _options;
    private readonly IEmailSender _emailSender;
    private readonly EmailOptions _emailOptions;
    private readonly IGoogleTokenVerifier _googleTokenVerifier;
    private readonly IFacebookTokenVerifier _facebookTokenVerifier;

    public AuthService(
        UserDbContext dbContext,
        IJwtTokenService jwtTokenService,
        IUserCache cache,
        IHttpContextAccessor httpContextAccessor,
        IOptions<AuthOptions> authOptions,
        IEmailSender emailSender,
        IOptions<EmailOptions> emailOptions,
        IGoogleTokenVerifier googleTokenVerifier,
        IFacebookTokenVerifier facebookTokenVerifier)
    {
        _dbContext = dbContext;
        _jwtTokenService = jwtTokenService;
        _cache = cache;
        _httpContextAccessor = httpContextAccessor;
        _options = authOptions.Value;
        _emailSender = emailSender;
        _emailOptions = emailOptions.Value;
        _googleTokenVerifier = googleTokenVerifier;
        _facebookTokenVerifier = facebookTokenVerifier;
    }

    public async Task RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(request.Phone)
            && !Regex.IsMatch(request.Phone, "^(0\\d{9})$"))
        {
            throw new InvalidOperationException("SDT khong hop le");
        }

        if (string.IsNullOrWhiteSpace(request.Password) || !IsPasswordValid(request.Password))
        {
            throw new InvalidOperationException("Password must be at least 8 characters long and include at least one uppercase letter, one digit, and one special character.");
        }

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
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
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
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _dbContext.Roles.Add(role);
        }

        var userRole = new UserRole
        {
            Id = Guid.NewGuid(),
            User = user,
            Role = role,
            AssignedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        _dbContext.UserRoles.Add(userRole);

        var verificationCode = GenerateVerificationCode();
        _dbContext.EmailVerifications.Add(new EmailVerification
        {
            Id = Guid.NewGuid(),
            User = user,
            VerificationTokenHash = HashToken(verificationCode),
            RequestedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_options.EmailVerifyTokenExpiryMinutes),
            Status = "PENDING",
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            await _emailSender.SendEmailAsync(
                user.Email,
                "Xác minh email đăng ký",
                GetEmailVerificationBody(user.FullName, verificationCode));

        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task VerifyEmailAsync(string code, CancellationToken cancellationToken = default)
    {
        var verificationHash = HashToken(code);
        var verification = await _dbContext.EmailVerifications
            .Include(v => v.User)
            .FirstOrDefaultAsync(v => v.VerificationTokenHash == verificationHash, cancellationToken)
            ?? throw new InvalidOperationException("Email verification request not found.");

        if (verification.Status != "PENDING" || verification.ExpiresAt < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Verification code is invalid or expired.");
        }

        var user = verification.User;
        verification.Status = "VERIFIED";
        verification.VerifiedAt = DateTime.UtcNow;

        user.IsEmailVerified = true;
        user.EmailVerifiedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;        

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        const int MaxAttempts = 5;
        var AttemptWindow = TimeSpan.FromMinutes(15);
        var BlockDuration = TimeSpan.FromMinutes(15);

        var ip = GetClientIp();
        if (!string.IsNullOrWhiteSpace(ip))
        {
            var blocked = await _cache.GetAsync(GetBlockKey(ip));
            if (!string.IsNullOrWhiteSpace(blocked))
            {
                throw new InvalidOperationException("Too many attempts from your IP. Try again later.");
            }
        }

        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == request.UsernameOrEmail || u.PhoneNumber == request.UsernameOrEmail, cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException("Invalid credentials.");
        }

        if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
        {
            throw new InvalidOperationException("Account is locked.");
        }

        if (!BCrypt.Net.BCrypt.EnhancedVerify(request.Password, user.PasswordHash))
        {
            user.FailedLoginCount++;
            user.UpdatedAt = DateTime.UtcNow;
            if (user.FailedLoginCount >= MaxAttempts)
            {
                user.Status = Domain.Enums.UserStatus.Locked;
                user.LockedUntil = DateTime.UtcNow.AddMinutes(BlockDuration.TotalMinutes);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(ip))
            {
                var attempts = await _cache.IncrementAsync(GetAttemptsKey(ip), 1, AttemptWindow);
                if (attempts >= MaxAttempts)
                {
                    await _cache.SetAsync(GetBlockKey(ip), "1", BlockDuration);
                }
            }

            throw new InvalidOperationException("Invalid credentials.");
        }

        user.FailedLoginCount = 0;
        user.Status = Domain.Enums.UserStatus.Active;
        user.LockedUntil = null;
        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        var clientIp = GetClientIp();
        if (!string.IsNullOrWhiteSpace(clientIp))
        {
            await _cache.RemoveAsync(GetAttemptsKey(clientIp));
            await _cache.RemoveAsync(GetBlockKey(clientIp));
        }

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
            RefreshExpiresAt = DateTime.UtcNow.AddDays(_options.RefreshTokenExpiryDays),
            LoginAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_options.AccessTokenExpiryMinutes),
            Status = "ACTIVE",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
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

    public async Task SendLoginOtpAsync(SendOtpRequest request, CancellationToken cancellationToken = default)
    {
        var phone = NormalizePhoneNumber(request.PhoneNumber);
        if (!Regex.IsMatch(phone, "^(0\\d{9})$"))
        {
            throw new InvalidOperationException("SDT khong hop le");
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone, cancellationToken);
        if (user is null)
        {
            return;
        }

        var attemptsKey = GetOtpAttemptsKey(phone);
        var attempts = await _cache.IncrementAsync(attemptsKey, 1, TimeSpan.FromMinutes(_options.OtpAttemptWindowMinutes));
        if (attempts > _options.OtpAttemptLimit)
        {
            throw new InvalidOperationException("Too many OTP requests. Try again later.");
        }

        var otp = GenerateVerificationCode();
        await _cache.SetAsync(GetOtpKey(phone), otp, TimeSpan.FromMinutes(_options.OtpExpiryMinutes));

        Console.WriteLine($"[DEV OTP] phone={phone}, otp={otp}");

        _dbContext.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            ActorUserId = user.Id,
            Action = "OTP_SENT",
            TargetType = "User",
            TargetRefId = user.Id,
            DetailJson = JsonSerializer.Serialize(new { user.PhoneNumber }),
            IpAddress = GetClientIp(),
            CreatedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<AuthResponse> LoginWithOtpAsync(LoginOtpRequest request, CancellationToken cancellationToken = default)
    {
        var phone = NormalizePhoneNumber(request.PhoneNumber);
        if (!Regex.IsMatch(phone, "^(0\\d{9})$"))
        {
            throw new InvalidOperationException("SDT khong hop le");
        }

        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.PhoneNumber == phone, cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException("Invalid credentials.");
        }

        var expectedOtp = await _cache.GetAsync(GetOtpKey(phone));
        if (string.IsNullOrWhiteSpace(expectedOtp) || expectedOtp != request.OtpCode)
        {
            throw new InvalidOperationException("Invalid credentials.");
        }

        await _cache.RemoveAsync(GetOtpKey(phone));

        user.ResetFailedLogin();
        user.Status = Domain.Enums.UserStatus.Active;
        user.LockedUntil = null;
        user.UpdateLastLogin(DateTime.UtcNow);

        var role = user.UserRoles.Select(u => u.Role.Code).FirstOrDefault() ?? "BUYER";
        var scope = role.ToLowerInvariant();

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await CreateSessionAsync(user, role, scope, cancellationToken);
    }

    public async Task<AuthResponse> OAuthLoginAsync(OAuthLoginRequest request, CancellationToken cancellationToken = default)
    {
        var provider = request.Provider.Trim().ToLowerInvariant();
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        OAuthTokenPayload payload = provider switch
        {
            "google" => await _googleTokenVerifier.VerifyAsync(request.IdToken, cancellationToken),
            "facebook" => await _facebookTokenVerifier.VerifyAsync(request.IdToken, cancellationToken),
            _ => throw new InvalidOperationException("Unsupported OAuth provider.")
        };

        if (!string.Equals(payload.Email.Trim().ToLowerInvariant(), normalizedEmail, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("OAuth email does not match the provided email.");
        }

        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user is null)
        {
            user = new UserAccount
            {
                Id = Guid.NewGuid(),
                Email = normalizedEmail,
                FullName = string.IsNullOrWhiteSpace(request.FullName) ? payload.FullName : request.FullName,
                PhoneNumber = null,
                PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(Guid.NewGuid().ToString("N")),
                Status = Domain.Enums.UserStatus.Active,
                IsEmailVerified = payload.EmailVerified,
                EmailVerifiedAt = payload.EmailVerified ? DateTime.UtcNow : null,
                FailedLoginCount = 0,
                IsDeleted = false,
                OAuthProvider = provider,
                OAuthProviderId = payload.ProviderId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
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
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _dbContext.Roles.Add(role);
            }

            var userRole = new UserRole
            {
                Id = Guid.NewGuid(),
                User = user,
                Role = role,
                AssignedAt = DateTime.UtcNow
            };

            _dbContext.Users.Add(user);
            _dbContext.UserRoles.Add(userRole);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        else
        {
            user.OAuthProvider = provider;
            user.OAuthProviderId = payload.ProviderId;
            if (string.IsNullOrWhiteSpace(user.FullName) && !string.IsNullOrWhiteSpace(request.FullName))
            {
                user.FullName = request.FullName;
            }

            user.IsEmailVerified = payload.EmailVerified || user.IsEmailVerified;
            user.EmailVerifiedAt ??= payload.EmailVerified ? DateTime.UtcNow : user.EmailVerifiedAt;
            user.ResetFailedLogin();
            user.Status = Domain.Enums.UserStatus.Active;
            user.LockedUntil = null;
            user.UpdateLastLogin(DateTime.UtcNow);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var userRoleCode = user.UserRoles.Select(u => u.Role.Code).FirstOrDefault() ?? "BUYER";
        var scope = userRoleCode.ToLowerInvariant();
        return await CreateSessionAsync(user, userRoleCode, scope, cancellationToken);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var refreshHash = HashToken(refreshToken);
        var session = await _dbContext.UserSessions
            .Include(s => s.User)
            .ThenInclude(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(s => s.RefreshTokenHash == refreshHash, cancellationToken);

        if (session is null || session.Status != "ACTIVE" || session.ExpiresAt < DateTime.UtcNow)
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
        session.RefreshExpiresAt = DateTime.UtcNow.AddDays(_options.RefreshTokenExpiryDays);
        session.ExpiresAt = DateTime.UtcNow.AddMinutes(_options.AccessTokenExpiryMinutes);
        session.UpdatedAt = DateTime.UtcNow;

        _dbContext.UserSessions.Update(session);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await BlacklistRefreshHashAsync(refreshHash, session.RefreshExpiresAt.HasValue ? session.RefreshExpiresAt.Value - DateTime.UtcNow : TimeSpan.Zero);
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
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var forgotPasswordAttemptsKey = GetForgotPasswordAttemptsKey(normalizedEmail);
        var requestCount = await _cache.IncrementAsync(forgotPasswordAttemptsKey, 1, TimeSpan.FromMinutes(5));
        if (requestCount > 3)
        {
            throw new InvalidOperationException("Too many password reset requests. Try again later.");
        }

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
            RequestedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_options.ResetPasswordTokenExpiryMinutes),
            Status = "PENDING",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.PasswordResetTokens.Add(resetToken);
        _dbContext.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            ActorUserId = null,
            Action = "PASSWORD_RESET_REQUESTED",
            TargetType = "User",
            TargetRefId = user.Id,
            DetailJson = JsonSerializer.Serialize(new { user.Email }),
            IpAddress = GetClientIp(),
            CreatedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _emailSender.SendEmailAsync(
            user.Email,
            "Yêu cầu đặt lại mật khẩu",
            GetPasswordResetBody(user.FullName, token));
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(request.Token);
        var resetToken = await _dbContext.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (resetToken is null || resetToken.Status != "PENDING" || resetToken.ExpiresAt < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Reset token is invalid or expired.");
        }

        var user = resetToken.User;
        if (BCrypt.Net.BCrypt.EnhancedVerify(request.NewPassword, user.PasswordHash))
        {
            throw new InvalidOperationException("New password must be different from the current password.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        _dbContext.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            ActorUserId = user.Id,
            Action = "PASSWORD_RESET_COMPLETED",
            TargetType = "User",
            TargetRefId = user.Id,
            DetailJson = JsonSerializer.Serialize(new { user.Email }),
            IpAddress = GetClientIp(),
            CreatedAt = DateTime.UtcNow
        });

        resetToken.Status = "USED";
        resetToken.UsedAt = DateTime.UtcNow;
        resetToken.UpdatedAt = DateTime.UtcNow;

        var sessions = await _dbContext.UserSessions.Where(s => s.UserId == user.Id && s.Status == "ACTIVE").ToListAsync(cancellationToken);
        foreach (var session in sessions)
        {
            session.Status = "REVOKED";
            session.LogoutAt = DateTime.UtcNow;
            session.UpdatedAt = DateTime.UtcNow;
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
        user.UpdatedAt = DateTime.UtcNow;

        var sessions = await _dbContext.UserSessions
            .Where(s => s.UserId == user.Id && s.Status == "ACTIVE" && s.AccessJti != accessTokenJti)
            .ToListAsync(cancellationToken);

        foreach (var session in sessions)
        {
            session.Status = "REVOKED";
            session.LogoutAt = DateTime.UtcNow;
            session.UpdatedAt = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(session.AccessJti))
            {
                await BlacklistAccessJtiAsync(session.AccessJti, TimeSpan.FromMinutes(_options.AccessTokenExpiryMinutes));
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var refreshHash = HashToken(refreshToken);
        var session = await _dbContext.UserSessions.FirstOrDefaultAsync(s => s.RefreshTokenHash == refreshHash, cancellationToken);
        if (session is null)
        {
            return;
        }

        session.Status = "REVOKED";
        session.LogoutAt = DateTime.UtcNow;
        session.UpdatedAt = DateTime.UtcNow;

        _dbContext.UserSessions.Update(session);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var expiry = session.RefreshExpiresAt.HasValue ? session.RefreshExpiresAt.Value - DateTime.UtcNow : TimeSpan.Zero;
        await BlacklistRefreshHashAsync(refreshHash, expiry);
    }

    private static string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash);
    }

    private static string GenerateVerificationCode()
    {
        var randomNumber = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return randomNumber.ToString("D6");
    }

    private string? GetClientIp()
    {
        try
        {
            var ctx = _httpContextAccessor.HttpContext;
            if (ctx is null) return null;
            // Prefer X-Forwarded-For when behind proxies
            if (ctx.Request.Headers.TryGetValue("X-Forwarded-For", out var vals))
            {
                var first = vals.ToString().Split(',').Select(s => s.Trim()).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(first)) return first;
            }

            return ctx.Connection.RemoteIpAddress?.ToString();
        }
        catch
        {
            return null;
        }
    }

    private static string GetAttemptsKey(string ip) => $"auth:attempts:ip:{ip}";
    private static string GetBlockKey(string ip) => $"auth:block:ip:{ip}";

    private string GetEmailVerificationBody(string fullName, string token)
    {
        var name = string.IsNullOrWhiteSpace(fullName) ? "Người dùng" : fullName;
        var body = new StringBuilder();
        body.AppendLine($"<p>Xin chào {name},</p>");
        body.AppendLine("<p>Cảm ơn bạn đã đăng ký. Vui lòng xác minh email với mã sau:</p>");
        body.AppendLine($"<p><strong>{token}</strong></p>");
        body.AppendLine("<p>Mã này có hiệu lực trong vài phút. Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email này.</p>");
        return body.ToString();
    }

    private string GetPasswordResetBody(string fullName, string token)
    {
        var name = string.IsNullOrWhiteSpace(fullName) ? "Người dùng" : fullName;
        var resetUrl = BuildResetPasswordUrl(token);
        var body = new StringBuilder();
        body.AppendLine($"<p>Xin chào {name},</p>");
        body.AppendLine("<p>Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu. Vui lòng nhấn vào liên kết bên dưới để đặt lại mật khẩu của bạn:</p>");
        body.AppendLine($"<p><a href=\"{resetUrl}\">Đặt lại mật khẩu</a></p>");
        body.AppendLine("<p>Nếu liên kết không hoạt động, hãy sao chép và dán mã sau vào trang đặt lại mật khẩu:</p>");
        body.AppendLine($"<p><strong>{token}</strong></p>");
        return body.ToString();
    }

    private async Task<AuthResponse> CreateSessionAsync(UserAccount user, string role, string scope, CancellationToken cancellationToken)
    {
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
            RefreshExpiresAt = DateTime.UtcNow.AddDays(_options.RefreshTokenExpiryDays),
            LoginAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_options.AccessTokenExpiryMinutes),
            Status = "ACTIVE",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
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

    private string GetPasswordChangedConfirmationBody(string fullName)
    {
        var name = string.IsNullOrWhiteSpace(fullName) ? "Người dùng" : fullName;
        var body = new StringBuilder();
        body.AppendLine($"<p>Xin chào {name},</p>");
        body.AppendLine("<p>Mật khẩu của bạn đã được thay đổi thành công.</p>");
        body.AppendLine("<p>Nếu bạn không thực hiện thay đổi này, vui lòng liên hệ với bộ phận hỗ trợ.</p>");
        return body.ToString();
    }

    private string BuildResetPasswordUrl(string token)
    {
        if (string.IsNullOrWhiteSpace(_emailOptions.AppBaseUrl))
        {
            return token;
        }

        return $"{_emailOptions.AppBaseUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(token)}";
    }

    private async Task CacheSessionAsync(UserSession session, Guid userId, string role, string scope)
    {
        var cacheKey = GetSessionCacheKey(session.Id);
        var payload = JsonSerializer.Serialize(new { session.UserId, role, scope, session.AccessJti, session.ExpiresAt });
        await _cache.SetAsync(cacheKey, payload, session.ExpiresAt - DateTime.UtcNow);
    }

    private async Task BlacklistRefreshHashAsync(string refreshHash, TimeSpan expiry)
    {
        if (string.IsNullOrWhiteSpace(refreshHash))
        {
            return;
        }

        await _cache.SetAsync(GetRefreshBlacklistKey(refreshHash), "1", expiry);
    }

    private static bool IsPasswordValid(string password)
    {
        return Regex.IsMatch(password, "^(?=.*[A-Z])(?=.*\\d)(?=.*[^a-zA-Z0-9]).{8,}$");
    }

    private static string GetForgotPasswordAttemptsKey(string email) => $"forgot_password_attempts:{email}";
    private static string GetOtpKey(string phone) => $"auth:otp:{phone}";
    private static string GetOtpAttemptsKey(string phone) => $"auth:otp:attempts:{phone}";
    private static string NormalizePhoneNumber(string phone) => phone?.Trim() ?? string.Empty;

    private async Task BlacklistAccessJtiAsync(string jti, TimeSpan expiry)
    {
        if (string.IsNullOrWhiteSpace(jti))
        {
            return;
        }

        await _cache.SetAsync(GetAccessBlacklistKey(jti), "1", expiry);
    }

    private static string GetSessionCacheKey(Guid sessionId) => $"user:session:{sessionId}";
    private static string GetRefreshBlacklistKey(string tokenHash) => $"auth:refresh:blacklist:{tokenHash}";
    private static string GetAccessBlacklistKey(string jti) => $"auth:access:blacklist:{jti}";
}

