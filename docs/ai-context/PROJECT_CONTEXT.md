# PROJECT_CONTEXT.md - FastBite Solution

## What This Project Does

**FastBiteGroup** is a .NET 10 backend REST API for a Unified Communication Platform (hybrid Microsoft Teams/Slack + Facebook Workplace/Yammer). The storage architecture combines SQL for strict relational state and NoSQL for high-volume unstructured social/chat workloads.

Implemented capabilities:
- User and Workspace management basics (replacing legacy product concepts).
- Authentication: register, login, refresh token, logout, revoke all sessions.
- Session protection: JWT blacklist in Redis.
- PostgreSQL persistence with EF Core and ASP.NET Core Identity.
- Optional MongoDB persistence scaffold for future messages, notifications, projections, and outbox workflows.
- Aspire orchestration for PostgreSQL, Redis, API, and migration service.

The system is still a Modular Monolith, not microservices.

---

## Target Users

- Enterprise employees or community members collaborating via channels and social feeds.
- Workspace Administrators managing roles and moderating content.
- System Administrators monitoring logs and health.
- Internal developers consuming the API.

---

## Main Modules

| Module | Project | Responsibility |
|---|---|---|
| Domain | `src/backend/FastBiteGroup.Domain` | Pure entities, domain exceptions, repository interfaces, EF unit-of-work abstraction |
| Contract | `src/backend/FastBiteGroup.Contract` | Commands, queries, responses, Result pattern, shared outbox contracts |
| Application | `src/backend/FastBiteGroup.Application` | CQRS handlers, validators, pipeline behaviors, mappings |
| Persistence | `src/backend/FastBiteGroup.Persistence` | EF Core DbContext, Identity persistence, EF repositories, MongoDB driver scaffold, Mongo outbox store |
| Infrastructure | `src/backend/FastBiteGroup.Infrastructure` | Redis cache, JWT token service, Identity auth adapter |
| Presentation | `src/backend/FastBiteGroup.Presentation` | Minimal API endpoint definitions |
| API | `src/backend/FastBiteGroup.API` | ASP.NET Core host, middleware, DI composition root |
| MigrationService | `src/backend/FastBiteGroup.MigrationService` | Runs EF migrations and seed data during Aspire startup |
| AppHost | `src/Aspire/FastBiteGroup.AppHost` | Aspire orchestration |
| ServiceDefaults | `src/Aspire/FastBiteGroup.ServiceDefaults` | OpenTelemetry, health checks, service discovery |

---

## Repository Structure

```text
FastBiteSolution/
  AGENTS.md
  FastBiteSolution.slnx
  docs/ai-context/

  src/backend/
    FastBiteGroup.API/
    FastBiteGroup.Application/
    FastBiteGroup.Contract/
      Abstractions/
        Message/
        Outbox/
        Shared/
      Services/V1/
    FastBiteGroup.Domain/
      Abstractions/
      Entities/
      Exceptions/
    FastBiteGroup.Infrastructure/
      Services/
      DependencyInjection/
    FastBiteGroup.Persistence/
      ApplicationDbContext.cs
      EFUnitOfWork.cs
      Configurations/
      Identity/
      Mongo/
        Documents/
        Outbox/
      Repositories/
    FastBiteGroup.Presentation/
      APIs/
      Abstractions/
    FastBiteGroup.MigrationService/

  src/Aspire/
    FastBiteGroup.AppHost/
    FastBiteGroup.ServiceDefaults/

  tests/backend/
    FastBiteGroup.Architecture.Tests/
    FastBiteGroup.Application.Tests/
    FastBiteGroup.Contract.Tests/
    FastBiteGroup.Domain.Tests/
    FastBiteGroup.Integration.Tests/
```

---

## Development Workflow

1. Define Commands, Queries, Responses in `FastBiteGroup.Contract/Services/V1/<Feature>/`.
2. Implement handlers in `FastBiteGroup.Application/UseCases/V1/Commands|Queries/`.
3. Add validators in `FastBiteGroup.Application/Validators/`.
4. For relational entities, update `ApplicationDbContext`, configurations, and add EF migration.
5. For MongoDB documents, add document/repository code under `FastBiteGroup.Persistence/Mongo/`.
6. Register services in the owning layer's DI extension.
7. Map routes in `FastBiteGroup.Presentation/APIs/`.
8. Run architecture tests and relevant unit/integration tests.

---

## Storage Direction

PostgreSQL is used for strongly relational data and transactional business state. MongoDB is optional and reserved for document-oriented workloads such as chat messages, notifications, delivery logs, and projections.

Cross-database consistency should be handled by outbox/inbox, retry, idempotency, and eventual consistency. Do not design features that require atomic distributed transactions across PostgreSQL and MongoDB.

---

## Local Setup

Run with Aspire:

```bash
dotnet run --project src/Aspire/FastBiteGroup.AppHost
```

Run API standalone:

```bash
dotnet run --project src/backend/FastBiteGroup.API
```

MongoDB is optional. To enable it manually, configure:

```json
{
  "ConnectionStrings": {
    "mongodb": "mongodb://localhost:27017"
  },
  "MongoDbOptions": {
    "DatabaseName": "fastbite",
    "OutboxCollectionName": "integration_outbox"
  }
}
```

Secrets such as `JwtOptions:SecretKey` and commercial license keys must come from user secrets or environment variables.
