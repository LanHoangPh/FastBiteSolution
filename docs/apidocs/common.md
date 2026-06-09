# Common API Conventions

## Base URL

```text
/api/v1
```

## Authentication

Protected endpoints require a JWT access token:

```http
Authorization: Bearer <accessToken>
```

Authentication is enforced by ASP.NET Core JWT bearer middleware and token blacklist middleware. A logged-out or blacklisted token is rejected before endpoint handlers run.

## JSON Naming

Request and response examples use camelCase JSON property names, matching the default ASP.NET Core JSON behavior.

Contract records use PascalCase in C#, for example `AccessToken`, which is serialized as:

```json
{
  "accessToken": "..."
}
```

## Success Responses

Successful responses use the endpoint-specific status code:

| Status | Meaning |
|---:|---|
| `200 OK` | Request succeeded and returns a body. |
| `201 Created` | Resource was created and returns a body. |
| `202 Accepted` | Request was accepted for asynchronous or follow-up processing. |
| `204 No Content` | Request succeeded and returns no body. |

## Validation Errors

Validation failures return `400 Bad Request` with `ProblemDetails`.

Shape:

```json
{
  "type": "Validation.Error",
  "title": "Validation Error",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "errors": [
    {
      "code": "Validation.Error",
      "message": "Email is required."
    }
  ]
}
```

The exact validation `code` depends on the validation pipeline's shared error type. The `message` values listed in each endpoint come from FluentValidation validators.

## Business Errors

Business errors return `ProblemDetails`.

Shape:

```json
{
  "type": "Auth.InvalidCredentials",
  "title": "Bad Request",
  "status": 400,
  "detail": "Email or password is incorrect."
}
```

## Current Error Status Mapping

There are two failure mapping paths in `ApiEndpoint`.

For non-generic `Result` failures:

| Error Code Contains | HTTP Status |
|---|---:|
| `NotFound` | `404 Not Found` |
| `Conflict` | `409 Conflict` |
| `Unauthorized` | `401 Unauthorized` |
| `Forbidden` | `403 Forbidden` |
| Any other code | `400 Bad Request` |

For generic `Result<T>` failures, the current implementation always returns:

```text
400 Bad Request
```

Most Auth and Workspace endpoints return `Result<T>`, so their business failures are documented with current HTTP status `400` unless middleware handles the error first.

## Common Middleware Errors

| Scenario | Typical HTTP Status | Notes |
|---|---:|---|
| Missing `Authorization` header on protected endpoint | `401 Unauthorized` | JWT middleware rejects the request. |
| Invalid/expired access token on protected endpoint | `401 Unauthorized` | JWT middleware rejects the request. |
| Blacklisted access token | `401 Unauthorized` | Token blacklist middleware rejects the request. |
| Unhandled server exception | `500 Internal Server Error` | Global exception handler should not expose production details. |

