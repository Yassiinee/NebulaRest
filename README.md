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

