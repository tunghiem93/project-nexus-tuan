using Microsoft.Extensions.DependencyInjection;

namespace Nexus.Notification.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddNotificationApplication(this IServiceCollection services) => services;
}
