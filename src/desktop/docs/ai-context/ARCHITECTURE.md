# ARCHITECTURE.md - FastBite Desktop

## Architecture Direction

The desktop app follows a lightweight Clean Architecture style suitable for a WPF client.

```text
UI
  -> Application abstractions/use cases
  -> Infrastructure implementations
  -> Domain shared models
```

The UI project is the runtime composition root because WPF startup, resource dictionaries, Syncfusion visual behavior, and local settings are UI concerns.

## Layer Responsibilities

| Layer | Project | Responsibility |
|---|---|---|
| Domain | `FastBiteGroup.Desktop.Domain` | Shared result models and desktop domain exceptions |
| Application | `FastBiteGroup.Desktop.Application` | Use case contracts, app abstractions, feature orchestration |
| Infrastructure | `FastBiteGroup.Desktop.Infrastructure` | Refit API clients, auth header handler, token persistence |
| UI | `FastBiteGroup.Desktop.UI` | WPF views, resources, theme service, navigation service, host composition |

## Dependency Rules

- Domain must not reference Application, Infrastructure, or UI.
- Application may reference Domain only.
- Infrastructure may reference Application and Domain.
- UI may reference Application and Infrastructure.
- Views should not call HTTP clients directly.
- WPF-specific services stay in UI unless they are pure application abstractions.

## Service Registration

- Application services are registered in `FastBiteGroup.Desktop.Application/DependencyInjection.cs`.
- Infrastructure services are registered in `FastBiteGroup.Desktop.Infrastructure/DependencyInjection.cs`.
- UI-only services are registered in `App.xaml.cs`.

## API Client Direction

Use Refit interfaces in Application abstractions when the UI needs backend access. Infrastructure configures the concrete Refit clients.

Current placeholder:

```text
FastBiteGroup.Desktop.Application/Abstractions/IAuthClient.cs
```

Future auth flow should use typed request/response records instead of `object`.

## Theme Architecture

Theme is a UI concern.

- `IThemeService` and `ThemeService` live in `FastBiteGroup.Desktop.UI/Services`.
- App theme preference is `System`, `Light`, or `Dark`.
- Effective theme is `Light` or `Dark`.
- Theme resources are top-level merged dictionaries in `App.xaml`.
- Runtime UI uses `DynamicResource`.
- Syncfusion skinning is applied best-effort from the same theme service.

Do not rely on default WPF `MenuItem` popup styling for dark mode; use custom popup UI or explicit templates.
