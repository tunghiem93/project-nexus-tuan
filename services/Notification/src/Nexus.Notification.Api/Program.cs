using Nexus.Notification.Application;
using Nexus.Notification.Infrastructure;
using Nexus.Notification.Infrastructure.Persistence;
using Nexus.AspNetCore.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.AddNexusApi("notification");
builder.Services.AddNotificationInfrastructure(builder.Configuration);
builder.Services.AddNotificationApplication();
builder.AddNexusDbHealthCheck<NotificationDbContext>();

var app = builder.Build();
app.UseNexusApi("notification");

app.Run();
