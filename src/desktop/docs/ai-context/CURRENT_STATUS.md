# CURRENT_STATUS.md - FastBite Desktop

**Last Updated:** 2026-06-08

## Current Status

The desktop app builds and starts on .NET 8 WPF.

The current UI is a fully-themed, premium dashboard shell displaying a custom layout, theme-switching capabilities, and reusable components. It uses host composition, logging, and Refit preparation.

---

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

### Theme System & Premium UI Design
- Supported theme modes: `System`, `Light`, `Dark` (stored in `%AppData%\FastBite\settings.json`).
- `System` resolves from Windows `AppsUseLightTheme` registry.
- Colors mapped from Next.js/Tailwind v4 theme variables to Hex codes.
- Added comprehensive brush sets (Sidebar, Charts, Inputs, Ring, Destructive, Popover) in `LightColors.xaml` and `DarkColors.xaml`.
- Custom design tokens created in `AppTheme.Spacing.xaml` and `AppTheme.Typography.xaml`.
- Implemented `DropShadowEffect` design tokens with `x:Shared="False"` attribute in `AppTheme.Effects.xaml` to prevent visual parent exceptions.
- Main shell utilizes a grid-based **Dashboard layout** containing a Sidebar, analytics cards, a configuration form, and a custom-styled `DataGrid` (System Access Logs).
- Control styles are fully switchable at runtime using `DynamicResource`.

### Reusable UI Components
- **ToggleSwitch:** Styled CheckBox (`ToggleSwitchStyle`) providing smooth iOS/Web-like slide transition animation. Integrated directly in the header bar as a fast theme-switcher.
- **ModernButton:** Custom control (`ModernButton` inheriting from `Button`) supporting:
  - Variants: `Primary`, `Secondary`, `Outline`, `Ghost`, `Destructive` (shadcn style).
  - Icons: customizable left or right icon placements.
  - CornerRadius: custom border radius binding.

### Storage and API Preparation
- `TokenStorage` uses `ProtectedData` with `DataProtectionScope.CurrentUser`.
- `JwtAuthHeaderHandler` attaches bearer token if one exists.
- Refit is configured for `IAuthClient`.
- `IAuthClient` is still a placeholder.

---

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

---

## Pending Work

- Add real MVVM structure for views and view models (binding handlers to Commands instead of code-behind).
- Implement login flow with typed request/response models.
- Add auth token refresh/logout behavior.
- Add workspace selection shell.
- Add desktop test projects.
- Decide exact Syncfusion theme package/skin strategy as more Syncfusion controls are introduced.
- Add live OS theme change listener for `System` mode if needed.

---

## Known Notes

- The desktop app intentionally does not depend on backend source projects.
- Backend/root docs are not part of the normal desktop context.
- Default WPF `MenuItem` popup styling caused dark-mode color mismatch; use custom popup UI or explicit templates for themed menus.
- Use `DynamicResource` for all spacing and effect tokens referenced in `AppTheme.Controls.xaml` to prevent parse time errors.
- Visual Effects must be configured with `x:Shared="False"` inside styles to allow multiple element bindings.
