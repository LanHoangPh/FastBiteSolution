# CURRENT_STATUS.md - FastBite Desktop

**Last Updated:** 2026-06-08

## Current Status

The desktop app builds and starts on .NET 8 WPF.

The current UI is a functional shell, not a complete product workflow. It includes startup hosting, logging, basic layout, and theme switching.

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
- Serilog writes rolling logs under `%AppData%\FastBite\logs`.

### Theme System

- Supported theme modes:
  - `System`
  - `Light`
  - `Dark`
- Theme preference is stored in `%AppData%\FastBite\settings.json`.
- `System` resolves from Windows `AppsUseLightTheme`.
- Runtime colors use top-level resource dictionaries:
  - `Resources/Themes/LightColors.xaml`
  - `Resources/Themes/DarkColors.xaml`
- Main shell uses `DynamicResource` for theme-aware brushes.
- Settings popup is custom themed WPF UI.

### Storage and API Preparation

- `TokenStorage` uses `ProtectedData` with `DataProtectionScope.CurrentUser`.
- `JwtAuthHeaderHandler` attaches bearer token if one exists.
- Refit is configured for `IAuthClient`.
- `IAuthClient` is still a placeholder.

## Verified Commands

```powershell
dotnet build FastBiteDesktop.slnx
dotnet run --project FastBiteGroup.Desktop.UI\FastBiteGroup.Desktop.UI.csproj --no-build
```

Last observed build result:

```text
Build succeeded.
0 Warning(s)
0 Error(s)
```

## Pending Work

- Add real MVVM structure for views and view models.
- Implement login flow with typed request/response models.
- Add auth token refresh/logout behavior.
- Add workspace selection shell.
- Add desktop test projects.
- Decide exact Syncfusion theme package/skin strategy as more Syncfusion controls are introduced.
- Add live OS theme change listener for `System` mode if needed.

## Known Notes

- The desktop app intentionally does not depend on backend source projects.
- Backend/root docs are not part of the normal desktop context.
- Default WPF `MenuItem` popup styling caused dark-mode color mismatch; use custom popup UI or explicit templates for themed menus.
