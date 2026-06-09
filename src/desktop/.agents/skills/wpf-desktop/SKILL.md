---
name: wpf-desktop
description: Primary FastBite desktop skill for Codex, Antigravity, and other AI agents. Use for WPF UI, MVVM, theme resources, reusable components, navigation shell, API integration, and desktop refactoring in src/desktop.
---

# FastBite WPF Desktop Skill

This is the primary skill for work under `D:\CodeVs\FastBiteSolution\src\desktop`.

## Read First

Before making changes, read:

1. `AGENTS.md`
2. `docs/ai-context/DESKTOP_CONTEXT.md`
3. `docs/ai-context/ARCHITECTURE.md`
4. `docs/ai-context/UI_GUIDELINES.md`
5. `docs/ai-context/CURRENT_STATUS.md`

Do not use backend or root-level project memory unless explicitly requested.

## Workflow Playbooks

Use the workflow playbooks in `.agents/workflows` when a task matches them:

- `desktop-change.md` for normal desktop edits.
- `ui-theme-change.md` for XAML, resources, components, and theme work.
- `mvvm-feature.md` for screens, commands, navigation, and feature behavior.
- `api-integration.md` for backend API consumption.
- `review-verification.md` before final handoff.

## What This Project Is

FastBite Desktop is a native WPF Windows client shell for FastBiteGroup.

- Runtime: .NET 8
- UI: WPF
- UI library: Syncfusion WPF
- MVVM: CommunityToolkit.Mvvm
- DI/Host: Microsoft.Extensions.Hosting
- HTTP: Refit + HttpClientFactory
- Logging: Serilog
- Secure local storage: Windows DPAPI
- Theme modes: System, Light, Dark

## What This Project Is Not

Do not generate:

- React, Vue, Angular, Svelte, HTML, or CSS
- Tailwind classes
- GSAP, scroll-triggered landing pages, or AIDA marketing sections
- Web routing patterns
- Microservice architecture
- Backend changes unless explicitly requested

## Required Thinking

Before implementation, think from four perspectives:

- Business Analyst: what user problem is being solved?
- Product Manager: does this move the desktop product forward?
- Product Owner: is the requested scope clear and valuable now?
- Solution Architect: does the change preserve WPF MVVM and layer boundaries?

Prefer the simplest architecture that fits the current product stage. The app is still a shell/prototype; do not overbuild production workflows before real requirements exist.

## Layer Boundaries

- Domain: shared models and desktop domain exceptions only.
- Application: abstractions and use cases; depends on Domain only.
- Infrastructure: API clients, auth handler, secure storage; depends on Application + Domain.
- UI: WPF views, ViewModels, resources, services, components, theme, navigation, composition root.

Service registration:

- Application services: `FastBiteGroup.Desktop.Application/DependencyInjection.cs`
- Infrastructure services: `FastBiteGroup.Desktop.Infrastructure/DependencyInjection.cs`
- UI services, ViewModels, windows: `FastBiteGroup.Desktop.UI/App.xaml.cs`

## MVVM Requirements

- Use `ObservableObject`, `ObservableValidator`, `[ObservableProperty]`, and `[RelayCommand]`.
- Commands represent user intent, not raw UI events.
- Async work uses `async Task` commands and exposes loading/error state.
- Keep code-behind limited to WPF-specific wiring.
- ViewModels must not reference WPF controls or instantiate views.
- Views/components must not call Refit/API clients directly.

## Theme Requirements

- Use `DynamicResource` for theme-aware resources.
- Raw hex colors are allowed only in:
  - `FastBiteGroup.Desktop.UI/Resources/Themes/LightColors.xaml`
  - `FastBiteGroup.Desktop.UI/Resources/Themes/DarkColors.xaml`
- Add semantic brushes to both Light and Dark dictionaries.
- Store typography, spacing, radius, dimensions, and effects in existing resource dictionaries.
- Do not hardcode `Foreground="White"` or `Background="#..."`.
- Popups, menus, dropdowns, and flyouts need explicit app-token styling.

## UI Taste

FastBite Desktop should feel:

- Work-focused
- Calm
- Dense but readable
- Fast to scan
- Native to Windows
- Professional in both Light and Dark mode

Use sidebars, top bars, panes, tabs, commands, context menus, focus states, and clear active states. Avoid decorative landing-page patterns.

## Component Guidance

Existing reusable components:

- `ModernButton`
- `IconButton`
- `Avatar`
- `StatusBadge`
- `SearchBox`
- `EmptyState`
- `SectionHeader`

Before creating a new component, check whether one of these can be extended.

Reusable components:

- Live under `FastBiteGroup.Desktop.UI/Views/Components`
- Expose configuration through `DependencyProperty`
- Use theme tokens through `DynamicResource`
- Keep default styles/templates in `Resources/AppTheme.Controls.xaml`
- Avoid backend/Application dependencies

## API Integration

For real backend workflows:

- Put client/service abstractions in Application.
- Put Refit/HttpClient implementations in Infrastructure.
- Use HttpClientFactory and configured handlers.
- Store tokens through DPAPI-backed storage, not appsettings.
- Use cancellation tokens for long-running calls.
- Show user-friendly offline/error states.

## Verification Checklist

After changes:

- Run `dotnet build FastBiteDesktop.slnx`.
- If UI changed, verify Light and Dark mode when feasible.
- Scan for hardcoded colors outside theme dictionaries.
- Confirm ViewModels remain UI-testable.
- Confirm no backend/root files changed unless requested.
