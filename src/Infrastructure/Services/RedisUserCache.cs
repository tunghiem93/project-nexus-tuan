using System.Text;
using StackExchange.Redis;
using Nexus.User.Application;

namespace Nexus.User.Infrastructure.Services;

public class RedisUserCache : IUserCache, IDisposable
{
    private readonly IConnectionMultiplexer _connection;
    private readonly IDatabase _db;
    private bool _disposed;

    public RedisUserCache(IConnectionMultiplexer connection)
    {
        _connection = connection;
        _db = _connection.GetDatabase();
    }

    public Task SetAsync(string key, string value, TimeSpan? expiry = null)
    {
        return _db.StringSetAsync(key, value, expiry);
    }

    public async Task<long> IncrementAsync(string key, long value = 1, TimeSpan? expiry = null)
    {
        var result = await _db.StringIncrementAsync(key, value).ConfigureAwait(false);
        if (expiry.HasValue)
        {
            await _db.KeyExpireAsync(key, expiry).ConfigureAwait(false);
        }

        return result;
    }

    public async Task<string?> GetAsync(string key)
    {
        var val = await _db.StringGetAsync(key).ConfigureAwait(false);
        if (val.IsNull)
        {
            return null;
        }

        return val.ToString();
    }

    public Task RemoveAsync(string key)
    {
        return _db.KeyDeleteAsync(key);
    }

    public void Dispose()
    {
        if (_disposed) return;
        (_connection as IDisposable)?.Dispose();
        _disposed = true;
    }
}
