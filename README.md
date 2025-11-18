# NebulaRest

A clean, RESTful ASP.NET Core Web API demonstrating resource-oriented design, API versioning, caching, Swagger, and EF Core (SQL Server).

## Features
- API versioning: routes under `/api/v1` (Microsoft.AspNetCore.Mvc.Versioning)
- Users resource CRUD: `GET/POST/PUT/DELETE /api/v1/users`
- Pagination on list, ETag support on GET by id, response caching on list
- EF Core + SQL Server LocalDB (dev) with auto database creation
- Swagger/OpenAPI UI

## Getting Started
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
NebulaRest/
├── Controllers/      # API endpoints
├── Services/         # Business logic
├── Data/            # EF Core DbContext
├── Entities/        # Database models
├── Dtos/            # Data transfer objects
├── Validators/      # FluentValidation validators
├── Middleware/      # Custom middleware
├── Extensions/      # Extension methods
└── Migrations/      # EF Core migrations
