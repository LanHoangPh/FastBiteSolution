# CURRENT_STATUS.md - FastBiteGroup

**Last Updated:** 2026-06-07

---

## Overall Status

Build passing. Architecture tests passing 10/10.

The project has a working PostgreSQL/EF Core foundation and an optional MongoDB.Driver scaffold for future document-oriented workloads.

---

## Completed Work

### Domain

- `Products` entity with factory/update methods and product domain exceptions.
- `AppRefreshToken` entity with refresh-token rotation/revocation behavior.
- Entity base abstractions and audit/soft-delete interfaces.
- Repository interfaces: `IRepositoryBase<TEntity, TKey>`, `IRefreshTokenRepository`.
- `IUnitOfWork` abstraction for EF Core transaction boundaries.
- Domain exceptions inherit `DomainException`.

### Contract

- Command/query abstractions for MediatR.
- `Result`, `Result<T>`, validation result, `Error`, paged result.
- Auth contracts: login, register, verify email, refresh, logout, revoke all, forgot password, reset password, google login.
- Product contracts: create, update, delete, get all, get by id, product response.
- Outbox contracts: `IntegrationOutboxMessage`, `IIntegrationOutboxStore`.

### Persistence

- `ApplicationDbContext` with Identity persistence.
- `AppUser`, `AppRole`.
- EF configurations for products and refresh tokens.
- Existing EF migration under `src/backend/FastBiteGroup.Persistence/Migrations`.
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
- Product handlers: create, update, delete, get all, get by id.
- Validators for auth/product commands.
- AutoMapper profile for product response mapping.
- MediatR pipeline behaviors: performance, tracing, transaction, validation.

### Presentation/API

- Auth API: register, verify email, login, refresh, logout, revoke all, forgot password, reset password, google login.
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
| Contract tests | Build/pass available |
| Domain tests | Build/pass available |
| Application tests | Build/pass available (Expanded with extensive tests for Auth Flows: OTP, Forgot Password, Reset Password, etc.) |
| Infrastructure tests | Build/pass available (OTP Service Mocking tests) |
| Integration tests | Require live PostgreSQL/Redis environment |

Last verified commands:

```bash
dotnet build FastBiteSolution.slnx
dotnet test tests/backend/FastBiteGroup.Architecture.Tests/FastBiteGroup.Architecture.Tests.csproj
```

Known warnings:
- NuGet vulnerability warnings for transitive packages `SharpCompress` and `Snappier`.

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
