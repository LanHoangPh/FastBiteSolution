# API_CONTEXT.md — FastBite Solution

## API Overview

The API follows **Clean Architecture + CQRS**. Endpoints are thin wrappers that:
1. Accept HTTP request.
2. Map to a Command or Query (Contract layer).
3. Send via `ISender` (MediatR).
4. Map `Result<T>` to HTTP response using `ApiEndpoint.HandleFailure()`.

---

## API Versioning Strategy

- URL-based: `/api/v{version:apiVersion}/...`
- Library: `Asp.Versioning.Http v10.0.0`
- **V1**: Active (definitions exist, endpoints EMPTY).
- **V2**: Folder scaffold only, no content.

---

## Modules

### Authentication Module (`/api/v1/auth`)

**Class**: `AuthApi` in `FastBiteGroup.Presentation/APIs/AuthApi.cs`

**Defined Commands (Contract)**:

| Command | Input Fields | Response | Description |
|---|---|---|---|
| `LoginCommand` | `Email`, `Password` | `Result<AuthResponse>` | Authenticate user, return JWT pair |
| `RegisterCommand` | `Email`, `Password`, `FirstName`, `LastName`, `DayOfBirth` | `Result<AuthResponse>` | Create new user account |
| `RefreshTokenCommand` | `AccessToken`, `RefreshToken` | `Result<AuthResponse>` | Rotate refresh token, issue new JWT pair |
| `LogoutCommand` | `Jti`, `RefreshToken`, `UserId` | `Result` | Blacklist JWT in Redis, revoke refresh token |
| `RevokeAllSessionsCommand` | `UserId` | `Result` | Revoke all active refresh tokens for user |

**Defined Queries (Contract)**:

| Query | Input | Response | Notes |
|---|---|---|---|
| `AuthQueries.Token` | `AccessToken?`, `RefreshToken?` | `Result<AuthResponse>` | Deprecated — superseded by `RefreshTokenCommand` |

**Auth Response Shape**:
```json
{
  "accessToken": "eyJ...",
  "refreshToken": "...",
  "accessTokenExpiresAt": "2026-01-01T00:00:00Z",
  "refreshTokenExpiresAt": "2026-01-31T00:00:00Z",
  "user": {
    "id": "guid",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "roles": ["Customer"]
  }
}
```

---

### Product Module (`/api/v1/products`)

**Class**: `ProductApi` in `FastBiteGroup.Presentation/APIs/ProductApi.cs`
**Defined Commands (Contract)**:

| Command | Input Fields | Response | Description |
|---|---|---|---|
| `CreateProductCommand` | `Name`, `Description`, `Price` | `Result<int>` | Create product, return new ID |
| `UpdateProductCommand` | `Id`, `Name`, `Description`, `Price` | `Result` | Update existing product |
| `DeleteProductCommand` | `Id` | `Result` | Delete product |

**Product Queries**: Not yet defined in Contract.

---

## Authentication Flow

```
POST /api/v1/auth/login
  → LoginCommand { Email, Password }
  → Validate credentials against ASP.NET Identity (planned)
  → Generate JWT Access Token (HMAC-SHA256, signed with JwtOptions.SecretKey)
  → Generate Refresh Token (opaque string, stored in DB as RefreshToken entity)
  → Return AuthResponse { AccessToken, RefreshToken, Expiry, UserInfo }

POST /api/v1/auth/refresh
  → RefreshTokenCommand { AccessToken, RefreshToken }
  → Validate: RefreshToken.IsActive, Jti matches AccessToken.jti
  → MarkUsed(newToken) on old RefreshToken
  → Issue new JWT + new RefreshToken
  → Return new AuthResponse

POST /api/v1/auth/logout
  → LogoutCommand { Jti, RefreshToken, UserId }
  → ICacheService.BlacklistTokenAsync(jti, remainingLifetime)
  → IRefreshTokenRepository.FindByTokenAsync → Revoke()
  → Save changes

POST /api/v1/auth/revoke-all
  → RevokeAllSessionsCommand { UserId }
  → IRefreshTokenRepository.RevokeAllForUserAsync(userId)
```

---

## Authorization Flow

```
Incoming request with Bearer token
  ↓
[JwtBearerAuthentication middleware]
  - Validates signature (SymmetricSecurityKey from JwtOptions.SecretKey)
  - Validates Issuer, Audience, Lifetime
  - ClockSkew = TimeSpan.Zero (exact expiry)
  - On failure: returns 401 with application/problem+json
  ↓
[TokenBlacklistMiddleware]
  - Extracts jti claim from authenticated user
  - Calls ICacheService.IsTokenBlacklistedAsync(jti)
  - If blacklisted: returns 401 with "session revoked" message
  ↓
[Controller / Endpoint authorization attribute]
  - [Authorize] → requires authenticated user
  - Role-based [Authorize(Roles = "...")] → coming once Identity is integrated
```

---

## Error Response Format

All errors return `application/problem+json` (RFC 7807):

```json
// Validation error (400)
{
  "title": "Validation Error",
  "type": "ValidationError.Code",
  "detail": "...",
  "status": 400,
  "errors": [
    { "code": "Email", "message": "Email is required." }
  ]
}

// Not found (404)
{
  "title": "Not Found",
  "type": "Product.NotFound",
  "detail": "Product with id 42 not found.",
  "status": 404
}

// Server error (500)
{
  "title": "Server Error",
  "detail": "An unexpected error occurred.",
  "status": 500
}
```

**Error code → HTTP status mapping** (in `ApiEndpoint.GetStatusCode`):
- Contains `"NotFound"` → 404
- Contains `"Conflict"` → 409
- Contains `"Unauthorized"` → 401
- Contains `"Forbidden"` → 403
- Default → 400

---

## MediatR Pipeline (Request Lifecycle)

All Commands and Queries go through this ordered pipeline:

```
1. PerformancePipelineBehavior    — Logs warning if handler takes > 5000ms
2. TracingPipelineBehaviors       — Logs success/failure + elapsed time
3. TransactionPipelineBehaviors   — Wraps command in DB transaction (via IUnitOfWork)
4. ValidationPipelineBehaviors    — Runs all FluentValidation validators, returns ValidationResult on failure
5. Handler                        — Actual use case implementation
```

---

## Middleware Pipeline (Program.cs)

```
builder.AddServiceDefaults()         ← OTel, health checks, service discovery
builder.Services.AddApplicationServices(config)  ← MediatR, AutoMapper, FluentValidation
builder.Services.AddAuthorization()
builder.Services.AddOpenApi()

app.UseHttpsRedirection()
app.UseAuthorization()               ← Note: Authentication middleware is added but UseAuthentication() call is MISSING in Program.cs
app.MapControllers()
app.MapDefaultEndpoints()            ← /health, /alive (dev only)
```


---

## Caching Strategy

| Cache Key Pattern | Description |
|---|---|
| `auth:blacklist:jti:{jti}` | Blacklisted JWT JTI, TTL = remaining token lifetime |
| `user:profile:{userId}` | User profile cache (planned) |
| `products:all` | All products cache (planned) |

---

## Background Jobs

- **Unknown from current repository state.** No background jobs or hosted services are defined.

---

## External Integrations

- **Unknown from current repository state.** No external HTTP clients, email providers, payment gateways, or third-party APIs are configured.

---

## Planned Endpoints (Not Yet Implemented)

Based on defined Commands and the AppHost topology:

| Method | Route | Command | Status |
|---|---|---|---|
| POST | `/api/v1/auth/register` | `RegisterCommand` | implemented |
| POST | `/api/v1/auth/login` | `LoginCommand` |  implemented |
| POST | `/api/v1/auth/refresh` | `RefreshTokenCommand` | implemented |
| POST | `/api/v1/auth/logout` | `LogoutCommand` | implemented |
| POST | `/api/v1/auth/revoke-all` | `RevokeAllSessionsCommand` |implemented |
| GET | `/api/v1/products` | Query (not defined) | implemented |
| GET | `/api/v1/products/{id}` | Query (not defined) |implemented |
| POST | `/api/v1/products` | `CreateProductCommand` | implemented |
| PUT | `/api/v1/products/{id}` | `UpdateProductCommand` |Not implemented |
| DELETE | `/api/v1/products/{id}` | `DeleteProductCommand` |Not implemented |
