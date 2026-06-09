# Workflow: API Integration

Use this when connecting the desktop app to backend HTTP APIs.

## 1. Scope

API access must not be called directly from Views or reusable components.

Preferred path:

```text
View -> ViewModel -> Application abstraction/use case -> Infrastructure Refit/HttpClient implementation
```

## 2. Contracts

- Use typed request and response records.
- Avoid `object` for real API methods.
- Keep DTOs in the owning layer according to current project patterns.
- Do not hardcode URLs, secrets, tokens, or connection strings.

## 3. Infrastructure

- Use Refit + HttpClientFactory.
- Register concrete clients/handlers in `FastBiteGroup.Desktop.Infrastructure/DependencyInjection.cs`.
- Use `JwtAuthHeaderHandler` for bearer token attachment.
- Store tokens through DPAPI-backed `ITokenStorage`.
- Handle `HttpRequestException`, timeouts, cancellation, and unauthorized responses.

## 4. ViewModel Behavior

- Call services asynchronously.
- Use cancellation tokens for long-running or repeatable actions.
- Disable or guard commands during loading.
- Surface user-friendly error messages.
- Never block the UI thread with `.Result` or `.Wait()`.

## 5. Verification

- Build the solution.
- Check service registration.
- Check app startup path still works when backend is unavailable if the feature allows offline startup.

