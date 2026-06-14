using Microsoft.Extensions.DependencyInjection;
using Nexus.User.Application.Services;

namespace Nexus.User.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddUserApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserQueryService, UserQueryService>();
        return services;
    }
}
