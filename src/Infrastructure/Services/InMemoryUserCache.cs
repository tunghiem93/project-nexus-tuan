using Nexus.User.Application;

namespace Nexus.User.Infrastructure.Services;

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

    public Task<long> IncrementAsync(string key, long value = 1, TimeSpan? expiry = null)
    {
        lock (_lock)
        {
            if (_store.TryGetValue(key, out var entry) && entry.ExpiresAt > DateTime.UtcNow)
            {
                if (long.TryParse(entry.Value, out var current))
                {
                    current += value;
                    _store[key] = (current.ToString(), entry.ExpiresAt);
                    return Task.FromResult(current);
                }
            }

            var expiresAt = expiry.HasValue ? DateTime.UtcNow.Add(expiry.Value) : DateTime.MaxValue;
            _store[key] = (value.ToString(), expiresAt);
            return Task.FromResult(value);
        }
    }

    public Task<string?> GetAsync(string key)
    {
        lock (_lock)
        {
            if (_store.TryGetValue(key, out var entry) && entry.ExpiresAt > DateTime.UtcNow)
            {
                return Task.FromResult<string?>(entry.Value);
            }

            if (_store.ContainsKey(key))
            {
                _store.Remove(key);
            }

            return Task.FromResult<string?>(null);
        }
    }

    public Task RemoveAsync(string key)
    {
        lock (_lock)
        {
            _store.Remove(key);
        }

        return Task.CompletedTask;
    }
}

