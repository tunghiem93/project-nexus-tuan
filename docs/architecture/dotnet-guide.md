# .NET / ASP.NET Core — Project Nexus

Hướng dẫn triển khai backend theo SRS với C#.

## Solution layout

```
services/<name>-service/
├── db/
│   └── schema.sql              # DDL T-SQL (SQL Server) — giữ sync với EF migrations
└── src/
    └── Nexus.<Name>Service/
        ├── Controllers/        # Thin — chỉ HTTP, gọi Application layer
        ├── Domain/
        │   ├── Entities/
        │   └── Enums/
        ├── Data/
        │   ├── <Name>DbContext.cs
        │   └── Configurations/ # IEntityTypeConfiguration<T>
        ├── Services/           # IUserService, IAuthService, ...
        ├── Messaging/
        │   ├── Publishers/
        │   └── Consumers/
        ├── Dtos/
        ├── Program.cs
        └── appsettings.json
```

## EF Core + SQL Server

Package:

```powershell
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

```csharp
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(connectionString));
```

Connection string:

```
Server=localhost,1433;Database=Nexus_User;User Id=sa;Password=***;TrustServerCertificate=True;Encrypt=True
```

## EF Core + schema.sql

Workflow đề xuất:

1. Thiết kế bảng trong `db/schema.sql` (đã có sẵn cho 6 service)
2. Tạo entity C# map 1-1 với bảng
3. `dotnet ef migrations add InitialCreate`
4. So sánh migration với `schema.sql` — **schema.sql là source of truth cho review**

```powershell
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate --project services/user-service/src/Nexus.UserService
dotnet ef database update
```

## API versioning

Tất cả public endpoints: `/api/v1/...`  
OpenAPI spec: `api-specs/<service>/openapi.yaml`

## Authentication (User Service)

```csharp
// Packages
// Microsoft.AspNetCore.Authentication.JwtBearer

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* validate issuer, audience, signing key */ });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PRODUCT.CREATE", policy =>
        policy.RequireClaim("privilege", "PRODUCT.CREATE"));
});
```

JWT claims gợi ý: `sub` (userId), `roles[]`, `privileges[]`, `jti`.

## Transactional Outbox (events)

```csharp
public class OutboxEvent
{
    public Guid Id { get; set; }
    public string AggregateType { get; set; } = "";
    public Guid AggregateId { get; set; }
    public string EventType { get; set; } = "";
    public string Payload { get; set; } = "";  // JSON
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

Background worker (`IHostedService`) poll `outbox_events WHERE published_at IS NULL` → publish RabbitMQ → mark published.

## MassTransit + RabbitMQ (gợi ý)

```powershell
dotnet add package MassTransit.RabbitMQ
```

```csharp
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderPaidConsumer>();
    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("nexus");
            h.Password("nexus_dev");
        });
        cfg.ConfigureEndpoints(ctx);
    });
});
```

## Idempotency middleware

```csharp
// Header: Idempotency-Key
// Check commerce idempotency_keys / processed_events trước khi xử lý POST
```

## Health checks

- `/health` — ASP.NET health checks (DB, broker)
- `/api/v1/health` — business health (SRS SYSTEM.HEALTH.VIEW)

## Config từ SRS

Đọc từ `appsettings.json` section `Nexus:*`, override bằng environment:

```json
"Nexus": {
  "Auth": {
    "AccessTokenExpiryMinutes": 60,
    "MaxLoginAttempts": 5
  }
}
```

Sau này Admin có thể quản lý runtime qua `system_config` table + `IOptionsMonitor`.

## Thứ tự implement (giữ nguyên từ phân tích SRS)

1. User Service — IAM, JWT, RBAC
2. Catalog Service
3. Fulfillment — inventory + shipping quote
4. Commerce — cart, checkout, Stripe
5. Notification — event log + email
6. Auction — bidding + settlement
7. Reputation enforcement
