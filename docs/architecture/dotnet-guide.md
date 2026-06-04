# .NET / ASP.NET Core — Project Nexus

## Building blocks

| Project | Role |
|---------|------|
| `Nexus.Abstractions` | `Entity`, `IRepository<T>`, `OutboxMessage` |
| `Nexus.Persistence` | `NexusDbContext`, `EfRepository<T>`, `AddSqlServerPersistence<TContext>` |
| `Nexus.AspNetCore` | `AddNexusApi`, `AddNexusDbHealthCheck`, `UseNexusApi`, `/api/v1/health`, `/swagger` (dev) |

## Service layout

```
services/User/src/
├── Nexus.User.Api/
├── Nexus.User.Application/     → ApplicationServiceCollectionExtensions
├── Nexus.User.Domain/          → references Abstractions only
├── Nexus.User.Infrastructure/  → *DbContext, EF configurations
└── Nexus.User.Contracts/       → DTOs
```

## EF Core + SQL Server

```csharp
// InfrastructureServiceCollectionExtensions.cs
services.AddSqlServerPersistence<UserDbContext>(connectionString);
```

```powershell
dotnet ef migrations add InitialCreate `
  --project services/User/src/Nexus.User.Infrastructure `
  --startup-project services/User/src/Nexus.User.Api
dotnet ef database update `
  --project services/User/src/Nexus.User.Infrastructure `
  --startup-project services/User/src/Nexus.User.Api
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
