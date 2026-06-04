using Nexus.User.Application;
using Nexus.User.Infrastructure;
using Nexus.User.Infrastructure.Persistence;
using Nexus.AspNetCore.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.AddNexusApi("user");
builder.Services.AddUserInfrastructure(builder.Configuration);
builder.Services.AddUserApplication();
builder.AddNexusDbHealthCheck<UserDbContext>();

var app = builder.Build();
app.UseNexusApi("user");

app.Run();
