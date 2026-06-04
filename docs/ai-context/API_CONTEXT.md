# API_CONTEXT.md - FastBite Solution

## API Overview

The API uses Minimal APIs in the Presentation layer. Endpoints are thin wrappers:

1. Accept HTTP input.
2. Create Contract command/query.
3. Send via MediatR `ISender`.
4. Convert `Result<T>` / `Result` to HTTP response.

API host:

```text
src/backend/FastBiteGroup.API
```

Endpoint definitions:

```text
src/backend/FastBiteGroup.Presentation/APIs
```

---

## API Versioning

- URL-based: `/api/v{version:apiVersion}/...`
- Library: `Asp.Versioning.Http`
- V1 is active.
- V2 folder exists as scaffold only.

---

## Auth Endpoints

Base route:

```text
/api/v1/auth
```

| Method | Route | Command | Status |
|---|---|---|---|
| POST | `/register` | `RegisterCommand` | Implemented |
| POST | `/login` | `LoginCommand` | Implemented |
| POST | `/refresh` | `RefreshTokenCommand` | Implemented |
| POST | `/logout` | `LogoutCommand` | Implemented |
| POST | `/revoke-all` | `RevokeAllSessionsCommand` | Implemented |

Logout extracts the JWT `jti` from claims rather than trusting the request body.

---

## Product Endpoints

Base route:

```text
/api/v1/products
```

| Method | Route | Command/Query | Status |
|---|---|---|---|
| GET | `/` | `GetAllProductsQuery` | Implemented |
| GET | `/{id}` | `GetProductByIdQuery` | Implemented |
| POST | `/` | `CreateProductCommand` | Implemented |
| PUT | `/{id}` | `UpdateProductCommand` | Implemented |
| DELETE | `/{id}` | `DeleteProductCommand` | Implemented |

Product endpoints require authentication.

---

## Response and Error Pattern

Application handlers return `Result<T>` or `Result`.

`ApiEndpoint.HandleFailure()` maps errors to `ProblemDetails`.

Status mapping convention:
- Code contains `NotFound` -> 404
- Code contains `Conflict` -> 409
- Code contains `Unauthorized` -> 401
- Code contains `Forbidden` -> 403
- Default -> 400

---

## Middleware Pipeline

Current API startup flow includes:

```text
builder.AddServiceDefaults()
AddPostgreSqlPersistence
AddMongoPersistence
AddIdentityPersistence
AddRepositoryPersistence
AddRedisInfrastructure
AddSecurityInfrastructure
AddApplicationServices
AddJwtAuthentication
AddEndpoints
Swagger/OpenAPI
ProblemDetails + GlobalExceptionHandler
API versioning

app.MapEndpoints()
app.MapDefaultEndpoints()
app.UseExceptionHandler()
app.ConfigureSwagger() in Development/Staging
app.UseSerilogRequestLogging()
app.UseAuthentication()
app.UseTokenBlacklist()
app.UseAuthorization()
```

---

## Auth Flow

```text
Login
  -> validate credentials through IUserAuthService
  -> generate JWT + refresh token
  -> persist refresh token

Refresh
  -> validate expired access token JTI
  -> validate refresh token
  -> mark old token used
  -> issue new token pair

Logout
  -> blacklist JWT jti in Redis
  -> revoke refresh token in PostgreSQL

Revoke all
  -> bulk revoke refresh tokens for user
```

---

## MongoDB / Background Work Status

MongoDB is registered only when configured. Current Mongo capability is infrastructure-level only:
- `IIntegrationOutboxStore`
- `MongoIntegrationOutboxStore`
- outbox document and indexes

There are not yet chat/message/notification API endpoints or background outbox processors. Future chat features should add explicit commands, documents, repositories, and processors around this scaffold.
