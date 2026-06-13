using Nexus.Catalog.Application;
using Nexus.Catalog.Infrastructure;
using Nexus.Catalog.Infrastructure.Persistence;
using Nexus.AspNetCore.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.AddNexusApi("catalog");
builder.Services.AddCatalogInfrastructure(builder.Configuration);
builder.Services.AddCatalogApplication();
builder.AddNexusDbHealthCheck<CatalogDbContext>();

var app = builder.Build();
app.UseNexusApi("catalog");

app.Run();
