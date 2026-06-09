---
name: gpt-taste
description: FastBite WPF Desktop taste guard. Use for modernizing or building WPF desktop UI in this repository. Enforces MVVM, theme tokens, reusable components, and productivity-app UX. Do not generate React, Tailwind, HTML, CSS, or GSAP for this project.
---

# FastBite WPF Desktop Taste Guard

This repository is a **WPF desktop application**, not a web landing page. Any agent that loads this skill must produce native WPF/.NET output aligned with `AGENTS.md` and the desktop context files.

## Mandatory Context

Before proposing or editing code, read these files in order:

1. `docs/ai-context/DESKTOP_CONTEXT.md`
2. `docs/ai-context/ARCHITECTURE.md`
3. `docs/ai-context/UI_GUIDELINES.md`
4. `docs/ai-context/CURRENT_STATUS.md`

Only read backend or repository-root context when the user explicitly asks for backend or full-solution work.

## Product Direction

FastBite Desktop should feel like a focused communication/workspace tool:

- Quiet, utilitarian, and fast to scan.
- Dense enough for repeated daily work.
- Clear hierarchy without marketing-page composition.
- Native desktop behavior over web-style spectacle.
- Professional Light, Dark, and System theme support.

Do not create hero sections, bento landing pages, scroll-driven storytelling, GSAP animations, Tailwind classes, React components, or web routing.

## Architecture Rules

- Keep edits scoped to `src/desktop`.
- `FastBiteGroup.Desktop.Domain` has no dependencies on other desktop layers.
- `FastBiteGroup.Desktop.Application` depends on Domain only.
- `FastBiteGroup.Desktop.Infrastructure` depends on Application + Domain.
- `FastBiteGroup.Desktop.UI` depends on Application + Infrastructure.
- UI concerns stay in UI: XAML resources, windows, ViewModels, theme switching, Syncfusion skinning, visual components.
- Views and reusable components must not call Refit/API clients directly.
- Real feature workflows should route through ViewModels and Application use cases.

## MVVM Rules

- Use `CommunityToolkit.Mvvm`.
- ViewModels inherit from `ObservableObject` or `ObservableValidator`.
- Use `[ObservableProperty]` and `[RelayCommand]`.
- Async commands return `Task` and expose loading/error/empty state where relevant.
- Keep code-behind thin: `InitializeComponent`, DataContext/service wiring, and WPF interop only.
- Do not instantiate windows, controls, `HttpClient`, database contexts, or API clients from ViewModels.

## Theme Rules

- Every UI change must work in Light and Dark mode.
- Use `DynamicResource` for theme-aware brushes, spacing, radius, effects, dimensions, and cross-dictionary resources.
- Raw hex colors belong only in:
  - `FastBiteGroup.Desktop.UI/Resources/Themes/LightColors.xaml`
  - `FastBiteGroup.Desktop.UI/Resources/Themes/DarkColors.xaml`
- When adding a semantic brush, add it to both theme dictionaries.
- Do not use `Foreground="White"`, `Background="#..."`, or default WPF popup/menu styling.
- Popups, dropdowns, menus, context menus, and flyouts need explicit app-token styling.

## Component Rules

- Reusable components live in `FastBiteGroup.Desktop.UI/Views/Components`.
- Default component styles/templates live in `FastBiteGroup.Desktop.UI/Resources/AppTheme.Controls.xaml`.
- Public visual/configuration properties must be `DependencyProperty` values.
- Visual states belong in XAML templates/triggers, not C# event code.
- Prefer WPF-native controls for shell, buttons, badges, avatars, search, and empty states.
- Use Syncfusion where it provides real value: complex grids, rich editors, chat-style controls, and advanced enterprise widgets.

## UX Rules

- Desktop is not web. Use sidebars, top bars, panes, tabs, toolbars, context menus, keyboard focus, and clear active states.
- Favor dense but readable screens over decorative whitespace.
- Use `Grid` and `DockPanel` for structural layout; avoid deep nested `StackPanel` trees.
- Avoid fixed widths/heights unless the element is truly fixed-format.
- Every interactive state needs a clear hover, pressed, disabled, and focus state.
- Use reusable components already present before inventing new ones: `ModernButton`, `IconButton`, `Avatar`, `StatusBadge`, `SearchBox`, `EmptyState`, and `SectionHeader`.

## Verification

After code changes:

- Run `dotnet build FastBiteDesktop.slnx`.
- If UI changed, verify Light and Dark mode when feasible.
- Scan for hardcoded colors outside theme dictionaries.
- Confirm no backend/root files were modified unless explicitly requested.

