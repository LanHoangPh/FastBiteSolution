# CURRENT_STATUS.md - FastBite Desktop

**Last Updated:** 2026-06-11

## Current Status

The desktop app is a .NET 8 WPF shell with host composition, theme/language switching, MVVM shell state, auth API wiring, and a reusable component system.

The current UI is still a shell/prototype for future product workflows. Login/register/forgot-password are wired to typed auth API abstractions, but workspace selection, chat, and backend-backed dashboard data are not implemented yet.

## Completed

### Runtime and Project Setup

- Desktop projects target .NET 8.
- WPF UI project targets `net8.0-windows`.
- Infrastructure project targets `net8.0-windows` because it uses Windows DPAPI.
- `appsettings.json` is copied to output.
- App configuration uses `AppContext.BaseDirectory` for reliable runtime loading.

### Host and Services

- `App.xaml.cs` creates a .NET host.
- Application and Infrastructure services are registered.
- UI services registered:
  - `NavigationService`
  - `ThemeService`
  - `LanguageService`
  - `DialogService`
- UI ViewModels and `MainWindow` are registered in UI host composition.
- `MainWindow` and `MainWindowViewModel` are transient so logout/login creates a fresh session window.
- Serilog writes rolling logs under `%AppData%\FastBite\logs`.
- Startup and shutdown paths log failures and show user-friendly error messages.

### MVVM Shell

- `MainWindowViewModel` owns shell state.
- Theme actions use ViewModel commands.
- Settings popup state is bound through the ViewModel.
- Dashboard metrics and access logs are bound from ViewModel collections.
- Sidebar item state is represented through `SidebarItemViewModel`.
- `MainWindow.xaml.cs` is thin and limited to WPF wiring plus Syncfusion theme re-application.
- Auth ViewModels use Application use cases/auth abstractions and `IDialogService`; they do not call `MessageBox.Show` directly.

### Theme System

- Supported theme modes: `System`, `Light`, `Dark`.
- Theme preference is stored in `%AppData%\FastBite\settings.json`.
- `System` resolves from Windows `AppsUseLightTheme` registry state.
- Light and Dark theme dictionaries live under `Resources/Themes/`.
- Core semantic brushes, sidebar brushes, chart brushes, status brushes, and presence brushes exist in both Light and Dark dictionaries.
- Component dimensions such as icon sizes and icon button sizes live in `AppTheme.Spacing.xaml`.
- Drop shadow effects live in `AppTheme.Effects.xaml` and use `x:Shared="False"`.
- Runtime UI uses `DynamicResource` for theme-aware values.

### Reusable UI Components

Current component set:

- `ModernButton`: variant button with icon placement, size/loading support, and custom corner radius.
- `IconButton`: compact icon-only button with variant and size support.
- `StatusBadge`: semantic status/presence indicator.
- `Avatar`: initials/image avatar with optional presence status.
- `SearchBox`: search input with placeholder and clear behavior.
- `EmptyState`: reusable empty content surface.
- `SectionHeader`: reusable title/subtitle/action header.
- `ToggleSwitchStyle`: styled CheckBox switch template.

Component styles/templates are currently kept as implicit styles in `Resources/AppTheme.Controls.xaml`.

### Storage and API Preparation

- `TokenStorage` uses `ProtectedData` with `DataProtectionScope.CurrentUser`.
- `DpapiSecureTokenStore` stores refresh tokens under `%AppData%\FastBite\refresh_token.dat`.
- `TokenProvider` holds runtime access token state.
- `AuthHeaderHandler` attaches bearer tokens from `ITokenProvider`.
- Refit is configured for `IAuthClient` and `IUserClient`.
- `IAuthClient` has typed login/register/refresh/logout/forgot-password/reset-password methods.
- `LoginUseCase` coordinates login token persistence; `RegisterUseCase` wraps registration.
- Auto-login restores persisted tokens and parses JWT expiry when possible instead of treating restored access tokens as indefinitely valid.

### Recent Cleanup

- Removed duplicate DPAPI-reading request handler (`JwtAuthHeaderHandler`).
- Replaced auth ViewModel `MessageBox.Show` calls with `IDialogService`.
- Replaced repeated hardcoded dashboard badge/icon colors with theme-aware brush tokens.
- `BooleanToBadgeStatusConverter.ConvertBack` now returns `DependencyProperty.UnsetValue` instead of throwing.

## Verified Commands

Previously verified:

```powershell
dotnet build FastBiteDesktop.slnx
dotnet run --project FastBiteGroup.Desktop.UI\FastBiteGroup.Desktop.UI.csproj --no-build
```

Last known clean build:

```text
Build succeeded.
0 Warning(s)
0 Error(s)
```

Most recent verification:

```powershell
dotnet build FastBiteDesktop.slnx
```

```text
Build succeeded.
0 Warning(s)
0 Error(s)
```

Known build caveat:

- If `FastBiteGroup.Desktop.UI` is running, build may fail with DLL copy errors because the running app locks output assemblies. Close the app process and build again.

## Pending Work

- Add a visible remember-me option if product requirements need non-persistent sessions.
- Implement workspace selection shell.
- Implement chat shell and message surfaces.
- Replace placeholder dashboard/access-log data with real feature data or remove it from production views.
- Add desktop test projects.
- Add manual or automated auth flow verification against the backend: login, logout, auto-login, refresh, expired token, and invalid credentials.
- Add visual QA checklist execution for Light/Dark/System mode.
- Add live OS theme change listener for `System` mode if needed.
- Decide Syncfusion theme package/skin strategy as more Syncfusion controls are introduced.
- Optional future cleanup: move custom-control default styles to `Themes/Generic.xaml` if components are extracted into a standalone control library.

## Known Notes

- The desktop app intentionally does not depend on backend source projects.
- Backend/root docs are not part of normal desktop context.
- Default WPF popup/menu styling can break dark mode; use custom popup UI or explicit templates.
- Use `DynamicResource` for all theme-aware brushes, spacing, radius, dimensions, and effects.
- Raw hex colors belong only in `LightColors.xaml` and `DarkColors.xaml`.
- When adding a semantic brush, add it to both Light and Dark dictionaries.
- `Resources/AppTheme.Controls.xaml` is the current home for implicit custom-control styles/templates.
- Visual effects must use `x:Shared="False"` when stored as resources.
