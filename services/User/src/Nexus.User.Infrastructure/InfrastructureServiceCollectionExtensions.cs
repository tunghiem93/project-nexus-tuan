using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexus.User.Infrastructure.Persistence;
using Nexus.Persistence.DependencyInjection;

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
        return services;
    }
}
