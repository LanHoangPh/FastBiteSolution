# ARCHITECTURE.md - FastBite Desktop

## Architecture Direction

The desktop app follows a lightweight Clean Architecture style suitable for a WPF client.

```text
UI
  -> Application abstractions/use cases
  -> Infrastructure implementations
  -> Domain shared models
```

The UI project is the runtime composition root because WPF startup, resource dictionaries, Syncfusion visual behavior, local settings, and theme resources are UI concerns.

## Layer Responsibilities

| Layer | Project | Responsibility |
|---|---|---|
| Domain | `FastBiteGroup.Desktop.Domain` | Shared result models and desktop domain exceptions |
| Application | `FastBiteGroup.Desktop.Application` | Use case contracts, app abstractions, feature orchestration |
| Infrastructure | `FastBiteGroup.Desktop.Infrastructure` | Refit API clients, auth header handler, token persistence |
| UI | `FastBiteGroup.Desktop.UI` | WPF views, ViewModels, resources, reusable components, theme/language/dialog services, navigation service, host composition |

## Dependency Rules

- Domain must not reference Application, Infrastructure, or UI.
- Application may reference Domain only.
- Infrastructure may reference Application and Domain.
- UI may reference Application and Infrastructure.
- Views and components must not call HTTP/API clients directly.
- WPF-specific services stay in UI unless they are pure application abstractions.
- Reusable UI components belong to the UI layer and must not depend on Application, Infrastructure, or backend APIs.

## MVVM Rules

- Views contain layout and bindings.
- ViewModels own display state, observable properties, and commands.
- Use `CommunityToolkit.Mvvm` for `ObservableObject`, `[ObservableProperty]`, and `[RelayCommand]`.
- Button actions should use `Command` bindings instead of `Click` handlers.
- Code-behind should stay thin and only handle WPF wiring such as `InitializeComponent`, `DataContext`, window-specific service calls, and interop that needs the view instance.
- Placeholder UI data may live in UI ViewModels until real Application use cases exist.
- When a feature becomes real, move feature orchestration into Application use cases and keep the ViewModel as a UI adapter.
- ViewModels should expose intent/state and call UI services for desktop-only concerns such as dialogs; they should not call `MessageBox.Show` directly.

## Service Registration

- Application services are registered in `FastBiteGroup.Desktop.Application/DependencyInjection.cs`.
- Infrastructure services are registered in `FastBiteGroup.Desktop.Infrastructure/DependencyInjection.cs`.
- UI-only services, ViewModels, and WPF windows are registered in `App.xaml.cs`.

## API Client Direction

Use Refit interfaces in Application abstractions when the UI needs backend access. Infrastructure configures concrete Refit clients.

Current auth/client surface:

```text
FastBiteGroup.Desktop.Application/Abstractions/IAuthClient.cs
FastBiteGroup.Desktop.Application/Abstractions/IUserClient.cs
FastBiteGroup.Desktop.Application/UseCases/Auth/LoginUseCase.cs
FastBiteGroup.Desktop.Application/UseCases/Auth/RegisterUseCase.cs
```

- Auth request/response models are typed records under `Application/Models/Auth`.
- Login persistence is coordinated by `LoginUseCase`; `AuthService` updates runtime auth state and handles API calls.
- Infrastructure uses `AuthHeaderHandler` with `ITokenProvider` for bearer headers. Token storage is used for persistence/restore, not per-request file reads.
- Access token expiry should come from backend response or JWT expiry parsing; do not treat restored tokens as indefinitely valid.

## Theme Architecture

Theme is a UI concern.

- `IThemeService` and `ThemeService` live in `FastBiteGroup.Desktop.UI/Services`.
- App theme preference is `System`, `Light`, or `Dark`.
- Effective theme is `Light` or `Dark`.
- `System` mode resolves through Windows app theme state.
- Theme resources are top-level merged dictionaries in `App.xaml`.
- Runtime UI uses `DynamicResource`.
- Syncfusion skinning is applied best-effort from the same theme service.
- App design tokens must remain independent from Syncfusion skins.

Do not rely on default WPF `MenuItem`, popup, or dropdown styling for dark mode. Use custom popup UI or explicit templates.

## Component Architecture

- Reusable controls live in `FastBiteGroup.Desktop.UI/Views/Components`.
- Component styles/templates live in `Resources/AppTheme.Controls.xaml`.
- Current custom controls use implicit styles in the app resource dictionary, not `Themes/Generic.xaml`.
- If components are later extracted into a separate control library, move default custom-control styles to `Themes/Generic.xaml`.
- Custom controls expose public UI configuration via `DependencyProperty`.
- Visual states should be implemented in XAML `ControlTemplate.Triggers`.
- Components should use `DynamicResource` for theme-aware values and must render correctly in Light and Dark mode.
