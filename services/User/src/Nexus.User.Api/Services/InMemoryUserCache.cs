using Nexus.User.Application;

namespace Nexus.User.Api.Services;

public class InMemoryUserCache : IUserCache
{
    private readonly Dictionary<string, (string Value, DateTime ExpiresAt)> _store = new();
    private readonly object _lock = new();

    public Task SetAsync(string key, string value, TimeSpan? expiry = null)
    {
        var expiresAt = expiry.HasValue ? DateTime.UtcNow.Add(expiry.Value) : DateTime.MaxValue;
        lock (_lock)
        {
            _store[key] = (value, expiresAt);
        }

        return Task.CompletedTask;
    }
}

