using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Fulfillment.Infrastructure.Persistence;
using Nexus.Persistence.DependencyInjection;

namespace Nexus.Fulfillment.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddFulfillmentInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("FulfillmentDb")
            ?? throw new InvalidOperationException("Connection string 'FulfillmentDb' is not configured.");

        services.AddSqlServerPersistence<FulfillmentDbContext>(connectionString);
        return services;
    }
}
