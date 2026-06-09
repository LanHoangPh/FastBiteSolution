# FastBiteGroup API Documentation

This folder documents the currently mapped HTTP APIs from source code in:

- `src/backend/FastBiteGroup.Presentation/APIs/AuthApi.cs`
- `src/backend/FastBiteGroup.Presentation/APIs/WorkspaceApi.cs`
- `src/backend/FastBiteGroup.Contract/Services/V1`
- `src/backend/FastBiteGroup.Application/Validators`

Current API version: `v1`

Base route convention:

```text
/api/v{version:apiVersion}
```

For local development, replace `{version}` with `1`, for example:

```text
/api/v1/auth/login
```

## Available Documents

| Document | Scope |
|---|---|
| [Common Conventions](./common.md) | Authentication header, response casing, validation and ProblemDetails shape. |
| [Authentication API](./auth.md) | Register, verify email, login, refresh, logout, revoke all, forgot/reset password, Google login. |
| [Workspace API](./workspaces.md) | Workspace onboarding, detail, members, invitations, invite links, join, update, archive. |

## Currently Mapped API Modules

| Module | Base Route | Auth Required | Status |
|---|---|---:|---|
| Authentication | `/api/v1/auth` | Mixed | Implemented |
| Workspace | `/api/v1/workspaces` | Yes | Implemented |

## Important Source-of-Truth Notes

- Product endpoints are mentioned in `docs/ai-context/API_CONTEXT.md`, but no `ProductApi` endpoint class is currently mapped under `FastBiteGroup.Presentation/APIs`. They are therefore not documented here as available HTTP endpoints.
- MongoDB is infrastructure-only for now. There are no chat/message/notification HTTP endpoints yet.
- Most business failures return `ProblemDetails`.
- Current generic result failure mapping in `ApiEndpoint.HandleFailure<TValue>()` returns `400 Bad Request` for all `Result<T>` failures. This means some errors with codes such as `Workspace.Forbidden` or `Workspace.InvitationNotFound` currently return HTTP `400` from endpoints that return a response body. See each endpoint's error table for details.

