# Project Nexus — Backend (.NET + SQL Server)

Microservices backend cho sàn E-Commerce & Auction (SRS v0.7).

**Stack:** ASP.NET Core 9, EF Core 9, SQL Server, Redis, RabbitMQ

## Cấu trúc

```
├── ARCHITECTURE.md
├── Directory.Build.props / Directory.Packages.props
├── Nexus.slnx
├── src/
│   ├── Nexus.Abstractions/
│   ├── Nexus.Persistence/
│   └── Nexus.AspNetCore/
└── services/{User|Catalog|Commerce|Auction|Fulfillment|Notification}/
    ├── db/schema.sql
    └── src/Nexus.{Name}.{Api|Application|Domain|Infrastructure|Contracts}/
```

Chi tiết layer và dependency rules: [ARCHITECTURE.md](ARCHITECTURE.md)

## Quick start

### 1. Hạ tầng

```powershell
docker compose up -d
```

### 2. Database

```powershell
.\infra\init-db\init-databases.ps1
```

### 3. Build & run

```powershell
dotnet build Nexus.slnx
cd services\User\src\Nexus.User.Api
dotnet run
curl http://localhost:8081/api/v1/health
# Swagger UI (Development): http://localhost:8081/swagger
```

| Service | API | Port |
|---------|-----|------|
| User | `services/User/src/Nexus.User.Api` | 8081 |
| Catalog | `services/Catalog/src/Nexus.Catalog.Api` | 8082 |
| Commerce | `services/Commerce/src/Nexus.Commerce.Api` | 8083 |
| Auction | `services/Auction/src/Nexus.Auction.Api` | 8084 |
| Fulfillment | `services/Fulfillment/src/Nexus.Fulfillment.Api` | 8085 |
| Notification | `services/Notification/src/Nexus.Notification.Api` | 8086 |

## Tài liệu

| | Path |
|---|------|
| Architecture | [ARCHITECTURE.md](ARCHITECTURE.md) |
| .NET guide | [docs/architecture/dotnet-guide.md](docs/architecture/dotnet-guide.md) |
| ERD | [docs/architecture/erd-overview.md](docs/architecture/erd-overview.md) |
