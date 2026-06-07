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
| POST | `/verify-email` | `VerifyEmailCommand` | Implemented |
| GET | `/verify-email?email={email}&token={token}` | `VerifyEmailCommand` | Implemented |
| POST | `/login` | `LoginCommand` | Implemented |
| POST | `/refresh` | `RefreshTokenCommand` | Implemented |
| POST | `/logout` | `LogoutCommand` | Implemented |
| POST | `/revoke-all` | `RevokeAllSessionsCommand` | Implemented |
| POST | `/forgot-password` | `ForgotPasswordCommand` | Implemented |
| POST | `/reset-password` | `ResetPasswordCommand` | Implemented |
| POST | `/google-login` | `GoogleLoginCommand` | Implemented |

Logout extracts the JWT `jti` from claims rather than trusting the request body.

### Email Verification

Email confirmation uses ASP.NET Identity confirmation tokens, not OTP. OTP remains reserved for password reset and similar short-lived verification flows.

#### POST `/api/v1/auth/verify-email`

Request body:

```json
{
  "email": "member@example.com",
  "token": "identity-email-confirmation-token"
}
```

Response:
- `200 OK` + `AuthResponse`

#### GET `/api/v1/auth/verify-email?email={email}&token={token}`

Used by email magic links. The token must be URL-encoded in the link.

Response:
- `200 OK` + `AuthResponse`

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

## Workspace Endpoints

Base route:

```text
/api/v1/workspaces
```

All Workspace endpoints require JWT authentication.

Workspace is the tenant boundary for future Channels, Messages, Social Feed, and Files. Frontend should treat `workspaceId` as required context after login.

### Common Response Types

`WorkspaceResponse`

```json
{
  "workspaceId": "0f5e0d4e-1b8f-4b3c-930f-9a8a68a8c7b1",
  "workspaceName": "Acme Team",
  "description": "Internal workspace",
  "workspaceType": "Private",
  "privacy": "Private",
  "workspaceAvatarUrl": "https://cdn.example.com/logo.png",
  "currentUserRole": "Owner",
  "createdAt": "2026-06-07T14:34:31Z",
  "isArchived": false,
  "memberCount": 1
}
```

`WorkspaceInvitationResponse`

```json
{
  "invitationId": 12,
  "workspaceId": "0f5e0d4e-1b8f-4b3c-930f-9a8a68a8c7b1",
  "workspaceName": "Acme Team",
  "description": "Internal workspace",
  "workspaceAvatarUrl": "https://cdn.example.com/logo.png",
  "invitedByUserId": "a3dff0ca-7f0e-42bb-b0f4-70112fdc6ab0",
  "createdAt": "2026-06-07T14:34:31Z",
  "expiresAt": "2026-06-14T14:34:31Z"
}
```

### Onboarding and Navigation

| Method | Route | Purpose | Request | Response |
|---|---|---|---|---|
| GET | `/me` | Get workspaces where current user is an active member. FE uses this after login for empty state/sidebar. Uses Redis cache. | None | `200 OK` + `WorkspaceResponse[]` |
| GET | `/invitations/me` | Get pending invitations for the current user's email. FE uses this in onboarding empty state. | None | `200 OK` + `WorkspaceInvitationResponse[]` |

`GET /me` behavior:
- Returns only active memberships.
- Excludes archived/deleted workspaces.
- Returns `[]` for new users with no workspace.

### Workspace Management

#### POST `/api/v1/workspaces`

Creates a workspace. The current user becomes `Owner`.

Request body:

```json
{
  "workspaceName": "Acme Team",
  "description": "Internal workspace",
  "workspaceType": 1,
  "privacy": 2,
  "workspaceAvatarUrl": "https://cdn.example.com/logo.png"
}
```

Enum values:
- `workspaceType`: `1 = Private`, `2 = Public`, `3 = Community`
- `privacy`: `1 = Public`, `2 = Private`

Response:
- `201 Created`
- Body: `WorkspaceResponse`

#### GET `/api/v1/workspaces/{workspaceId}`

Gets workspace detail for the selected workspace.

Request:
- Route: `workspaceId` as GUID

Response:
- `200 OK` + `WorkspaceDetailResponse`
- Only active members can access.

#### PATCH `/api/v1/workspaces/{workspaceId}`

Updates workspace profile/settings. Only `Owner` or `Admin` can update.

Request body:

```json
{
  "workspaceName": "Acme Team",
  "description": "Updated description",
  "workspaceType": 1,
  "privacy": 2,
  "workspaceAvatarUrl": "https://cdn.example.com/new-logo.png"
}
```

Response:
- `200 OK` + `WorkspaceResponse`

#### DELETE `/api/v1/workspaces/{workspaceId}`

Archives a workspace. This is a soft archive, not hard delete. Only `Owner` can archive.

Request:
- Route: `workspaceId` as GUID

Response:
- `204 No Content`

### Members

#### GET `/api/v1/workspaces/{workspaceId}/members`

Gets active members in a workspace. Any active member can view the list.

Response:

```json
[
  {
    "workspaceMemberId": 1,
    "userId": "a3dff0ca-7f0e-42bb-b0f4-70112fdc6ab0",
    "email": "owner@example.com",
    "fullName": "Owner User",
    "avatarUrl": "https://cdn.example.com/avatar.png",
    "role": "Owner",
    "status": "Active",
    "joinedAt": "2026-06-07T14:34:31Z"
  }
]
```

Role values:
- `Owner`
- `Admin`
- `Moderator`
- `Member`

Member status values:
- `Pending`
- `Active`
- `Banned`
- `Left`

### Invitations and Join

#### POST `/api/v1/workspaces/{workspaceId}/invitations`

Creates a pending invitation for any email address. The invited user does not need to exist yet. Only `Owner` or `Admin` can invite.

Request body:

```json
{
  "email": "member@example.com"
}
```

Response:
- `201 Created` + `WorkspaceInvitationResponse`

Behavior:
- Stores normalized lowercase email.
- Default expiry is 7 days.
- Prevents duplicate pending invitations for the same email/workspace.
- If the user already exists and is an active member, returns conflict.
- Current MVP stores the record only; it does not send invitation email yet.

#### POST `/api/v1/workspaces/invitations/{invitationId}/accept`

Accepts a pending email invitation for the current authenticated user's email.

Request:
- Route: `invitationId` as integer

Response:
- `200 OK` + `WorkspaceResponse`

Behavior:
- Current user's email must match the invitation email.
- Adds user as `Member` with `Active` status.
- Existing left member can rejoin; banned member cannot.

#### POST `/api/v1/workspaces/{workspaceId}/invite-links`

Creates a shared invite code/link for a workspace. Only `Owner` or `Admin` can create.

Request body:

```json
{
  "expiresAt": "2026-06-14T14:34:31Z",
  "maxUses": 25
}
```

Both fields are optional.

Response:

```json
{
  "invitationId": 7,
  "workspaceId": "0f5e0d4e-1b8f-4b3c-930f-9a8a68a8c7b1",
  "invitationCode": "9F2A8C1B0D3E4F5A",
  "createdAt": "2026-06-07T14:34:31Z",
  "expiresAt": "2026-06-14T14:34:31Z",
  "maxUses": 25,
  "currentUses": 0,
  "isActive": true
}
```

#### POST `/api/v1/workspaces/join`

Joins a workspace using a shared invite code.

Request body:

```json
{
  "invitationCode": "9F2A8C1B0D3E4F5A"
}
```

Response:
- `200 OK` + `WorkspaceResponse`

Behavior:
- Code must exist, be active, not expired, and not exceed `maxUses`.
- Adds user as `Member` with `Active` status.
- Existing left member can rejoin; banned member cannot.

### Workspace Error Codes

Errors are returned as `ProblemDetails`.

| Error Code | Meaning |
|---|---|
| `Workspace.NotFound` | Workspace does not exist or is archived. |
| `Workspace.Forbidden` | Current user is not allowed to access or modify the workspace. |
| `Workspace.InvitationNotFound` | Invitation does not exist or does not belong to current user's email. |
| `Workspace.InvitationExpired` | Invitation has expired. |
| `Workspace.InvitationInactive` | Shared invite code is inactive. |
| `Workspace.InvitationMaxUsesReached` | Shared invite code reached max uses. |
| `Workspace.InvitationConflict` | Duplicate pending email invitation exists. |
| `Workspace.MemberConflict` | User is already an active member. |
| `Workspace.LastOwnerRequired` | Workspace must keep at least one active owner. |

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

Verify Email
  -> validate ASP.NET Identity email confirmation token
  -> set EmailConfirmed to true and IsActive to true
  -> issue new token pair (auto login)

Forgot Password
  -> validate user exists
  -> generate 6-digit OTP and store in Redis
  -> enqueue Integration Event to Outbox for async email delivery

Reset Password
  -> validate OTP with limits (MaxAttempts)
  -> call UserManager to reset password
  -> bulk revoke all active sessions for security

Google Login
  -> validate Google ID token signature via `Google.Apis.Auth`
  -> check if user exists by email -> auto-register with random secure password if not found
  -> auto-activate user if not active
  -> issue new token pair
```

---

## MongoDB / Background Work Status

MongoDB is registered only when configured. Current Mongo capability is infrastructure-level only:
- `IIntegrationOutboxStore`
- `MongoIntegrationOutboxStore`
- outbox document and indexes

There are not yet chat/message/notification API endpoints or background outbox processors. Future chat features should add explicit commands, documents, repositories, and processors around this scaffold.
