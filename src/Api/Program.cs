using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using FluentValidation;
using FluentValidation.AspNetCore;
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
// Bind Email options and allow SmtpPass to be provided via environment variable (SMTP_PASS)
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
builder.Services.PostConfigure<EmailOptions>(opts =>
{
    if (string.IsNullOrWhiteSpace(opts.SmtpPass))
    {
        var envPass = Environment.GetEnvironmentVariable("SMTP_PASS");
        var configPass = builder.Configuration["Email:SmtpPass"];
        opts.SmtpPass = envPass ?? configPass ?? string.Empty;
    }
});
builder.Services.AddUserInfrastructure(builder.Configuration);
builder.Services.AddUserApplication();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddHostedService<TokenCleanupService>();
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
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Nexus.User.Application.Validators.RegisterRequestValidator>();

builder.AddNexusDbHealthCheck<UserDbContext>();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.UseUserApi();
app.MapGrpcService<UserGrpcService>();
app.UseNexusApi("user");

app.Run();
