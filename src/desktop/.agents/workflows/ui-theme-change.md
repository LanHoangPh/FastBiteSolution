# Workflow: UI And Theme Change

Use this when changing XAML, resource dictionaries, component templates, theme colors, spacing, effects, or shell visuals.

## 1. Inspect UI Rules

Read:

1. `docs/ai-context/UI_GUIDELINES.md`
2. `.agents/skills/wpf-desktop/SKILL.md`
3. `wpf-taste-skill/wpf-ui-taste/SKILL.md`
4. Relevant detailed skill:
   - `wpf-taste-skill/wpf-theme-system/SKILL.md`
   - `wpf-taste-skill/wpf-component-design/SKILL.md`
   - `wpf-taste-skill/wpf-xaml-refactor/SKILL.md`
   - `wpf-taste-skill/wpf-navigation-shell/SKILL.md`

## 2. Theme Rules

- Use `DynamicResource` for theme-aware brushes, spacing, radius, dimensions, and effects.
- Raw hex values are allowed only in:
  - `FastBiteGroup.Desktop.UI/Resources/Themes/LightColors.xaml`
  - `FastBiteGroup.Desktop.UI/Resources/Themes/DarkColors.xaml`
- Add semantic brushes to both Light and Dark dictionaries.
- Do not use `Foreground="White"` or `Background="#..."`.
- Do not rely on default WPF popup/menu/dropdown styling.

## 3. Component Rules

- Reuse existing components before creating new ones:
  - `ModernButton`
  - `IconButton`
  - `Avatar`
  - `StatusBadge`
  - `SearchBox`
  - `EmptyState`
  - `SectionHeader`
- Public component options must be `DependencyProperty` values.
- Visual states belong in XAML templates/triggers.
- Component templates live in `Resources/AppTheme.Controls.xaml`.

## 4. Localization Rules

- Do not hardcode user-facing strings in XAML views.
- Add labels/text to both `Resources/Languages/Strings.vi.xaml` and `Resources/Languages/Strings.en.xaml`.
- Bind localized strings with `{DynamicResource Key}` so language switching can happen at runtime.

## 5. Visual QA

Before handoff:

- Check Light mode.
- Check Dark mode.
- Check System mode if theme behavior changed.
- Search for raw hex colors outside theme dictionaries.
- Search for fixed foreground/background values.
- Build the solution.

