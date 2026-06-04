using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Abstractions.Persistence;

namespace Nexus.Persistence.DependencyInjection;

public static class PersistenceExtensions
{
    public static IServiceCollection AddSqlServerPersistence<TContext>(
        this IServiceCollection services,
        string connectionString)
        where TContext : NexusDbContext
    {
        services.AddDbContext<TContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        return services;
    }
}
