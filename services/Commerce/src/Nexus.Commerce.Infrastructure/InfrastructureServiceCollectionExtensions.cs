using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Commerce.Infrastructure.Persistence;
using Nexus.Persistence.DependencyInjection;

namespace Nexus.Commerce.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddCommerceInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CommerceDb")
            ?? throw new InvalidOperationException("Connection string 'CommerceDb' is not configured.");

        services.AddSqlServerPersistence<CommerceDbContext>(connectionString);
        return services;
    }
}
