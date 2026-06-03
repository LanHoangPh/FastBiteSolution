# AGENTS.md — FastBite Project Memory

> **AI Agent Quick Reference** — Read this first. For details, see `/docs/ai-context/`.

---

## Project Summary

**FastBiteGroup** is a .NET 10 backend API for a food-ordering / e-commerce platform.

- **Architecture**: Clean Architecture (Layered Monolith)
- **Pattern**: CQRS via MediatR + Repository + Unit of Work
- **Runtime**: .NET 10, ASP.NET Core 10
- **Database**: PostgreSQL (primary) + SQL Server (configured, switchable)
- **Cache**: Redis (for token blacklisting and response caching)
- **Auth**: JWT Bearer + Refresh Token Rotation + Token Blacklist
- **Orchestration**: .NET Aspire 13.x (AppHost)
- **Observability**: OpenTelemetry (traces, metrics, logs via OTLP)
- **API Versioning**: `Asp.Versioning.Http` (v1 active, v2 scaffold)

---

## Build Commands

```bash
# Restore
dotnet restore FastBiteSolution.slnx

# Build
dotnet build FastBiteSolution.slnx

# Run API (standalone)
dotnet run --project FastBiteGroup.API

# Run via Aspire (recommended for dev — starts PostgreSQL + Redis automatically)
dotnet run --project FastBiteGroup.AppHost

# Run all tests
dotnet test FastBiteSolution.slnx

# Run only architecture tests
dotnet test FastBiteGroup.Architecture.Tests

# Run EF migrations (from solution root)
dotnet ef migrations add <MigrationName> --project FastBiteGroup.Persistence --startup-project FastBiteGroup.API
dotnet ef database update --project FastBiteGroup.Persistence --startup-project FastBiteGroup.API
```

---

## Development Rules

### Architecture Constraints

- **Domain** has ZERO dependencies on other layers. It is pure C#.
- **Application** depends only on Domain + Contract. Never on Persistence or Infrastructure.
- **Contract** is the shared message layer. It depends on nothing in this solution.
- **Persistence** depends on Domain + Contract.
- **Infrastructure** depends on Application + Domain.
- **Presentation** depends only on Contract. Never on Domain or Persistence directly.
- **API** is the composition root — wires everything together.

> ⚠️ These rules are **enforced by `ArchitectureTests.cs`** using NetArchTest.

### Coding Conventions

- Use `record` for Commands, Queries, and Responses (immutable DTOs).
- All handlers return `Result<T>` or `Result` — never throw raw exceptions from Application layer.
- Domain exceptions inherit from `DomainException`.
- EF Core: use `AsNoTracking()` for all read-only queries. Avoid N+1. Use projections.
- All entities live in `FastBiteGroup.Domain.Entities`.
- API endpoints are defined in `FastBiteGroup.Presentation.APIs` via `IEndpoint` / `ApiEndpoint`.
- Use `.NET Aspire` service references for connection strings — never hardcode.

### Testing Requirements

- Architecture rules → `FastBiteGroup.Architecture.Tests` (xUnit + NetArchTest)
- Integration tests → `FastBiteGroup.Integration.Tests` (xUnit + Respawn + WebApplicationFactory)
- Domain unit tests → `FastBiteGroup.Domain.Tests`
- Application tests → `FastBiteGroup.Application.Tests`
- Contract tests → `FastBiteGroup.Contract.Tests`

### Security Requirements

- Never expose error details in production `ProblemDetails` responses.
- JWT uses symmetric key — store `JwtOptions:SecretKey` in secrets/environment, never in `appsettings.json`.
- All logout flows must blacklist the `jti` in Redis via `ICacheService.BlacklistTokenAsync`.
- Refresh token rotation: mark old token as Used + issue new token.

---

## AI Agent Rules

### Before Making Any Changes

1. Read `docs/ai-context/PROJECT_CONTEXT.md`
2. Read `docs/ai-context/ARCHITECTURE.md`
3. Read `docs/ai-context/CURRENT_STATUS.md`

### Never

- Change architecture layer dependencies without explicit request.
- Change NuGet package versions without explicit request.
- Modify `ApplicationDbContext` or add `DbSet<>` without a corresponding EF migration plan.
- Remove or skip architecture tests.
- Hardcode secrets, connection strings, or license keys in source files.
- Add Microservice boundaries — this is intentionally a Modular Monolith.

### Always

- Implement command/query handlers returning `Result<T>`.
- Register new services in the correct layer's `DependencyInjection/Extensions/ServiceCollectionExtensions.cs`.
- Keep `AGENTS.md` under 300 lines. Update `CURRENT_STATUS.md` instead of this file for status changes.
