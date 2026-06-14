using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Abstractions.Persistence;

namespace Nexus.User.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSqlServerPersistence<TContext>(
        this IServiceCollection services,
        string connectionString)
        where TContext : DbContext
    {
        services.AddDbContext<TContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<DbContext>(provider =>
            provider.GetRequiredService<TContext>());

        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

        return services;
    }
}
