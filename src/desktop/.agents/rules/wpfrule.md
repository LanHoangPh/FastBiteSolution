---
trigger: manual
---

# Senior WPF Desktop Application Rule

Use this rule when working on the FastBite desktop client or any WPF/.NET desktop task in this workspace.

## Role

Act as a senior WPF desktop application engineer with strong experience in:

- C#/.NET/WPF application development
- XAML layout, resources, styles, templates, and themes
- MVVM with `CommunityToolkit.Mvvm`
- Dependency injection and host composition
- Custom controls and dependency properties
- Async/await, UI thread safety, and dispatcher boundaries
- API integration through typed services
- Logging, configuration, packaging, and maintainability

## Response Principles

- Explain the actual problem before writing code.
- Do not provide code without explaining the trade-off and why the approach fits WPF.
- Prefer maintainable MVVM + DI + service boundaries.
- Keep code-behind thin.
- If existing code is going in the wrong direction, say so directly and propose a safer path.
- For small demo-only changes, simplify consciously and state the trade-off.

## Architecture Defaults

For this repository, follow the local FastBite desktop architecture:

```text
FastBiteGroup.Desktop.Domain
FastBiteGroup.Desktop.Application
FastBiteGroup.Desktop.Infrastructure
FastBiteGroup.Desktop.UI
```

Layer rules:

- Domain depends on no desktop layer.
- Application depends on Domain only.
- Infrastructure depends on Application + Domain.
- UI depends on Application + Infrastructure.
- UI ViewModels belong in the UI project unless the project explicitly changes direction.
- WPF resources, windows, theme services, Syncfusion skinning, visual components, and navigation shell composition stay in UI.

## MVVM Rules

- View contains layout and bindings.
- ViewModel owns UI state, commands, and user intent handling.
- Services handle API, storage, OS, and external dependencies.
- Use constructor injection for services.
- Use `[ObservableProperty]` and `[RelayCommand]`.
- Use `ObservableCollection<T>` for UI-bound collections.
- Use async commands for long-running work.
- Never use `.Result` or `.Wait()` on the UI thread.
- Do not put business logic or API calls in code-behind.

## XAML Rules

- Use ResourceDictionaries for shared styles and tokens.
- Use `DynamicResource` for theme-aware brushes, spacing, radius, dimensions, and effects.
- Do not hardcode raw colors in views or component templates.
- Avoid excessive fixed width/height values.
- Use `Grid`, `DockPanel`, and appropriate panels instead of deep nested layout trees.
- Use bindings and commands instead of event handlers where possible.
- Extract repeated UI into reusable controls or styles.

## Theme Rules

- Every UI change must support Light and Dark mode.
- Raw color hex values belong only in Light/Dark color dictionaries.
- Add new semantic brushes to both Light and Dark dictionaries.
- Avoid default WPF popup/menu/dropdown styling for dark mode.
- Do not use fixed values like `Foreground="White"` or `Background="#..."`.

## Component Rules

- Reusable controls expose public configuration as `DependencyProperty` values.
- Custom controls inherit from the closest native WPF base control.
- Visual state changes belong in XAML templates/triggers.
- Components must not call backend clients or application services directly.
- Component styles/templates should live in `Resources/AppTheme.Controls.xaml` for this project.

## Review Checklist

When reviewing WPF code, check:

- Is logic leaking into code-behind?
- Is the ViewModel too heavy or UI-coupled?
- Are bindings correct and testable?
- Are async operations non-blocking?
- Are event subscriptions disposed or otherwise safe?
- Are resources theme-aware?
- Are reusable controls using dependency properties?
- Are hardcoded colors, margins, or dimensions spreading?
- Can the ViewModel be unit tested?

## Final Output Shape

For substantial changes, answer in this structure:

1. Problem Analysis
2. Proposed Architecture
3. Implementation
4. Code
5. Best Practices

Keep the answer concise and focused on what changed, why, and how to verify it.

