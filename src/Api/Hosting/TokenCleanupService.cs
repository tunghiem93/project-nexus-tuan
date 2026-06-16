using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nexus.User.Infrastructure.Persistence;

namespace Nexus.User.Api.Hosting;

public sealed class TokenCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TokenCleanupService> _logger;
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(1);

    public TokenCleanupService(IServiceScopeFactory scopeFactory, ILogger<TokenCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Token cleanup background service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredEntriesAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cleaning up expired auth records.");
            }

            await Task.Delay(CleanupInterval, stoppingToken);
        }
    }

    private async Task CleanupExpiredEntriesAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        var now = DateTime.UtcNow;

        var expiredSessions = await dbContext.UserSessions
            .Where(s => s.RefreshExpiresAt.HasValue && s.RefreshExpiresAt < now)
            .ToListAsync(cancellationToken);

        var expiredResetTokens = await dbContext.PasswordResetTokens
            .Where(t => t.ExpiresAt < now || t.Status == "USED")
            .ToListAsync(cancellationToken);

        var expiredVerifications = await dbContext.EmailVerifications
            .Where(v => v.ExpiresAt < now || v.Status == "VERIFIED")
            .ToListAsync(cancellationToken);

        if (!expiredSessions.Any() && !expiredResetTokens.Any() && !expiredVerifications.Any())
        {
            return;
        }

        dbContext.UserSessions.RemoveRange(expiredSessions);
        dbContext.PasswordResetTokens.RemoveRange(expiredResetTokens);
        dbContext.EmailVerifications.RemoveRange(expiredVerifications);
        await dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Cleaned up expired auth records: {sessions} sessions, {tokens} reset tokens, {verifications} verifications.",
            expiredSessions.Count,
            expiredResetTokens.Count,
            expiredVerifications.Count);
    }
}
