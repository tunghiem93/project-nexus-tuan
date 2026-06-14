namespace Nexus.User.Application;

public interface IUserCache
{
    Task SetAsync(string key, string value, TimeSpan? expiry = null);
    Task<long> IncrementAsync(string key, long value = 1, TimeSpan? expiry = null);
    Task<string?> GetAsync(string key);
    Task RemoveAsync(string key);
}
