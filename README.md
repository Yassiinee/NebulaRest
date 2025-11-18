# NebulaRest

A clean, production-grade ASP.NET Core Web API demonstrating **REST principles**, **API versioning**, **caching**, **OpenTelemetry**, **rate limiting**, **EF Core**, **Swagger**, and best-practice architecture.


---

## 🚀 Features

- RESTful resource-oriented design
- API versioning (`/api/v1`)
- CRUD for `Users`
- Pagination, response caching, and ETag support
- Centralized error handling (`ProblemDetails`)
- Output caching / rate limiting / health checks
- OpenTelemetry + OTLP exporter
- Serilog structured logging
- EF Core (SQL Server LocalDB)

---

## 📦 Getting Started
1) Restore/build
```
 dotnet build NebulaRest.sln
```
2) Run
```
 dotnet run --project NebulaRest
```
3) Explore
- Swagger UI: https://localhost:PORT/swagger
- Users: `/api/v1/users`, `/api/v1/users/{id}`

## Advanced Features Enabled
- Output caching (global policy + per-endpoint via [OutputCache])
- Rate limiting (fixed window, 100 req/min)
- Centralized error handling with ProblemDetails
- OpenTelemetry (ASP.NET Core + HTTP) with OTLP exporter (configure OTLP endpoint via environment variable)
- Serilog structured logging
- Health checks at `/health`

## EF Core Migrations
Local tool manifest is included.
```
 dotnet tool run dotnet-ef migrations add <Name> -p NebulaRest -s NebulaRest
 dotnet tool run dotnet-ef database update -p NebulaRest -s NebulaRest
```

## Configuration
Update `NebulaRest/appsettings.json` connection string `ConnectionStrings:Default`.
- Default uses `(localdb)\\MSSQLLocalDB`. If unavailable, ask to switch to SQLite/InMemory.

## Project Layout
- `NebulaRest.sln` – Solution
- `NebulaRest/Program.cs` – Host and middleware
- `NebulaRest/Controllers/UsersController.cs`
- `NebulaRest/Data/AppDbContext.cs`
- `NebulaRest/Entities/User.cs`
- `NebulaRest/Dtos/UserDtos.cs`

## Architecture

```
NebulaRest/
├── Controllers/
├── Services/
├── Data/
├── Entities/
├── Dtos/
├── Validators/
├── Middleware/
├── Extensions/
└── Migrations/
```
 Extension methods
└── Migrations/      # EF Core migrations
