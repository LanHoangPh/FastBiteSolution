# AGENTS.md - FastBite Desktop Project Memory

> Local instructions for `src/desktop` only. Do not use or edit the repository-level project memory unless the user explicitly asks for backend or full-solution work.

---

## Project Summary

**FastBiteGroup Desktop** is a WPF client shell for FastBiteGroup. It targets **.NET 8 Windows** for stable WPF and third-party UI support.

- UI: WPF
- Runtime: .NET 8
- UI library: Syncfusion WPF
- MVVM helpers: CommunityToolkit.Mvvm
- HTTP client direction: Refit + HttpClientFactory
- DI/Hosting: Microsoft.Extensions.Hosting
- Logging: Serilog
- Local secure token storage: Windows DPAPI via `ProtectedData`
- Theme modes: System, Light, Dark

---

## Before Making Changes

Always read local desktop context first, in this order:

1. `docs/ai-context/DESKTOP_CONTEXT.md`
2. `docs/ai-context/ARCHITECTURE.md`
3. `docs/ai-context/UI_GUIDELINES.md`
4. `docs/ai-context/CURRENT_STATUS.md`

Only read files outside `src/desktop` when the user explicitly asks for backend/full-solution context.

---

## Build Commands

```powershell
dotnet restore FastBiteDesktop.slnx
dotnet build FastBiteDesktop.slnx
dotnet run --project FastBiteGroup.Desktop.UI\FastBiteGroup.Desktop.UI.csproj
```

---

## Layer Rules

- `FastBiteGroup.Desktop.Domain` has no dependencies on other desktop layers.
- `FastBiteGroup.Desktop.Application` depends on Domain only.
- `FastBiteGroup.Desktop.Infrastructure` depends on Application + Domain.
- `FastBiteGroup.Desktop.UI` depends on Application + Infrastructure.
- UI-only concerns, such as WPF resources, window behavior, theme switching, and Syncfusion visual skinning, stay in the UI layer.
- Do not call Refit/API clients directly from views. Route user actions through view models/use cases when features become real.

---

## Desktop Coding Rules

- Keep target framework at `.NET 8` unless the user explicitly asks to change it.
- Keep WPF projects on `net8.0-windows`.
- Use `DynamicResource` for theme-aware brushes.
- Store user preferences under `%AppData%\FastBite`, not in source-controlled `appsettings.json`.
- Keep `appsettings.json` for app/environment config only.
- Do not hardcode API secrets, tokens, connection strings, or license keys.
- Register services in the owning layer's `DependencyInjection.cs` or UI host composition.
- Prefer Syncfusion controls where the project already uses them, but keep app-level design tokens independent from Syncfusion skins.

---

## Current Feature Notes

- Theme switching is implemented in the UI layer with `IThemeService`.
- Supported theme preferences: `System`, `Light`, `Dark`.
- Theme preference is saved in `%AppData%\FastBite\settings.json`.
- Runtime colors live in `FastBiteGroup.Desktop.UI/Resources/Themes/`.
- The Settings popup is custom WPF UI, not the default WPF `MenuItem`, because default menu popup styling did not respect dark theme tokens.

---

## Never

- Do not modify backend projects or root-level docs unless explicitly requested.
- Do not upgrade/downgrade packages without explaining why.
- Do not replace the desktop architecture with a web stack or microservice concept.
- Do not remove `.NET 8` compatibility without explicit approval.
- Do not store user tokens/settings in source-controlled files.

---

## Always

- Keep edits scoped to `src/desktop` for desktop requests.
- Build `FastBiteDesktop.slnx` after changes.
- If changing UI, verify the app starts.
- Explain trade-offs briefly when choosing between WPF-native and Syncfusion approaches.
