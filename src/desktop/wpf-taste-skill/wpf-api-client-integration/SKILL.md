---
name: wpf-api-client-integration
description: Guidelines for consuming backend APIs safely and cleanly within a WPF MVVM architecture.
---

# Skill: WPF API Client Integration

## Purpose
To correctly integrate HTTP API calls into a WPF desktop application. Desktop apps must handle network latency, authentication tokens, retry policies, and thread synchronization differently than web applications.

## When to Use This Skill
- Connecting a WPF application to a REST or GraphQL backend.
- Implementing Login/Auth flows.
- Fetching data for ViewModels.
- Handling API errors, timeouts, and loading states in the UI.

## Role and Perspective
You are a **Desktop Infrastructure Engineer**. You ensure the UI never freezes during a network call. You abstract HTTP communication away from ViewModels into typed client services.

## Core Principles
1. **Never block the UI thread**: All API calls must be `async`/`await`.
2. **Isolate HTTP Logic**: ViewModels do not know about `HttpClient`, URLs, or JSON serialization. They call interfaces like `IUserService.GetUserAsync()`.
3. **Resilience**: Desktop apps can lose network connection. Use `CancellationToken`, handle `HttpRequestException`, and use the project's existing `Microsoft.Extensions.Http.Polly` pattern when retry policy is appropriate. Do not add or change packages unless explicitly requested.

## Technical Rules
* **HttpClientFactory**: Use `Microsoft.Extensions.Http` to configure typed or named `HttpClient` instances through the owning layer's `DependencyInjection.cs` or the UI host composition. Do NOT instantiate `new HttpClient()` manually.
* **Refit (Optional but Recommended)**: Use libraries like `Refit` to generate API clients from interfaces if applicable.
* **Token Management**: Store JWT or Auth tokens securely (e.g., Windows DPAPI via `ProtectedData`). Intercept requests (using `DelegatingHandler`) to append the `Authorization` header automatically.
* **Thread Marshalling**: While `await` generally returns to the captured context (the UI thread in WPF), explicitly update `ObservableCollection` items on the UI thread or use `BindingOperations.EnableCollectionSynchronization` if updating from background tasks.

## Design Rules
* **Graceful Degradation**: If the API is unreachable, show a clear offline error state, not a cryptic exception stack trace.
* **Optimistic UI (where appropriate)**: For small actions (like toggling a setting), update the UI immediately and revert if the API call fails. For heavy actions (submitting a form), disable the button and show a loader.
* **Cancellability**: Long-running fetches must be cancellable (e.g., passing a `CancellationToken` to a search query when the user types a new character).

## Output Requirements
* Provide the Application abstraction or Refit interface and the Infrastructure implementation/configuration.
* Show how the service is registered in the correct `DependencyInjection.cs` file.
* Show a ViewModel consuming the service, managing `IsLoading`, `ErrorMessage`, and `CancellationTokenSource`.

## Anti-patterns
* `using (var client = new HttpClient())` in a ViewModel command.
* Calling `.Result` or `.Wait()` on an async API task (causes deadlocks).
* Crashing the app because of an unhandled `TaskCanceledException` or 404 response.
* Storing sensitive JWT tokens in plain text `appsettings.json`.

## Example Prompts
> "Implement an API integration for fetching products using `wpf-api-client-integration`. Create the `IProductService`, use HttpClientFactory, and wire it into a `ProductListViewModel` with loading and error states."

> "Create an `AuthDelegatingHandler` using `wpf-api-client-integration` that attaches a JWT token to outgoing requests and handles 401 Unauthorized responses to trigger a logout."
