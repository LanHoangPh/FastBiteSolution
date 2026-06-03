# PROJECT_CONTEXT.md — FastBite Solution

## What This Project Does

**FastBiteGroup** is a backend REST API for a food-ordering / product e-commerce platform (name: **FastBite**). It provides:

- **Product management** — Create, Update, Delete, Query products.
- **Authentication** — Register, Login, Logout with JWT + Refresh Token Rotation.
- **Session management** — Token blacklisting on logout using Redis.
- **Multi-client support** — Aspire orchestration supports optional Frontend (web) and Desktop (WPF) clients.

The system is in early-to-mid development stage. Core infrastructure scaffolding is complete; business use cases are being built out.

---

## Target Users

- End users ordering food / browsing products (via a future frontend).
- Administrators managing product catalogue.
- Internal developers consuming the API.

---

## Main Modules

| Module | Project | Responsibility |
|---|---|---|
| **Domain** | `FastBiteGroup.Domain` | Entities, Domain Exceptions, Repository interfaces, Unit of Work interface |
| **Application** | `FastBiteGroup.Application` | Use Cases (CQRS handlers), Pipeline Behaviors, Validators, Mappers |
| **Contract** | `FastBiteGroup.Contract` | Shared DTOs, Commands, Queries, Responses, `Result<T>` pattern |
| **Persistence** | `FastBiteGroup.Persistence` | EF Core DbContext, Repository implementations, UnitOfWork |
| **Infrastructure** | `FastBiteGroup.Infrastructure` | Redis (cache), JWT utilities, Security services |
| **Presentation** | `FastBiteGroup.Presentation` | Minimal API endpoint definitions (`IEndpoint`) |
| **API** | `FastBiteGroup.API` | ASP.NET Core host, Middleware, DI composition root |
| **AppHost** | `FastBiteGroup.AppHost` | .NET Aspire orchestrator — wires PostgreSQL, Redis, API |
| **ServiceDefaults** | `FastBiteGroup.ServiceDefaults` | Shared Aspire defaults: OpenTelemetry, Health Checks, Service Discovery |

---

## Repository Structure

```
FastBiteSolution/
├── AGENTS.md                          ← AI agent quick reference
├── FastBiteSolution.slnx              ← Solution file
│
├── FastBiteGroup.API/                 ← Entry point (ASP.NET Core host)
│   ├── Program.cs
│   ├── Middleware/
│   │   ├── GlobalExceptionHandler.cs
│   │   └── TokenBlacklistMiddleware.cs
│   └── DependencyInjection/Extensions/
│       ├── JwtExtensions.cs
│       ├── EndpointExtensions.cs
│       └── SwaggerExtensions.cs
│
├── FastBiteGroup.Domain/              ← Pure domain (no external deps)
│   ├── Entities/
│   │   ├── Products.cs
│   │   └── RefreshToken.cs
│   ├── Abstractions/
│   │   ├── EntityBase<TKey>
│   │   ├── EntityAuditBase<TKey>    ← Soft-delete + audit fields
│   │   ├── IUnitOfWork
│   │   └── Repositories/
│   │       ├── IRepositoryBase<TEntity, TKey>
│   │       └── IRefreshTokenRepository
│   └── Exceptions/
│       ├── DomainException.cs
│       ├── BadRequestException.cs
│       ├── NotFoundException.cs
│       └── ProductException.cs
│
├── FastBiteGroup.Application/         ← Business logic (CQRS)
│   ├── Behaviors/                     ← MediatR pipeline behaviors
│   │   ├── PerformancePipelineBehavior.cs  (logs requests > 5s)
│   │   ├── TracingPipelineBehaviors.cs     (logs success/failure)
│   │   ├── TransactionPipelineBehaviors.cs (wraps in UoW transaction)
│   │   └── ValidationPipelineBehaviors.cs  (FluentValidation)
│   ├── Abstractions/Caching/
│   │   └── ICacheService.cs           ← Cache + JWT blacklist interface
│   ├── Constants/
│   │   └── CacheKeys.cs               ← Cache key factories
│   ├── Mappers/ServiceProfile.cs      ← AutoMapper profile (empty)
│   ├── Validators/Identity/           ← (empty, to be implemented)
│   └── UseCases/V1/                   ← (empty, to be implemented)
│
├── FastBiteGroup.Contract/            ← Shared message contracts
│   ├── Abstractions/Message/
│   │   ├── ICommand.cs / ICommandHandler.cs
│   │   ├── IQuery.cs / IQueryHandler.cs
│   │   └── IDomainEvent.cs / IDomainEventHandler.cs
│   ├── Abstractions/Shared/
│   │   ├── Result.cs / Result<T>.cs
│   │   ├── Error.cs (sealed record)
│   │   ├── ValidationResult.cs / ValidationResult<T>.cs
│   │   └── PagedResult.cs
│   └── Services/V1/
│       ├── Auth/Commands/AuthCommands.cs      ← Login, Register, Refresh, Logout, RevokeAll
│       ├── Auth/Queries/AuthQueries.cs
│       ├── Auth/Responses/AuthResponse.cs     ← AuthResponse, UserInfoResponse
│       └── Product/Commands/ProductCommands.cs ← Create, Update, Delete
│
├── FastBiteGroup.Persistence/         ← EF Core + Repositories
│   ├── ApplicationDbContext.cs        ← (EMPTY — no DbSets yet)
│   ├── EFUnitOfWork.cs               ← (STUB — NotImplementedException)
│   ├── Repositories/RepositoryBase.cs ← (STUB — NotImplementedException)
│   └── DependencyInjection/
│       └── Extensions/ServiceCollectionExtensions.cs
│
├── FastBiteGroup.Infrastructure/      ← External services
│   ├── Constants/TableNames.cs        ← DB table name constants
│   ├── DependencyInjection/Options/JwtOptions.cs
│   └── DependencyInjection/Extentions/
│       └── ServiceCollectionExtensions.cs ← (STUB — AddRedis/AddSecurity empty)
│
├── FastBiteGroup.Presentation/        ← Minimal API endpoints
│   ├── Abstractions/
│   │   ├── IEndpoint.cs
│   │   └── ApiEndpoint.cs             ← HandleFailure + ProblemDetails mapping
│   └── APIs/
│       ├── AuthApi.cs                 ← (STUB — versioned group, no routes yet)
│       └── ProductApi.cs              ← (STUB — versioned group, no routes yet)
│
├── FastBiteGroup.AppHost/             ← .NET Aspire orchestrator
│   ├── AppHost.cs
│   └── Extensions/
│       ├── ApiExtensions.cs           ← Wires API + DB + Redis + license keys
│       ├── DatabaseExtensions.cs      ← PostgreSQL "DefaultConnection"
│       ├── CacheExtensions.cs         ← Redis
│       ├── FrontendExtensions.cs      ← Optional web frontend (not built)
│       └── DesktopExtensions.cs       ← Optional WPF desktop (not built)
│
├── FastBiteGroup.ServiceDefaults/     ← Aspire shared defaults
│   └── Extensions.cs                  ← OTel, Health Checks, Service Discovery
│
└── Tests/
    ├── FastBiteGroup.Architecture.Tests/   ← 10 architecture rules (NetArchTest)
    ├── FastBiteGroup.Domain.Tests/         ← (empty)
    ├── FastBiteGroup.Application.Tests/    ← (empty)
    ├── FastBiteGroup.Contract.Tests/       ← (empty)
    └── FastBiteGroup.Integration.Tests/   ← Respawn + WebApplicationFactory (empty)
```

---

## Development Workflow

1. Define Commands/Queries/Responses in `FastBiteGroup.Contract/Services/V1/<Feature>/`.
2. Implement Handlers in `FastBiteGroup.Application/UseCases/V1/Commands|Queries/`.
3. Add FluentValidation validators in `FastBiteGroup.Application/Validators/`.
4. Register domain entities in `ApplicationDbContext` and add EF migration.
5. Implement Repository methods in `FastBiteGroup.Persistence/Repositories/`.
6. Map API routes in `FastBiteGroup.Presentation/APIs/`.
7. Run Architecture Tests to verify boundaries are not violated.
8. Write Integration Tests for happy path + edge cases.

---

## Local Setup Instructions

### Prerequisites

- .NET 10 SDK
- Docker Desktop (for Aspire-managed PostgreSQL + Redis)
- Visual Studio 2022+ or JetBrains Rider

### Run with Aspire (Recommended)

```bash
cd d:/CodeVs/FastBiteSolution
dotnet run --project FastBiteGroup.AppHost
```

Aspire Dashboard will open automatically. PostgreSQL and Redis containers are created automatically.

### Manual Setup (without Aspire)

1. Start PostgreSQL and Redis manually.
2. Add connection strings to `FastBiteGroup.API/appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=fastbite;Username=postgres;Password=yourpassword"
     },
     "JwtOptions": {
       "SecretKey": "your-secret-key-at-least-32-chars",
       "Issuer": "FastBiteGroup",
       "Audience": "FastBiteGroup",
       "ExpiryMinutes": 60
     }
   }
   ```
3. Run EF migrations: `dotnet ef database update --project FastBiteGroup.Persistence --startup-project FastBiteGroup.API`
4. Run: `dotnet run --project FastBiteGroup.API`

### License Keys

MediatR and AutoMapper **require commercial license keys** for production use.
- Dev keys are hardcoded in `FastBiteGroup.AppHost/appsettings.json` (DO NOT commit new keys).
- Production: inject via environment variables `LicenseKeyOptions__LicenseKeyMediatR` and `LicenseKeyOptions__LicenseKeyAutoMapper`.
