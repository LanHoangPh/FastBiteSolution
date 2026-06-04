# TECH_STACK.md - FastBite Solution

## Runtime

| Component | Version |
|---|---|
| .NET | 10.0 |
| ASP.NET Core | 10.0 |
| C# | Latest supported by .NET 10 SDK |

---

## Core Frameworks

| Framework | Version | Project | Purpose |
|---|---|---|---|
| MediatR | 14.0.0 | Contract/Application | CQRS dispatch and pipeline behaviors |
| AutoMapper | 16.1.1 | Application | Domain-to-response mapping |
| FluentValidation | 12.1.1 | Contract/Application | Request validation |
| Asp.Versioning.Http | 10.0.0 | API | URL-based API versioning |
| Swashbuckle.AspNetCore | 10.2.1 | API | Swagger/OpenAPI UI |
| Microsoft.OpenApi | 2.7.5 | API | OpenAPI model support |
| Serilog.AspNetCore | 10.0.0 | API | Structured request/application logging |

MediatR and AutoMapper require license configuration for production use.

---

## Persistence

| Package | Version | Project | Purpose |
|---|---|---|---|
| Microsoft.EntityFrameworkCore | 10.0.5 | Persistence | EF Core base |
| Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.0 | Persistence | PostgreSQL provider |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 10.0.5 | Persistence | Identity tables with EF Core |
| MongoDB.Driver | 3.7.1 | Persistence | Optional MongoDB document persistence |
| StackExchange.Redis | 2.8.41 | Infrastructure | Redis cache and JWT blacklist |

PostgreSQL is active. MongoDB is scaffolded and optional. SQL Server is not active in the current Persistence project.

---

## Aspire

| Package | Version | Project |
|---|---|---|
| Aspire.AppHost.Sdk | 13.3.5 | AppHost |
| Aspire.Hosting.PostgreSQL | 13.4.0 | AppHost |
| Aspire.Hosting.Redis | 13.4.0 | AppHost |

The current AppHost wires PostgreSQL, Redis connection, MigrationService, API, and secret parameters. MongoDB is not yet provisioned by AppHost; standalone Mongo configuration can be supplied through appsettings/user-secrets/environment variables.

---

## Observability

ServiceDefaults configures:
- OpenTelemetry traces, metrics, and health checks.
- Service discovery and HTTP resilience defaults.

API configures:
- Serilog from configuration.
- Swagger/OpenAPI in Development or Staging.

---

## Authentication and Security

- JWT Bearer with symmetric HMAC key.
- `JwtOptions:SecretKey` must come from secrets/environment.
- Redis stores blacklisted JWT `jti` values.
- Refresh tokens are stored in PostgreSQL and rotated on refresh.

---

## Testing

| Tool | Purpose |
|---|---|
| xUnit | Unit/integration tests |
| FluentAssertions | Assertions |
| NetArchTest.Rules | Architecture rules |
| Respawn | Integration database reset |
| Microsoft.AspNetCore.Mvc.Testing | WebApplicationFactory integration tests |
| coverlet.collector | Coverage |

Common commands:

```bash
dotnet build FastBiteSolution.slnx
dotnet test FastBiteSolution.slnx
dotnet test tests/backend/FastBiteGroup.Architecture.Tests/FastBiteGroup.Architecture.Tests.csproj
```
