# Project Nexus — Backend (.NET + SQL Server)

Backend cho dịch vụ User trong repo này.

**Stack:** ASP.NET Core 9, EF Core 9, SQL Server, Redis, RabbitMQ

## Cấu trúc

```
├── ARCHITECTURE.md
├── Directory.Build.props / Directory.Packages.props
├── Nexus.slnx
├── src/
│   ├── Api/
│   │   ├── Hosting/
│   │   ├── Controllers/
│   │   ├── Grpc/
│   │   └── Services/
│   ├── Application/
│   ├── Domain/
│   ├── Infrastructure/
│   └── Shared/
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
dotnet run --project src/Api/Nexus.User.Api.csproj
curl http://localhost:8081/api/v1/health
# Swagger UI (Development): http://localhost:8081/swagger
```

| Service | API | Port |
|---------|-----|------|
| User | `src/Api/Nexus.User.Api.csproj` | 8081 |

## Tài liệu

| | Path |
|---|------|
| Architecture | [ARCHITECTURE.md](ARCHITECTURE.md) |
| .NET guide | [docs/architecture/dotnet-guide.md](docs/architecture/dotnet-guide.md) |
| ERD | [docs/architecture/erd-overview.md](docs/architecture/erd-overview.md) |
