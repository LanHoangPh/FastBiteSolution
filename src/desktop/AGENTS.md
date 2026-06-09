# AGENTS.md - FastBite Desktop Project Memory

> Local instructions for `src/desktop` only. Do not use or edit repository-level project memory unless the user explicitly asks for backend or full-solution work.

---

## Project Summary

**FastBiteGroup Desktop** is a WPF Windows client shell for FastBiteGroup. It targets **.NET 8 Windows** for stable WPF and third-party UI support.

- UI: WPF
- Runtime: .NET 8
- UI library: Syncfusion WPF
- MVVM helpers: CommunityToolkit.Mvvm
- HTTP client direction: Refit + HttpClientFactory
- DI/Hosting: Microsoft.Extensions.Hosting
- Logging: Serilog
- Local secure token storage: Windows DPAPI via `ProtectedData`
- Theme modes: `System`, `Light`, `Dark`

---

## Before Making Changes

Always read local desktop context first, in this order:

1. `docs/ai-context/DESKTOP_CONTEXT.md`
2. `docs/ai-context/ARCHITECTURE.md`
3. `docs/ai-context/UI_GUIDELINES.md`
4. `docs/ai-context/CURRENT_STATUS.md`

Only read files outside `src/desktop` when the user explicitly asks for backend or full-solution context.

---

## Build Commands

```powershell
dotnet restore FastBiteDesktop.slnx
dotnet build FastBiteDesktop.slnx
dotnet run --project FastBiteGroup.Desktop.UI\FastBiteGroup.Desktop.UI.csproj
```

If build fails because DLLs are locked by `FastBiteGroup.Desktop.UI`, close the running app process and build again.

---

## Layer Rules

- `FastBiteGroup.Desktop.Domain` has no dependencies on other desktop layers.
- `FastBiteGroup.Desktop.Application` depends on Domain only.
- `FastBiteGroup.Desktop.Infrastructure` depends on Application + Domain.
- `FastBiteGroup.Desktop.UI` depends on Application + Infrastructure.
- UI-only concerns such as WPF resources, window behavior, theme switching, visual components, and Syncfusion skinning stay in the UI layer.
- Views and components must not call Refit/API clients directly. Route feature actions through ViewModels and Application use cases when features become real.

---

## Desktop Coding Rules

- Keep target framework at `.NET 8` unless the user explicitly asks to change it.
- Keep WPF projects on `net8.0-windows`.
- Use MVVM for UI state and actions: ViewModels use `CommunityToolkit.Mvvm`, properties use `ObservableObject`, and actions use `[RelayCommand]`.
- Keep code-behind thin. It may do `InitializeComponent`, `DataContext`, window/service wiring, and WPF interop that truly needs a view instance.
- Do not hardcode colors, spacing, radius, or effects in views or component templates.
- Every UI change must work in both Light and Dark mode.
- Use `DynamicResource` for theme-aware brushes, spacing, radius, effects, and cross-dictionary resources.
- Add raw color hex values only in `Resources/Themes/LightColors.xaml` and `Resources/Themes/DarkColors.xaml`.
- When adding a semantic brush, add it to both Light and Dark dictionaries.
- Store user preferences under `%AppData%\FastBite`, not in source-controlled `appsettings.json`.
- Keep `appsettings.json` for app/environment config only.
- Do not hardcode API secrets, tokens, connection strings, or license keys.
- Register services in the owning layer's `DependencyInjection.cs` or UI host composition.
- Prefer Syncfusion controls where they provide real value, but keep app-level design tokens independent from Syncfusion skins.

---

## Component Rules

- Reusable WPF components live in `FastBiteGroup.Desktop.UI/Views/Components`.
- Custom controls inherit from the closest native WPF base control.
- Public visual/configuration properties must be `DependencyProperty` values.
- Visual states belong in XAML `ControlTemplate.Triggers`, not C# code-behind.
- Default component styles/templates are kept in `Resources/AppTheme.Controls.xaml`.
- Current reusable components include `ModernButton`, `IconButton`, `Avatar`, `StatusBadge`, `SearchBox`, `EmptyState`, and `SectionHeader`.

---

## Current Feature Notes

- Theme switching is implemented in the UI layer with `IThemeService`.
- Supported theme preferences are `System`, `Light`, and `Dark`.
- Theme preference is saved in `%AppData%\FastBite\settings.json`.
- Runtime colors live in `FastBiteGroup.Desktop.UI/Resources/Themes/`.
- The current shell uses `MainWindowViewModel` for dashboard metrics, access logs, theme state, settings popup state, and sidebar state.
- Dashboard metrics and access logs are placeholder UI data until real backend workflows are implemented.
- Default WPF `MenuItem` popup styling is not reliable for dark mode; use custom popup UI or explicit templates.

---

## Never

- Do not modify backend projects or root-level docs unless explicitly requested.
- Do not upgrade/downgrade packages without explaining why.
- Do not replace the desktop architecture with a web stack or microservice concept.
- Do not remove `.NET 8` compatibility without explicit approval.
- Do not store user tokens/settings in source-controlled files.
- Do not use `Foreground="White"` or `Background="#..."` in views/component styles; use theme tokens.

---

## Always

- Keep edits scoped to `src/desktop` for desktop requests.
- Build `FastBiteDesktop.slnx` after changes.
- If changing UI, verify the app starts when feasible.
- Check Light and Dark mode for any UI/component change.
- Explain trade-offs briefly when choosing between WPF-native and Syncfusion approaches.
