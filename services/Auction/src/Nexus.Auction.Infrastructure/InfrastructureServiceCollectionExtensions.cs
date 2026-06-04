using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Auction.Infrastructure.Persistence;
using Nexus.Persistence.DependencyInjection;

namespace Nexus.Auction.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddAuctionInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AuctionDb")
            ?? throw new InvalidOperationException("Connection string 'AuctionDb' is not configured.");

        services.AddSqlServerPersistence<AuctionDbContext>(connectionString);
        return services;
    }
}
