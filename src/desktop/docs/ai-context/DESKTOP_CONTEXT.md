# DESKTOP_CONTEXT.md - FastBite Desktop

## Purpose

FastBiteGroup Desktop is the Windows desktop client for FastBiteGroup. It is currently a WPF shell prepared for authentication, workspace navigation, chat UI, and later communication features.

This context applies only to:

```text
D:\CodeVs\FastBiteSolution\src\desktop
```

Do not infer backend work from this file. Backend/full-solution work must be requested explicitly.

## Technology Stack

| Area | Choice |
|---|---|
| Runtime | .NET 8 |
| UI | WPF |
| UI library | Syncfusion WPF |
| MVVM helpers | CommunityToolkit.Mvvm |
| Dependency injection | Microsoft.Extensions.Hosting |
| HTTP client | Refit + HttpClientFactory |
| Resilience | Microsoft.Extensions.Http.Polly |
| Logging | Serilog |
| Secure token storage | Windows DPAPI |

## Solution Structure

```text
FastBiteDesktop.slnx

FastBiteGroup.Desktop.Domain/
  Exceptions/
  Models/Shared/

FastBiteGroup.Desktop.Application/
  Abstractions/
  UseCases/
  DependencyInjection.cs

FastBiteGroup.Desktop.Infrastructure/
  Authentication/
  ApiClients/
  Http/
  Storage/
  DependencyInjection.cs

FastBiteGroup.Desktop.UI/
  Resources/
    AppTheme.Controls.xaml
    AppTheme.Effects.xaml
    AppTheme.Spacing.xaml
    AppTheme.Typography.xaml
    MainTheme.xaml
    Themes/
      LightColors.xaml
      DarkColors.xaml
  Services/
  ViewModels/
  Views/
    Components/
  App.xaml
  App.xaml.cs
  MainWindow.xaml
  MainWindow.xaml.cs
```

## Current App Shape

- The UI project is the composition root.
- `App.xaml.cs` builds the host, registers desktop services and ViewModels, starts the WPF shell, and initializes theme/language state.
- `MainWindow` is a dashboard shell with sidebar navigation, settings popup, theme selection, metrics, access logs, and reusable component usage.
- `MainWindowViewModel` owns shell display state, theme commands, sidebar state, dashboard metrics, and access log placeholder data.
- Login, registration, and forgot-password screens are wired to typed auth API client methods through Application use cases or auth abstractions.
- The app has reusable WPF components under `Views/Components`.
- The app does not yet have real workspace selection, chat workflows, or backend-backed dashboard data.
- Dashboard metrics and access logs are placeholder UI data until real workflows are wired.
- `IAuthClient` and `IUserClient` are typed Refit abstractions; Infrastructure configures concrete clients and auth headers.

## Local User Data

Runtime user data belongs under:

```text
%AppData%\FastBite
```

Current usage:

- `auth.dat`: encrypted token storage via DPAPI.
- `refresh_token.dat`: encrypted refresh token storage via DPAPI.
- `settings.json`: user preferences such as theme mode.
- `language.json`: language preference.
- `logs/log-.txt`: Serilog rolling logs.

Do not store user preferences, tokens, or secrets in source-controlled `appsettings.json`.

## Documentation Scope

- `AGENTS.md` is the quick instruction file for AI agents.
- `docs/ai-context/ARCHITECTURE.md` explains desktop layering and MVVM direction.
- `docs/ai-context/UI_GUIDELINES.md` is the source of truth for WPF theme/component rules.
- `docs/ai-context/CURRENT_STATUS.md` tracks implemented and pending desktop work.
