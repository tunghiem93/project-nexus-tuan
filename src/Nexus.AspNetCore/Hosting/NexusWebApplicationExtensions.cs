using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Nexus.AspNetCore.Health;
using Nexus.Persistence;

namespace Nexus.AspNetCore.Hosting;

public static class NexusWebApplicationExtensions
{
    public static WebApplicationBuilder AddNexusApi(this WebApplicationBuilder builder, string serviceId)
    {
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
                options.JsonSerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
            });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = $"Nexus {FormatServiceName(serviceId)} API",
                Version = "v1",
                Description = $"Project Nexus — {serviceId} microservice"
            });
        });

        return builder;
    }

    public static WebApplicationBuilder AddNexusDbHealthCheck<TContext>(
        this WebApplicationBuilder builder,
        string name = "database")
        where TContext : NexusDbContext
    {
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<TContext>(name);
        return builder;
    }

    public static WebApplication UseNexusApi(this WebApplication app, string serviceId)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                options.DocumentTitle = $"Nexus {FormatServiceName(serviceId)} API";
            });
        }

        app.UseHttpsRedirection();
        app.MapControllers();
        app.MapHealthChecks("/health");
        app.MapNexusServiceHealth(serviceId);

        SwaggerBrowserLauncher.Register(app);

        return app;
    }

    private static string FormatServiceName(string serviceId) =>
        string.IsNullOrEmpty(serviceId)
            ? "Service"
            : char.ToUpperInvariant(serviceId[0]) + serviceId[1..];
}
