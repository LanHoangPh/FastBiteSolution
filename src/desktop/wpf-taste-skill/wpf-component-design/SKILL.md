---
name: wpf-component-design
description: Guidelines for building reusable WPF UI components, choosing between UserControl and CustomControl, and managing DependencyProperties.
---

# Skill: WPF Component Design

## Purpose
To create highly reusable, encapsulate, and performant UI components in WPF. It helps you decide whether to use a `UserControl` (composition) or a `CustomControl` (lookless template) and how to expose properties cleanly.

## When to Use This Skill
- Building a reusable visual element (e.g., `IconButton`, `StatusBadge`, `Card`, `SearchBox`).
- Creating complex composite controls (e.g., `PaginationControl`, `EmptyState`).
- Standardizing the UI toolkit of an application.

## Role and Perspective
You are a **WPF Controls Engineer**. You design APIs for other developers to use. A component's public interface (`DependencyProperties`) must be intuitive, strongly typed, and agnostic of its internal layout.

## Core Principles
1. **UserControl vs. CustomControl**:
   - Use `UserControl` when combining existing controls into a fixed layout (e.g., a `UserProfileCard` or `EmptyState`). Code-behind is acceptable for internal wiring.
   - Use `CustomControl` (inheriting from `Control`, `Button`, `ContentControl`) when building a primitive that requires extensive restyling via `ControlTemplate` (e.g., `ModernButton`, `Badge`).
2. **Dependency Properties**: Expose configurable attributes (Text, Icon, CornerRadius, Command) as `DependencyProperty`. Never use standard C# properties for data-bound visual elements in a control.
3. **TemplateBinding**: In `CustomControl` templates, use `{TemplateBinding Property}` or `{Binding RelativeSource={RelativeSource TemplatedParent}}`.

## Technical Rules
* **Naming Conventions**: Name Dependency Properties explicitly (`public static readonly DependencyProperty IconProperty = ...`).
* **Visual States**: For `CustomControl`, define states in XAML templates. In this project, `ControlTemplate.Triggers` are the established default; `VisualStateManager` is acceptable when it makes a complex component clearer.
* **Component Encapsulation**: A control should not rely on application-level ViewModels. Pass data in via Dependency Properties.

## Design Rules
* **Theme Awareness**: Components must consume the global theme tokens (using `DynamicResource`) rather than defining hardcoded colors internally.
* **Flexibility**: Build controls to adapt to their containers. Do not hardcode `Width="200"` inside a reusable `Card` control; let the consumer dictate the layout.
* **Empty/Loading states for complex components**: A `DataList` or `SearchBox` component should gracefully handle its own loading animations or empty visuals.

## Output Requirements
* Specify if the output is a `UserControl` or a `CustomControl` and explain why.
* Provide the `.cs` file with properly defined `DependencyProperty` registrations.
* Provide the `.cs` file and the style/template location. In FastBite Desktop, current default component styles live in `FastBiteGroup.Desktop.UI/Resources/AppTheme.Controls.xaml`; use `Themes/Generic.xaml` only if components are later extracted into a standalone control library.

## Anti-patterns
* Using standard C# auto-properties instead of DependencyProperties on a UserControl, breaking `Binding`.
* Naming a UserControl `x:Name="root"` and binding inside it like `Text="{Binding ElementName=root, Path=MyProp}"` instead of setting `DataContext` correctly or using `RelativeSource`.
* Building a `CustomControl` when a simple `<Style TargetType="Button">` with a `ControlTemplate` would suffice.

## Example Prompts
> "Use `wpf-component-design` to create a reusable `StatusBadge` CustomControl. It should expose `Text` and `BadgeType` (Enum: Success, Warning, Error) dependency properties, and change colors based on the type."

> "Design an `EmptyState` UserControl using `wpf-component-design`. Expose `Title`, `Message`, `IconData` (Geometry), and `ActionCommand` as Dependency Properties."
