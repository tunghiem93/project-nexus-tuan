# Project Nexus — Architecture

Microservices E-Commerce & Auction platform. Each bounded context owns one SQL Server database and deployable API.

## Repository layout

```
├── Directory.Build.props      # Shared MSBuild properties
├── Directory.Packages.props   # Central package versions
├── global.json
├── Nexus.slnx
├── src/                       # Shared libraries and service implementation
│   ├── Shared/                # Common helpers and persistence base
│   ├── Domain/                # Entities and domain rules
│   ├── Application/           # Business use cases and service contracts
│   ├── Infrastructure/        # EF Core configuration and persistence
│   └── Api/                   # HTTP/gRPC surface and host glue
│       └── Hosting/           # API host conventions, health endpoints
├── api-specs/
├── docs/
└── infra/
```

## Layer rules

| Layer | References | Responsibility |
|-------|------------|----------------|
| **Domain** | Abstractions only | Entities, enums, domain rules |
| **Contracts** | — | DTOs, API models (no EF) |
| **Application** | Domain, Contracts, Abstractions | Use cases, orchestration |
| **Infrastructure** | Domain, Persistence | DbContext, EF configurations |
| **Api** | Application, Infrastructure, AspNetCore | HTTP, DI composition root |

Domain **must not** reference EF Core or ASP.NET.

## Persistence

- `NexusDbContext` — base context with `OutboxMessages` (`dbo.outbox_events`)
- `{Name}DbContext : NexusDbContext` — per-service DbSets
- `AddSqlServerPersistence<TContext>(connectionString)` registers `IRepository<>`

## API host

The `Program.cs` follows this pattern for the User service:

```csharp
builder.AddNexusApi("user");
builder.Services.AddUserInfrastructure(builder.Configuration);
builder.Services.AddUserApplication();
builder.AddNexusDbHealthCheck<UserDbContext>();
var app = builder.Build();
app.UseNexusApi("user");
```

- Liveness: `GET /health` (ASP.NET health checks + DB)
- Business health: `GET /api/v1/health`
- Swagger UI (Development): `GET /swagger`

## Services

| Service | Database | Port |
|---------|----------|------|
| User | Nexus_User | 8081 |
| Catalog | Nexus_Catalog | 8082 |
| Commerce | Nexus_Commerce | 8083 |
| Auction | Nexus_Auction | 8084 |
| Fulfillment | Nexus_Fulfillment | 8085 |
| Notification | Nexus_Notification | 8086 |

See [docs/architecture/dotnet-guide.md](docs/architecture/dotnet-guide.md) for EF migrations and JWT guidance.
