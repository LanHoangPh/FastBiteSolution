# TECH_STACK.md — FastBite Solution

## Runtime

| Component | Version | Why |
|---|---|---|
| .NET | 10.0 | Latest LTS-track; best performance, latest C# features |
| ASP.NET Core | 10.0 | Web host + Minimal API support |
| C# | 13+ | Records, pattern matching, primary constructors |

---

## Core Frameworks

### MediatR (`MediatR`)
- **Version**: Inferred from license key format (commercial/LuckyPenny edition)
- **Why**: Decouples request dispatch from handlers; enables pipeline behaviors (validation, transactions, logging) without touching handler code.
- **Trade-off**: Requires commercial license for production use (>250ms overhead discussions apply to very high-throughput scenarios — not a concern at this scale).
- **Where**: Registered in `Application.DependencyInjection.Extensions.ServiceCollectionExtensions`.

### AutoMapper (`AutoMapper v16.1.1`)
- **Why**: Maps between Domain entities and Contract DTOs without manual boilerplate.
- **Trade-off**: Requires commercial license. The `ServiceProfile` is currently empty — mapping is not yet implemented.
- **Status**: ⚠️ Configured but not used (empty `ServiceProfile`).

### FluentValidation (`FluentValidation.DependencyInjectionExtensions v12.1.1`)
- **Why**: Declarative, testable validators registered auto-scan from Application assembly.
- **Pipeline integration**: `ValidationPipelineBehaviors` runs all validators, collects `Error[]`, and returns `ValidationResult` — no exceptions thrown.
- **Status**: ⚠️ No validators implemented yet.

---

## Database

### PostgreSQL (primary)
- **NuGet**: `Npgsql.EntityFrameworkCore.PostgreSQL v10.0.1`
- **Why**: Open source, strong JSON support, better performance under high concurrency than SQL Server for typical web workloads.
- **Aspire**: Provisioned via `Aspire.Hosting.PostgreSQL v13.4.0`.
- **Connection name**: `"DefaultConnection"` (injected by Aspire).

### SQL Server (configured, secondary)
- **NuGet**: `Microsoft.EntityFrameworkCore.SqlServer v10.0.7`
- **Why**: Configured as an alternate option with retry strategy (`SqlServerRetryingExecutionStrategy`).
- **Status**: Both providers are referenced in `Persistence.csproj`. The active provider is selected in `ServiceCollectionExtensions.AddSqlServerPersistence()` — currently using SQL Server config with PostgreSQL NuGet. **This is inconsistent and should be resolved.**

### Entity Framework Core (`EF Core v10.0.7`)
- **NuGet**: `Microsoft.EntityFrameworkCore.Tools`, `Microsoft.EntityFrameworkCore.SqlServer`, `Npgsql.EFCore`
- **Pattern**: `DbContextPool` with retry strategy.
- **Lazy Loading**: `UseLazyLoadingProxies(true)` configured — navigation properties must be `virtual`.
- **Status**: ⚠️ `ApplicationDbContext` has no `DbSet<>` properties yet. No migrations exist.

---

## Caching

### Redis
- **NuGet**: Referenced in `Infrastructure` (empty implementation).
- **Aspire**: `Aspire.Hosting.Redis v13.4.0`.
- **`ICacheService` interface**: Defined in `Application.Abstractions.Caching`.
  - `SetAsync<T>` — store with TTL
  - `GetAsync<T>` — retrieve
  - `RemoveAsync` — evict
  - `BlacklistTokenAsync(jti, remainingLifetime)` — JWT blacklisting
  - `IsTokenBlacklistedAsync(jti)` — check blacklist
- **Status**: ⚠️ `ICacheService` is defined but **not implemented** in Infrastructure.

---

## Authentication & Security

### JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer v10.0.7`)
- **Algorithm**: HMAC-SHA (symmetric key)
- **Config**: `JwtOptions` (Issuer, Audience, SecretKey, ExpiryMinutes)
- **`ClockSkew`**: `TimeSpan.Zero` — tokens expire exactly at `exp` claim.
- **Events**: Custom `OnAuthenticationFailed` (adds `Token-Expired: true` header) and `OnChallenge` (returns `application/problem+json`).

### Token Blacklisting (Redis)
- On logout, the `jti` claim is stored in Redis with TTL = remaining token lifetime.
- `TokenBlacklistMiddleware` checks the blacklist on every authenticated request.

### Refresh Token (Database)
- `RefreshToken` entity stores: `Token`, `Jti`, `UserId`, `ExpiresAt`, `IsRevoked`, `IsUsed`, `ReplacedByToken`.
- Rotation pattern: `MarkUsed(newToken)` → issue new `RefreshToken`.
- Bulk revocation: `IRefreshTokenRepository.RevokeAllForUserAsync(userId)`.

---

## Observability

### OpenTelemetry (via ServiceDefaults)
- **NuGet**: `OpenTelemetry.Extensions.Hosting v1.15.3`, `OpenTelemetry.Instrumentation.AspNetCore v1.15.2`, `OpenTelemetry.Instrumentation.Http v1.15.1`, `OpenTelemetry.Instrumentation.Runtime v1.15.1`, `OpenTelemetry.Exporter.OpenTelemetryProtocol v1.15.3`
- **Metrics**: ASP.NET Core instrumentation + HTTP client + Runtime metrics.
- **Traces**: ASP.NET Core + HTTP client. Health check paths excluded from tracing.
- **Export**: OTLP exporter activated when `OTEL_EXPORTER_OTLP_ENDPOINT` env var is set.
- **Aspire Dashboard**: Receives OTel data automatically in development.

### Serilog (`Serilog.AspNetCore v10.0.0`)
- **Status**: Package referenced in API project but **not configured** in `Program.cs`.

### Health Checks
- `/health` endpoint — all health checks must pass.
- `/alive` endpoint — only `"live"` tagged checks.
- ⚠️ Only exposed in Development environment (security note in code).

---

## API Design

### API Versioning (`Asp.Versioning.Http v10.0.0`)
- URL-based versioning: `/api/v{version:apiVersion}/...`
- V1: active (Auth, Products endpoints).
- V2: scaffold exists (`UseCases/V2/` folder), no implementations.

### OpenAPI (`Microsoft.AspNetCore.OpenApi v10.0.8`)
- Minimal API style (built-in .NET 10 OpenAPI, not Swashbuckle-generated).
- `Swashbuckle.AspNetCore v10.2.1` also referenced (both configured — potential redundancy).
- OpenAPI endpoint: `/openapi/v1.json` (development only).

---

## Aspire & Infrastructure

### .NET Aspire (`Aspire.AppHost.Sdk v13.3.5`)
- Orchestrates all services in development.
- Service discovery: `Microsoft.Extensions.ServiceDiscovery v10.5.0`.
- Resilience: `Microsoft.Extensions.Http.Resilience v10.5.0` (standard resilience pipeline on all HTTP clients).

---

## Testing

| Framework | Version | Purpose |
|---|---|---|
| xUnit | 2.9.3 | Test runner |
| FluentAssertions | 8.10.0 | Assertion library |
| NetArchTest.Rules | 1.3.2 | Architecture rule enforcement |
| Respawn | 7.0.0 | Database reset between integration tests |
| Microsoft.AspNetCore.Mvc.Testing | 10.0.7 | In-process WebApplicationFactory for integration tests |
| coverlet.collector | 6.0.4 | Code coverage |

---

## Package Version Summary

| NuGet Package | Version | Project |
|---|---|---|
| `Asp.Versioning.Http` | 10.0.0 | API |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.7 | API |
| `Microsoft.AspNetCore.OpenApi` | 10.0.8 | API |
| `Microsoft.EntityFrameworkCore.Tools` | 10.0.7 | API |
| `Serilog.AspNetCore` | 10.0.0 | API |
| `Swashbuckle.AspNetCore` | 10.2.1 | API |
| `AutoMapper` | 16.1.1 | Application |
| `FluentValidation.DependencyInjectionExtensions` | 12.1.1 | Application |
| `MediatR` | (commercial) | Application |
| `Microsoft.Extensions.Configuration.Abstractions` | 10.0.7 | Infrastructure |
| `Microsoft.AspNetCore.Http` | 2.3.10 | Persistence |
| `Microsoft.EntityFrameworkCore.SqlServer` | 10.0.7 | Persistence |
| `Microsoft.Extensions.Hosting` | 10.0.7 | Persistence |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 10.0.1 | Persistence |
| `Aspire.Hosting.PostgreSQL` | 13.4.0 | AppHost |
| `Aspire.Hosting.Redis` | 13.4.0 | AppHost |
| `OpenTelemetry.*` | 1.15.x | ServiceDefaults |
| `Microsoft.Extensions.ServiceDiscovery` | 10.5.0 | ServiceDefaults |
| `Microsoft.Extensions.Http.Resilience` | 10.5.0 | ServiceDefaults |
