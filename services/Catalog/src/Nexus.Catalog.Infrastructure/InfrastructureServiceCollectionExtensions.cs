using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Catalog.Infrastructure.Persistence;
using Nexus.Persistence.DependencyInjection;

namespace Nexus.Catalog.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddCatalogInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CatalogDb")
            ?? throw new InvalidOperationException("Connection string 'CatalogDb' is not configured.");

        services.AddSqlServerPersistence<CatalogDbContext>(connectionString);
        return services;
    }
}
