using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Nexus.User.Api.Hosting;

internal static class SwaggerBrowserLauncher
{
    private const string EnvVar = "NEXUS_OPEN_SWAGGER";

    public static void Register(WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            return;

        // Mặc định mở Swagger khi Development; đặt NEXUS_OPEN_SWAGGER=false để tắt.
        if (string.Equals(Environment.GetEnvironmentVariable(EnvVar), "false", StringComparison.OrdinalIgnoreCase))
            return;

        app.Lifetime.ApplicationStarted.Register(() => TryOpenSwagger(app));
    }

    private static void TryOpenSwagger(WebApplication app)
    {
        try
        {
            var baseUrl = app.Urls.FirstOrDefault(u =>
                u.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                ?? app.Urls.FirstOrDefault();

            if (string.IsNullOrEmpty(baseUrl))
                return;

            var swaggerUrl = $"{baseUrl.TrimEnd('/')}/swagger";
            Process.Start(new ProcessStartInfo(swaggerUrl) { UseShellExecute = true });
        }
        catch
        {
            // Best effort — dev convenience only.
        }
    }
}
