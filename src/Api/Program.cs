using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Nexus.User.Application;
using Nexus.User.Application.Services;
using Nexus.User.Api;
using Nexus.User.Api.Grpc;
using Nexus.User.Api.Services;
using Nexus.User.Infrastructure.Services;
using Nexus.User.Infrastructure;
using Nexus.User.Infrastructure.Persistence;
using Nexus.User.Api.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.AddNexusApi("user");

builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection("Auth"));
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
builder.Services.AddUserInfrastructure(builder.Configuration);
builder.Services.AddUserApplication();
builder.Services.AddScoped<IAuthService, AuthService>();
// The API layer only exposes host-specific middleware and routing.
// Infrastructure services like JWT, email, cache, and user context are registered in Infrastructure.

var authOptions = builder.Configuration.GetSection("Auth").Get<AuthOptions>() ?? new AuthOptions();
var secret = Encoding.UTF8.GetBytes(authOptions.Secret ?? throw new InvalidOperationException("Auth:Secret must be configured."));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = authOptions.Issuer,
            ValidAudience = authOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(secret),
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddGrpc();

builder.AddNexusDbHealthCheck<UserDbContext>();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.UseUserApi();
app.MapGrpcService<UserGrpcService>();
app.UseNexusApi("user");

app.Run();
