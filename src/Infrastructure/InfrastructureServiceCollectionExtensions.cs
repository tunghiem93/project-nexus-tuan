using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexus.User.Persistence;
using StackExchange.Redis;
using Nexus.User.Application;
using Nexus.User.Application.Services;
using Nexus.User.Infrastructure.Persistence;
using Nexus.User.Infrastructure.Services;

namespace Nexus.User.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddUserInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("UserDb")
            ?? throw new InvalidOperationException("Connection string 'UserDb' is not configured.");

        services.AddSqlServerPersistence<UserDbContext>(connectionString);
        services.AddHttpContextAccessor();
        services.AddScoped<CurrentUser>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddHttpClient<IGoogleTokenVerifier, GoogleTokenVerifier>();
        services.AddHttpClient<IFacebookTokenVerifier, FacebookTokenVerifier>();

        var redisConnection = configuration.GetSection("Redis")["Connection"];
        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            var mux = ConnectionMultiplexer.Connect(redisConnection);
            services.AddSingleton<IConnectionMultiplexer>(mux);
            services.AddSingleton<IUserCache, RedisUserCache>();
        }
        else
        {
            services.AddScoped<IUserCache, InMemoryUserCache>();
        }
        return services;
    }
}
