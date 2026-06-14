using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Nexus.User.Api.Health;

public static class HealthEndpointExtensions
{
    public static IEndpointRouteBuilder MapNexusServiceHealth(
        this IEndpointRouteBuilder endpoints,
        string serviceName)
    {
        endpoints.MapGet("/api/v1/health", () => Results.Ok(new ServiceHealthResponse("UP", serviceName)))
            .WithName("GetServiceHealth")
            .WithTags("Health")
            .Produces<ServiceHealthResponse>(StatusCodes.Status200OK);

        return endpoints;
    }
}
