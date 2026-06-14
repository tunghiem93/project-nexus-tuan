using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Nexus.User.Infrastructure.Services;

namespace Nexus.User.Api;

public static class ApiServiceCollectionExtensions
{
    public static WebApplication UseUserApi(this WebApplication app)
    {
        app.UseMiddleware<JwtUserContextMiddleware>();
        return app;
    }
}
