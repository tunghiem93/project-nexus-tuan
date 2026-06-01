# Project Nexus — Backend Scaffold (.NET + SQL Server)

Microservices backend cho sàn E-Commerce & Auction (SRS v0.7).

**Stack:** C# / ASP.NET Core 9, EF Core 9, **SQL Server 2025/2026**, Redis 7, RabbitMQ 3.13

## Cấu trúc repo

```
project-nexus/
├── Nexus.slnx
├── docker-compose.yml              # SQL Server, Redis, RabbitMQ
├── infra/init-db/
│   ├── 01-create-databases.sql     # Tạo 6 database
│   └── init-databases.ps1          # Script apply toàn bộ schema
├── api-specs/
├── docs/architecture/
└── services/
    └── user-service/
        ├── db/schema.sql           # T-SQL schema (source of truth)
        └── src/Nexus.UserService/
```

## Quick start

### 1. Hạ tầng (Docker)

```powershell
cd project-nexus
docker compose up -d
```

- **SQL Server:** `localhost:1433` — user `sa` / password `Nexus_Dev_2026!`
- Redis: `6379`
- RabbitMQ UI: http://localhost:15672

> Nếu bạn cài **SQL Server 2026** trực tiếp trên Windows (không dùng Docker), chỉ cần tạo database và chạy script schema — connection string giữ nguyên format.

### 2. Khởi tạo database + schema

**Cách 1 — Script tự động (cần [sqlcmd](https://learn.microsoft.com/sql/tools/sqlcmd/sqlcmd-utility)):**

```powershell
cd project-nexus
.\infra\init-db\init-databases.ps1
```

**Cách 2 — SSMS / Azure Data Studio:**

1. Chạy `infra/init-db/01-create-databases.sql` trên database `master`
2. Chạy từng `services/*/db/schema.sql` trên database tương ứng

### 3. Chạy User Service

```powershell
cd services/user-service/src/Nexus.UserService
dotnet run
```

```powershell
curl http://localhost:8081/api/v1/health
```

## Databases (mỗi service một DB)

| Service | Database |
|---------|----------|
| User | `Nexus_User` |
| Catalog | `Nexus_Catalog` |
| Commerce | `Nexus_Commerce` |
| Auction | `Nexus_Auction` |
| Fulfillment | `Nexus_Fulfillment` |
| Notification | `Nexus_Notification` |

## Connection string mẫu (.NET)

```json
"ConnectionStrings": {
  "UserDb": "Server=localhost,1433;Database=Nexus_User;User Id=sa;Password=Nexus_Dev_2026!;TrustServerCertificate=True;Encrypt=True"
}
```

Dùng **User Secrets** cho password thật:

```powershell
dotnet user-secrets set "ConnectionStrings:UserDb" "Server=..."
```

## Packages (.NET + SQL Server)

| Mục đích | Package |
|----------|---------|
| SQL Server | `Microsoft.EntityFrameworkCore.SqlServer` |
| Auth JWT | `Microsoft.AspNetCore.Authentication.JwtBearer` |
| Message broker | `MassTransit.RabbitMQ` |

## Tài liệu

| Tài liệu | Path |
|----------|------|
| .NET guide | [docs/architecture/dotnet-guide.md](docs/architecture/dotnet-guide.md) |
| ERD | [docs/architecture/erd-overview.md](docs/architecture/erd-overview.md) |
| State machines | [docs/architecture/state-machines.md](docs/architecture/state-machines.md) |

## PostgreSQL → SQL Server mapping (đã convert)

| PostgreSQL | SQL Server |
|------------|------------|
| `UUID` | `UNIQUEIDENTIFIER` |
| `TIMESTAMPTZ` | `DATETIMEOFFSET` |
| `JSONB` | `NVARCHAR(MAX)` + JSON functions |
| `BOOLEAN` | `BIT` |
| `ENUM` | `NVARCHAR` + `CHECK` constraint |
| Partial index | Filtered index (`WHERE ...`) |
