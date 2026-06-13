using Nexus.Auction.Application;
using Nexus.Auction.Infrastructure;
using Nexus.Auction.Infrastructure.Persistence;
using Nexus.AspNetCore.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.AddNexusApi("auction");
builder.Services.AddAuctionInfrastructure(builder.Configuration);
builder.Services.AddAuctionApplication();
builder.AddNexusDbHealthCheck<AuctionDbContext>();

var app = builder.Build();
app.UseNexusApi("auction");

app.Run();
