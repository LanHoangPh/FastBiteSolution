# AGENTS.md - FastBite Project Memory

> AI Agent Quick Reference. Read this first. For details, see `docs/ai-context/`.

---

## Project Summary

**FastBiteGroup** is a .NET 10 backend API currently shaped as a food-ordering / e-commerce platform, with a planned evolution toward chat-style workloads that combine relational and document storage.

- **Architecture**: Clean Architecture / Modular Monolith
- **Pattern**: CQRS via MediatR + Repository + Unit of Work
- **Runtime**: .NET 10, ASP.NET Core 10
- **Relational DB**: PostgreSQL via EF Core / Npgsql
- **Document DB**: MongoDB scaffold via `MongoDB.Driver` for future messages, notifications, read models, and outbox use cases
- **Cache**: Redis for token blacklisting and response caching
- **Auth**: JWT Bearer + Refresh Token Rotation + Token Blacklist
- **Orchestration**: .NET Aspire 13.x AppHost
- **Observability**: OpenTelemetry + Serilog
- **API Versioning**: `Asp.Versioning.Http` with v1 active

---

## Build Commands

```bash
dotnet restore FastBiteSolution.slnx
dotnet build FastBiteSolution.slnx
dotnet test FastBiteSolution.slnx

dotnet run --project src/backend/FastBiteGroup.API
dotnet run --project src/Aspire/FastBiteGroup.AppHost

dotnet test tests/backend/FastBiteGroup.Architecture.Tests/FastBiteGroup.Architecture.Tests.csproj

dotnet ef migrations add <MigrationName> --project src/backend/FastBiteGroup.Persistence --startup-project src/backend/FastBiteGroup.API
dotnet ef database update --project src/backend/FastBiteGroup.Persistence --startup-project src/backend/FastBiteGroup.API
```

---

## Development Rules

### Architecture Constraints

- **Domain** has zero dependencies on other solution layers.
- **Contract** is shared messaging and DTO surface. It must not depend on Application or Persistence.
- **Application** depends only on Domain + Contract. Never on Persistence or Infrastructure.
- **Persistence** depends on Domain + Contract. It contains EF Core, Identity persistence, MongoDB driver code, repositories, and database-specific options.
- **Infrastructure** depends on Application + Domain and may use Persistence types only for external service adapters that need Identity persistence.
- **Presentation** depends only on Contract and MediatR-facing abstractions. Never on Domain or Persistence directly.
- **API** is the composition root and wires all services together.

These rules are enforced by `ArchitectureTests.cs` using NetArchTest.

### Data Storage Rules

- PostgreSQL/EF Core is the source of truth for relational business data: users, auth, products, conversation membership, permissions, and other strongly relational data.
- MongoDB is prepared for future high-volume document data: chat messages, notifications, delivery logs, projections, and read models.
- Do not create a fake generic repository abstraction shared by EF Core and MongoDB. Use storage-specific repository interfaces for real use cases.
- Do not attempt cross-database distributed transactions between PostgreSQL and MongoDB. Use outbox/inbox, retries, idempotency keys, and eventual consistency.
- `IUnitOfWork` represents EF Core/PostgreSQL transaction boundaries only.
- MongoDB connection is optional; the API must still start when MongoDB is not configured.

### Coding & Performance Conventions

- Use `record` for Commands, Queries, and Responses.
- All handlers return `Result<T>` or `Result`; do not throw raw exceptions from Application handlers.
- Domain exceptions inherit from `DomainException`.
- **Performance Rules**: EF Core read-only queries MUST use `AsNoTracking()` and Projection (`Select`). Avoid N+1 Queries.
- Domain entities live in `FastBiteGroup.Domain.Entities`.
- MongoDB documents live under `FastBiteGroup.Persistence.Mongo.Documents`.
- Endpoint definitions live in `FastBiteGroup.Presentation.APIs` via `IEndpoint` / `ApiEndpoint`.
- Use Aspire service references or environment variables for connection strings. Never hardcode secrets.

### Testing Requirements

- Architecture: `tests/backend/FastBiteGroup.Architecture.Tests`
- Domain unit tests: `tests/backend/FastBiteGroup.Domain.Tests`
- Application tests: `tests/backend/FastBiteGroup.Application.Tests`
- Contract tests: `tests/backend/FastBiteGroup.Contract.Tests`
- Integration tests: `tests/backend/FastBiteGroup.Integration.Tests`

### Security Requirements

- Never expose production error details through `ProblemDetails`.
- JWT symmetric key must come from secrets/environment, not source-controlled appsettings.
- Logout flows must blacklist JWT `jti` in Redis via `ICacheService.BlacklistTokenAsync`.
- Refresh token rotation must mark the old token as used and issue a new token.

---

## AI Agent Rules (Strict)

### 1. Act as Senior Architect & BA/PM First
Do not just write code or jump straight into solutions. Always think from 4 perspectives:
- Business Analyst
- Product Manager
- Product Owner
- Solution Architect
Challenge requirements: Why is it needed? Is there a simpler way? Does it align with the Product Vision?

### 2. Before Making Any Changes
Always read the full context in the following order:
1. `docs/ai-context/PRODUCT_REQUIREMENTS.md`
2. `docs/ai-context/PROJECT_CONTEXT.md`
3. `docs/ai-context/ARCHITECTURE.md`
4. `docs/ai-context/CURRENT_STATUS.md`

### 3. Never
- Change architecture layer dependencies without explicit request.
- Change NuGet package versions without explicit request.
- Modify `ApplicationDbContext` or add EF `DbSet<>` without a migration plan.
- Remove or skip architecture tests.
- Hardcode secrets, connection strings, or license keys.
- Propose Microservices; this project is intentionally a Modular Monolith.
- Provide code without explaining Trade-offs, Pros/Cons, and "Why".

### 4. Always
- Answer in structured format: 1. Problem Analysis -> 2. Proposed Architecture -> 3. Implementation -> 4. Code -> 5. Best Practices.
- Register new services in the correct layer's `DependencyInjection/Extensions/ServiceCollectionExtensions.cs`.
- Keep `AGENTS.md` concise. Put detailed status updates in `CURRENT_STATUS.md`.
