using Nexus.Fulfillment.Application;
using Nexus.Fulfillment.Infrastructure;
using Nexus.Fulfillment.Infrastructure.Persistence;
using Nexus.AspNetCore.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.AddNexusApi("fulfillment");
builder.Services.AddFulfillmentInfrastructure(builder.Configuration);
builder.Services.AddFulfillmentApplication();
builder.AddNexusDbHealthCheck<FulfillmentDbContext>();

var app = builder.Build();
app.UseNexusApi("fulfillment");

app.Run();
