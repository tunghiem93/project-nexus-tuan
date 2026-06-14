# .NET / ASP.NET Core — Project Nexus

## Building blocks

| Project | Role |
|---------|------|
| `Nexus.Abstractions` | `Entity`, `IRepository<T>`, `OutboxMessage` |
| `Nexus.User.Persistence` | `NexusDbContext`, `EfRepository<T>`, `AddSqlServerPersistence<TContext>` |
| `src/Api/Hosting` | `AddNexusApi`, `AddNexusDbHealthCheck`, `UseNexusApi`, `/api/v1/health`, `/swagger` (dev) |

## Service layout

```
src/
├── Api/                    → REST API and gRPC surface
├── Application/            → business logic services
├── Domain/                 → entities and domain rules
├── Infrastructure/         → EF Core configuration and persistence
├── Contracts/              → DTOs and API contracts
└── Shared/                 → shared abstractions and persistence helpers
```

## EF Core + SQL Server

```csharp
// InfrastructureServiceCollectionExtensions.cs
services.AddSqlServerPersistence<UserDbContext>(connectionString);
```

```powershell
dotnet ef migrations add InitialCreate \
  --project src/Infrastructure/Nexus.User.Infrastructure.csproj \
  --startup-project src/Api/Nexus.User.Api.csproj
dotnet ef database update \
  --project src/Infrastructure/Nexus.User.Infrastructure.csproj \
  --startup-project src/Api/Nexus.User.Api.csproj
```

`db/schema.sql` remains the review source of truth for DDL.

## API versioning

Public endpoints: `/api/v1/...`  
OpenAPI: `api-specs/<service>/openapi.yaml`

## Authentication (User Service)

Package: `Microsoft.AspNetCore.Authentication.JwtBearer`

Policy example: `PRODUCT.CREATE` → claim `privilege`.

## Outbox

Table `dbo.outbox_events` mapped to `OutboxMessage` on `NexusDbContext`. Background worker publishes to RabbitMQ (see SRS).

## Implement order

1. User — IAM, JWT, RBAC  
2. Catalog  
3. Fulfillment  
4. Commerce  
5. Notification  
6. Auction  
