# WPF JWT & Session Guidelines

## 1. Authentication Targets
- App authenticates using JWT.
- Backend provides: `accessToken`, `refreshToken`, `accessTokenExpiresAt`, `refreshTokenExpiresAt`.
- Use `Authorization: Bearer <access_token>`.

## 2. Storage Strategy
- **Access Token**: In-memory ONLY (`ITokenProvider`). Never written to disk.
- **Refresh Token**: Persistent secure storage (`ISecureTokenStore`). Uses Windows DPAPI (`DataProtectionScope.CurrentUser`). Never plain text.

## 3. Access Token Rules
- Short-lived. Always fetched via memory provider.

## 4. Refresh Token Rules
- Long-lived (7-30 days).
- Kept in `ISecureTokenStore`.

## 5. Login Flow
- POST `/api/v1/auth/login` -> Store tokens -> Navigate to Main Window.

## 6. Auto Login / App Startup Flow
- Read refresh token from DPAPI.
- If none -> Login Window.
- If present -> POST `/api/v1/auth/refresh`.
- If successful -> Store new tokens -> Main Window.
- If fails -> Clear tokens -> Login Window.

## 7. HTTP Headers
- Managed centrally by `AuthHeaderHandler` (a `DelegatingHandler`). No manual header appending in VMs or Refit services.

## 8. Refresh Token on 401
- Caught globally by `RefreshTokenHandler`.
- Locks via `SemaphoreSlim` to prevent multiple simultaneous refresh calls.
- Fetches new token, updates memory, and retries original request once.

## 9. Current User State
- Loaded after successful authentication. Managed by `ICurrentUserService`. 
- Data fetched from `/api/v1/user/me` rather than relying entirely on JWT claims.

## 10. Global Unauthorized Events
- Uses `IMessenger` to broadcast `SessionExpiredMessage`.
- Shell/App listens to navigate to Login Window globally.

## 11. Security
- Never log JWT tokens.
- Handle multi-threading correctly when refreshing.
