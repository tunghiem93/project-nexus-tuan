using Nexus.Commerce.Application;
using Nexus.Commerce.Infrastructure;
using Nexus.Commerce.Infrastructure.Persistence;
using Nexus.AspNetCore.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.AddNexusApi("commerce");
builder.Services.AddCommerceInfrastructure(builder.Configuration);
builder.Services.AddCommerceApplication();
builder.AddNexusDbHealthCheck<CommerceDbContext>();

var app = builder.Build();
app.UseNexusApi("commerce");

app.Run();
