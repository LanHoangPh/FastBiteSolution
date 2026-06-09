# CURRENT_STATUS.md - FastBiteGroup

**Last Updated:** 2026-06-09

---

## Overall Status

Build passing. Architecture tests passing 10/10.

The project has a working PostgreSQL/EF Core foundation and an optional MongoDB.Driver scaffold for future document-oriented workloads.

Workspace/Tenant MVP is implemented for onboarding, sidebar workspace switching, email invitations, shared invite codes, member listing, workspace update, and archive.

---

## Completed Work

### Domain

- `Products` entity with factory/update methods and product domain exceptions.
- `AppRefreshToken` entity with refresh-token rotation/revocation behavior.
- Entity base abstractions and audit/soft-delete interfaces.
- Repository interfaces: `IRepositoryBase<TEntity, TKey>`, `IRefreshTokenRepository`.
- Workspace tenant entities and rules: `Workspace`, `WorkspaceMember`, `UserWorkspaceInvitation`, `WorkspaceInvitation`, workspace role enum, and member status enum.
- `IUnitOfWork` abstraction for EF Core transaction boundaries.
- Domain exceptions inherit `DomainException`.

### Contract

- Command/query abstractions for MediatR.
- `Result`, `Result<T>`, validation result, `Error`, paged result.
- Auth contracts: login, register, verify email, refresh, logout, revoke all, forgot password, reset password, google login.
- Workspace contracts: create, update, archive, get my workspaces, get detail, list members, invite by email, get my invitations, accept invitation, create invite link, join by code.
- Product contracts: create, update, delete, get all, get by id, product response.
- Outbox contracts: `IntegrationOutboxMessage`, `IIntegrationOutboxStore`.

### Persistence

- `ApplicationDbContext` with Identity persistence.
- `AppUser`, `AppRole`.
- EF configurations for products and refresh tokens.
- Initial EF migration `initDB` under `src/backend/FastBiteGroup.Persistence/Migrations`.
- PostgreSQL-compatible EF mappings for filtered indexes, Identity tables, Workspace PK, soft-delete query filters, and app-generated video call session IDs.
- Workspace tenant migration `WorkspaceTenantMvp` adds member status and email-based workspace invitations.
- `EFUnitOfWork` with transaction support.
- `RepositoryBase<TEntity, TKey>` with `AsNoTracking()` for reads.
- `RefreshTokenRepository` with bulk revoke.
- PostgreSQL DI through `AddPostgreSqlPersistence`.
- Identity/repository/interceptor DI.
- Optional MongoDB DI through `AddMongoPersistence`.
- MongoDB scaffold: `MongoDbContext`, document base, Mongo outbox document, outbox store, outbox index initializer.

### Infrastructure

- Redis cache service using `StackExchange.Redis`.
- OTP service with limits (MaxAttempts, RateLimiting) using `StackExchange.Redis`.
- JWT token service with JTI support.
- User auth service wrapping ASP.NET Core Identity.
- Google Auth service wrapping `Google.Apis.Auth` for verifying ID Tokens.
- Email sender using `SendGrid`.
- `IntegrationOutboxProcessorService` running as a background service to process outbox events (e.g., sending emails).
- DI for Redis, email, google auth, and security services.

### Application

- Auth handlers: login, register, verify email, forgot password, reset password, google login, refresh token, logout, revoke all sessions.
- Workspace handlers: create, update, archive, get my workspaces, get detail, list members, invite by email, get my invitations, accept invitation, create invite link, join by code.
- Product handlers: create, update, delete, get all, get by id.
- Validators for auth/product commands.
- AutoMapper profile for product response mapping.
- MediatR pipeline behaviors: performance, tracing, transaction, validation.

### Presentation/API

- Auth API: register, verify email, login, refresh, logout, revoke all, forgot password, reset password, google login.
- Workspace API: onboarding/sidebar, detail, members, email invitations, invite links, join, update, archive.
- Product API: get all, get by id, create, update, delete.
- API composition in `Program.cs`.
- JWT auth, token blacklist middleware, global exception handler.
- Swagger/OpenAPI and API versioning.
- Serilog request logging.

---

## Test Status

| Test Area | Status |
|---|---|
| Solution build | Passing |
| Architecture tests | 10/10 passing |
| Contract tests | Passing, includes Workspace validators |
| Domain tests | Build/pass available |
| Application tests | Passing, includes Auth flows and Workspace handlers |
| Infrastructure tests | Build/pass available (OTP Service Mocking tests) |
| Integration tests | Passing with Testcontainers PostgreSQL/Redis in dedicated workflow |

Last verified commands:

```bash
dotnet format FastBiteSolution.slnx --verify-no-changes --no-restore
dotnet build FastBiteSolution.slnx
dotnet test tests/backend/FastBiteGroup.Contract.Tests/FastBiteGroup.Contract.Tests.csproj
dotnet test tests/backend/FastBiteGroup.Application.Tests/FastBiteGroup.Application.Tests.csproj
dotnet test tests/backend/FastBiteGroup.Architecture.Tests/FastBiteGroup.Architecture.Tests.csproj
dotnet test tests/backend/FastBiteGroup.Infrastructure.Tests/FastBiteGroup.Infrastructure.Tests.csproj
dotnet test tests/backend/FastBiteGroup.Integration.Tests/FastBiteGroup.Integration.Tests.csproj
```

CI notes:
- Main CI workflow is the required PR gate for formatting, solution build, architecture tests, application tests, contract tests, and infrastructure tests.
- Integration tests are split into a dedicated `workflow_dispatch` workflow and use Testcontainers for PostgreSQL and Redis.

Known warnings:
- NuGet vulnerability warnings for transitive packages `SharpCompress` and `Snappier`.

Migration notes:
- The initial migration was regenerated on 2026-06-07 after normalizing Domain entity and EF configuration mismatches.
- Workspace tenant migration `20260607143431_WorkspaceTenantMvp` was added on 2026-06-07.
- Local/dev PostgreSQL data should be reset before applying the regenerated `initDB`; reusing an older dev database can produce duplicate-table errors such as `relation "RefreshTokens" already exists`.
- `PendingModelChangesWarning` is intentionally not suppressed and should continue to fail fast when EF model and migration snapshot drift.

---

## Current MongoDB Status

MongoDB is infrastructure-only for now. It is prepared for future features but has no chat/message/notification domain implemented yet.

Enabled when any supported Mongo connection string exists:
- `ConnectionStrings:mongodb`
- `ConnectionStrings:MongoDb`
- `MongoDbOptions:ConnectionString`

Default options:

```json
{
  "MongoDbOptions": {
    "DatabaseName": "fastbite",
    "OutboxCollectionName": "integration_outbox"
  }
}
```

Future work:
- Add message/notification documents.
- Add feature-specific repository interfaces.
- Add outbox processors/background workers.
- Add idempotency and inbox/processed-event tracking.
- Add Aspire MongoDB provisioning if local orchestration should start Mongo automatically.

---

## Pending Work

- Seed data: default roles `Admin`, `Customer`, and initial admin user.
- Full integration tests with live PostgreSQL and Redis.
- Future chat module design: conversation/member relational model, message documents, notification documents, delivery/read models.
- MongoDB background outbox processing.
- Decide whether AppHost should provision MongoDB locally or use an external Mongo connection.

---

## Architecture Compliance

| Rule | Status |
|---|---|
| Domain has no Application dependency | Passing |
| Domain has no Persistence dependency | Passing |
| Domain has no Infrastructure dependency | Passing |
| Application has no Persistence dependency | Passing |
| Application has no Infrastructure dependency | Passing |
| Presentation has no Domain dependency | Passing |
| Presentation has no Persistence dependency | Passing |
| Contract has no Application/Persistence dependency | Passing |
| Entities reside in Domain layer | Passing |
| Domain exceptions inherit DomainException | Passing |
