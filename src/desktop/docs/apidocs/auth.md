# Authentication API

Base route:

```text
/api/v1/auth
```

## Response Models

### AuthResponse

```json
{
  "tokenType": "Bearer",
  "accessToken": "eyJhbGciOi...",
  "refreshToken": "base64-or-random-token",
  "accessTokenExpiresAt": "2026-06-10T01:15:00Z",
  "refreshTokenExpiresAt": "2026-07-10T01:00:00Z",
  "user": {
    "id": "f5e31f66-f3af-4c8a-9ec1-f29b91e2b06e",
    "email": "member@example.com",
    "firstName": "Jane",
    "lastName": "Doe",
    "fullName": "Jane Doe",
    "avatarUrl": "https://cdn.example.com/avatar.png",
    "bio": "Team member",
    "isActive": true,
    "roles": ["Customer"]
  }
}
```

### RegisterResponse

```json
{
  "message": "User registered successfully. Please check your email to activate the account."
}
```

## Error Codes

| Code | Message | Current HTTP Status |
|---|---|---:|
| `Auth.InvalidCredentials` | Email or password is incorrect. | `400` |
| `Auth.AccountInactive` | Account is not active. Please confirm your email first. | `400` |
| `Auth.AccountLocked` | Account is locked. Please try again later. | `400` |
| `Auth.InvalidToken` | Access token is invalid or cannot be parsed, or email confirmation token is invalid. | `400` |
| `Auth.InvalidRefreshToken` | Refresh token not found. | `400` |
| `Auth.RefreshTokenExpiredOrRevoked` | Refresh token has expired or been revoked. Please log in again. | `400` |
| `Auth.TokenMismatch` | Access token and refresh token do not match. | `400` |
| `Auth.UserNotFound` | User not found. | `400` |
| `Auth.EmailAlreadyConfirmed` | Email is already confirmed. | `400` |
| `Auth.TooManyRequests` | Too many OTP requests. Please wait before trying again. | `400` |
| `Auth.AccountBlocked` | Too many failed OTP attempts. Please request a new OTP. | `400` |
| `Auth.InvalidOtp` | OTP is invalid or has expired. | `400` |
| `Auth.EmailAlreadyExists` | Email is already registered. | `400` |
| `Auth.RegistrationFailed` | Identity registration failed. | `400` |
| `Auth.ResetFailed` | Password reset failed. | `400` |
| `GoogleLogin.RegistrationFailed` | Auto-registration failed. | `400` |

## POST /register

Registers a new inactive user and queues an email confirmation message through the integration outbox.

Auth: anonymous

Request:

```json
{
  "email": "member@example.com",
  "password": "Str0ng!Pass",
  "firstName": "Jane",
  "lastName": "Doe",
  "dayOfBirth": "1995-04-20T00:00:00Z"
}
```

Success:

```text
202 Accepted
```

Response:

```json
{
  "message": "User registered successfully. Please check your email to activate the account."
}
```

Validation:

| Field | Rules |
|---|---|
| `email` | Required, valid email, max 256 chars. |
| `password` | Required, min 8 chars, at least one uppercase letter, one digit, one special character. |
| `firstName` | Required, max 100 chars. |
| `lastName` | Required, max 100 chars. |
| `dayOfBirth` | Required, must be in the past, must be within the last 120 years. |

Business errors:

| Code | When |
|---|---|
| `Auth.EmailAlreadyExists` | A user already exists with the same email. |
| `Auth.RegistrationFailed` | Identity user creation fails. |

## POST /verify-email

Confirms email with an ASP.NET Identity email confirmation token and returns a JWT token pair.

Auth: anonymous

Request:

```json
{
  "email": "member@example.com",
  "token": "identity-email-confirmation-token"
}
```

Success:

```text
200 OK
```

Response: `AuthResponse`

Business errors:

| Code | When |
|---|---|
| `Auth.UserNotFound` | No user exists for the email. |
| `Auth.EmailAlreadyConfirmed` | The email is already confirmed. |
| `Auth.InvalidToken` | The confirmation token is invalid or expired. |

## GET /verify-email

Magic-link variant of email verification.

Auth: anonymous

Query parameters:

| Name | Type | Required | Notes |
|---|---|---:|---|
| `email` | string | Yes | User email. |
| `token` | string | Yes | Must be URL-encoded in the link. |

Example:

```text
GET /api/v1/auth/verify-email?email=member@example.com&token=<url-encoded-token>
```

Success:

```text
200 OK
```

Response: `AuthResponse`

Business errors are the same as `POST /verify-email`.

## POST /login

Authenticates an active, email-confirmed user and returns a JWT token pair.

Auth: anonymous

Request:

```json
{
  "email": "member@example.com",
  "password": "Str0ng!Pass"
}
```

Success:

```text
200 OK
```

Response: `AuthResponse`

Validation:

| Field | Rules |
|---|---|
| `email` | Required, valid email. |
| `password` | Required, min 8 chars. |

Business errors:

| Code | When |
|---|---|
| `Auth.InvalidCredentials` | Email does not exist or password is wrong. |
| `Auth.AccountInactive` | Email is not confirmed or user is inactive. |
| `Auth.AccountLocked` | Identity account is locked. |

## POST /refresh

Rotates a refresh token and returns a new access token plus refresh token.

Auth: anonymous

Request:

```json
{
  "accessToken": "expired-or-current-access-token",
  "refreshToken": "current-refresh-token"
}
```

Success:

```text
200 OK
```

Response: `AuthResponse`

Validation:

| Field | Rules |
|---|---|
| `accessToken` | Required. |
| `refreshToken` | Required. |

Business errors:

| Code | When |
|---|---|
| `Auth.InvalidToken` | Access token cannot be parsed for `jti`. |
| `Auth.InvalidRefreshToken` | Refresh token does not exist. |
| `Auth.RefreshTokenExpiredOrRevoked` | Refresh token is expired, used, or revoked. If reuse is detected, all sessions are revoked. |
| `Auth.TokenMismatch` | Access token `jti` does not match refresh token `jti`. |
| `Auth.UserNotFound` | Associated user no longer exists. |

## POST /logout

Blacklists the current access token `jti` in Redis and revokes the supplied refresh token if it is active.

Auth: required

Headers:

```http
Authorization: Bearer <accessToken>
```

Request:

```json
{
  "refreshToken": "current-refresh-token"
}
```

Success:

```text
204 No Content
```

Notes:

- The endpoint reads `jti` and user id from the authenticated JWT claims.
- The raw bearer token is read from the `Authorization` header only to compute blacklist TTL.
- If the refresh token is not found or already inactive, the endpoint still succeeds after blacklisting the access token.

Middleware errors:

| Scenario | HTTP Status |
|---|---:|
| Missing, invalid, expired, or blacklisted bearer token | `401` |

## POST /revoke-all

Revokes all active refresh tokens for the current user.

Auth: required

Headers:

```http
Authorization: Bearer <accessToken>
```

Request body: none

Success:

```text
204 No Content
```

Middleware errors:

| Scenario | HTTP Status |
|---|---:|
| Missing, invalid, expired, or blacklisted bearer token | `401` |

## POST /forgot-password

Requests a 6-digit password reset OTP. Existing users receive an OTP through the integration outbox. Unknown emails intentionally return success to avoid account enumeration.

Auth: anonymous

Request:

```json
{
  "email": "member@example.com"
}
```

Success:

```text
202 Accepted
```

Response body: empty

Validation:

| Field | Rules |
|---|---|
| `email` | Required, valid email. |

Business errors:

| Code | When |
|---|---|
| `Auth.TooManyRequests` | More than 3 OTP requests within 15 minutes for the same user email. |

## POST /reset-password

Validates the reset OTP, resets the password, and revokes all active sessions for the user.

Auth: anonymous

Request:

```json
{
  "email": "member@example.com",
  "otp": "123456",
  "newPassword": "N3w!StrongPass"
}
```

Success:

```text
204 No Content
```

Validation:

| Field | Rules |
|---|---|
| `email` | Required, valid email. |
| `otp` | Required, exactly 6 characters. |
| `newPassword` | Required, min 8 chars, at least one uppercase letter, one lowercase letter, one number, one special character. |

Business errors:

| Code | When |
|---|---|
| `Auth.UserNotFound` | No user exists for the email. |
| `Auth.AccountBlocked` | OTP max attempts reached. |
| `Auth.InvalidOtp` | OTP is invalid or expired. |
| `Auth.ResetFailed` | Identity password reset fails. |

## POST /google-login

Validates a Google ID token. If the email does not exist, the API auto-registers a user. If an existing user is inactive, Google login activates and confirms the user.

Auth: anonymous

Request:

```json
{
  "idToken": "google-id-token"
}
```

Success:

```text
200 OK
```

Response: `AuthResponse`

Business errors:

| Code | When |
|---|---|
| Provider-specific Google validation error | Google ID token validation fails. |
| `GoogleLogin.RegistrationFailed` | Auto-registration from Google payload fails. |

