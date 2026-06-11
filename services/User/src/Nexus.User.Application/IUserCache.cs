namespace Nexus.User.Application;

public interface IUserCache
{
    Task SetAsync(string key, string value, TimeSpan? expiry = null);
}
