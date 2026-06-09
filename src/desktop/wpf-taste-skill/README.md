# WPF Taste Skill Collection

The **WPF Taste Skill Collection** is a curated set of AI agent instructions (skills) designed to guide large language models in building, refactoring, and designing modern, maintainable, and tasteful WPF (Windows Presentation Foundation) desktop applications.

Inspired by the [taste-skill](https://github.com/leonxlnx/taste-skill) framework for the web, this collection brings the "anti-slop" philosophy to .NET desktop development. It enforces clean MVVM architecture, proper XAML resource management, accessible enterprise UX, and modern visual design without relying on outdated WPF defaults or inappropriate web paradigms.

## Who Should Use This?

- **AI Coding Agents**: Load these `.md` files to understand context before touching WPF code.
- **.NET Developers**: Use these skills as system prompts when pairing with AI to build desktop apps.
- **UI/UX Designers**: Use the design parameters to translate modern UI/UX intent into WPF architecture.

## How It Differs from Web-focused Taste Skill

Web development heavily relies on CSS, Tailwind, flex-math, and HTML tags. WPF is fundamentally different.
- Instead of Tailwind, we use **ResourceDictionaries** and semantic tokens.
- Instead of DOM manipulation, we use **DataBindings**, **Templates**, and **MVVM**.
- Instead of web-like routing, we use **Desktop Navigation Regions** and **ViewModels**.
- Instead of CSS animations, we use **Storyboards** and **VisualStateManager**.
- Design priorities shift from "scroll-driven landing pages" to "data-heavy productivity dashboards", "management systems", and "enterprise tools".

## Included Skills

1. **`wpf-ui-taste`**: Core visual principles, layout rules, and aesthetic parameters for WPF.
2. **`wpf-mvvm-architecture`**: Clean separation of concerns using CommunityToolkit.Mvvm.
3. **`wpf-xaml-refactor`**: Guidelines for cleaning up messy, inline XAML.
4. **`wpf-theme-system`**: Scalable light/dark theming and token management.
5. **`wpf-datagrid-form`**: Professional data-heavy and form-heavy enterprise screens.
6. **`wpf-navigation-shell`**: Maintainable desktop application shells and routing.
7. **`wpf-component-design`**: Reusable component extraction (UserControl vs CustomControl).
8. **`wpf-api-client-integration`**: Connecting WPF ViewModels to backend APIs properly.
9. **`wpf-desktop-redesign`**: Auditing and incrementally improving existing WPF apps.

## Recommended Workflow

1. **Creating a new WPF UI**: Start by loading `wpf-ui-taste` and `wpf-mvvm-architecture`. Set your parameters (Variance, Density). Ask the AI to scaffold the ViewModel and View.
2. **Refactoring existing XAML**: Load `wpf-xaml-refactor`. Paste the messy XAML. Instruct the agent to extract styles to resources and clean up bindings.
3. **Building a theme system**: Load `wpf-theme-system`. Ask the AI to generate a semantic color dictionary and control templates.
4. **Designing a dashboard**: Load `wpf-ui-taste` and `wpf-datagrid-form`. Specify high `VISUAL_DENSITY`.
5. **Integrating with backend API**: Load `wpf-api-client-integration` and `wpf-mvvm-architecture`. 

## FastBite Desktop Notes

When using this collection inside `D:\CodeVs\FastBiteSolution\src\desktop`, `AGENTS.md` and `docs/ai-context/*` take precedence over generic examples in these skills.

- UI ViewModels live in `FastBiteGroup.Desktop.UI/ViewModels`.
- Application owns abstractions and use cases.
- Infrastructure owns Refit/HttpClient configuration and secure storage.
- Component styles currently live in `FastBiteGroup.Desktop.UI/Resources/AppTheme.Controls.xaml`.
- Use `DynamicResource` for theme-aware brushes, spacing, radius, effects, dimensions, and cross-dictionary resources.
- Do not generate React, Tailwind, HTML, CSS, GSAP, or web landing-page patterns for this WPF project.

## Example Prompts for Agents

> "Read `wpf-taste-skill/wpf-ui-taste/SKILL.md` and `wpf-taste-skill/wpf-datagrid-form/SKILL.md`. I need a customer management view. Set VISUAL_DENSITY to 8 and ENTERPRISE_POLISH_LEVEL to 9. Scaffold the DataGrid and ViewModel."

> "Load `wpf-xaml-refactor`. Look at `MainWindow.xaml`. It has too many hardcoded colors and inline margins. Extract these into standard StaticResources and clean up the layout."
