# Project Nexus — Architecture

Microservices E-Commerce & Auction platform. Each bounded context owns one SQL Server database and deployable API.

## Repository layout

```
├── Directory.Build.props      # Shared MSBuild properties
├── Directory.Packages.props   # Central package versions
├── global.json
├── Nexus.slnx
├── src/                       # Shared libraries (not business logic)
│   ├── Nexus.Abstractions/    # Entities, IRepository
│   ├── Nexus.Persistence/     # EF Core, NexusDbContext, EfRepository
│   └── Nexus.AspNetCore/      # API host conventions, health endpoints
├── services/{Name}/           # One folder per bounded context
│   ├── db/schema.sql
│   ├── README.md
│   └── src/
│       ├── Nexus.{Name}.Api/
│       ├── Nexus.{Name}.Application/
│       ├── Nexus.{Name}.Domain/
│       ├── Nexus.{Name}.Infrastructure/
│       └── Nexus.{Name}.Contracts/
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

Each `Program.cs` follows the same pattern:

```csharp
builder.AddNexusApi("{service-id}");
builder.Services.Add{Name}Infrastructure(builder.Configuration);
builder.Services.Add{Name}Application();
builder.AddNexusDbHealthCheck<{Name}DbContext>();
var app = builder.Build();
app.UseNexusApi("{service-id}");
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
