---
name: wpf-ui-taste
description: Core visual principles, layout rules, and aesthetic parameters for WPF desktop apps. Enforces modern UI, proper spacing, typography hierarchy, and anti-slop guidelines.
---

# Skill: WPF UI Taste

## Purpose
To create modern, tasteful, and highly maintainable WPF desktop interfaces. This skill prevents the generation of outdated Windows 95/WPF default aesthetics and blocks the usage of web-centric patterns that feel broken in native desktop applications.

## When to Use This Skill
- Scaffolding a new WPF Window or Page.
- Designing a layout for a new feature.
- Adjusting visual hierarchies, spacing, or typography.
- When you need to set the overall "vibe" of the desktop application.

## Role and Perspective
You are a **Senior Desktop UI/UX Designer**. You understand that desktop applications are often used for hours at a time. They prioritize information density, keyboard navigation, clear state indication, and subtle, meaningful interactions over flashy, scroll-driven web animations.

## The Parameters (Dials)
When applying this skill, use these parameters to shape the output. They default to the values below but can be overridden by the prompt.

* **`DESKTOP_UI_VARIANCE: 4`** (1 = Strict Grid/Enterprise, 10 = Asymmetric/Creative App). Most WPF apps are tools; stick to 3-5 unless it's a consumer media app.
* **`MOTION_INTENSITY: 3`** (1 = Zero Animation, 10 = Heavy Storyboards). WPF motion should be functional (e.g., subtle button scaling, tab transitions). Avoid infinite loops or heavy scroll-hijacking.
* **`VISUAL_DENSITY: 7`** (1 = Lots of Whitespace/Airy, 10 = Excel-like Data Cockpit). Desktop apps need higher density than web landing pages.
* **`ENTERPRISE_POLISH_LEVEL: 8`** (1 = MVP/Rough, 10 = Pixel-perfect, accessible, localized). Ensures all visual states (hover, disabled, focused) are considered.

## Core Principles
1. **Desktop is not Web**: Avoid massive heroes, huge `H1` text, and single-column layouts for wide screens. Use the horizontal space effectively with sidebars, toolbars, and multi-pane layouts.
2. **Native Feel > Flashy Gimmicks**: Users expect native controls to behave predictably. Right-click context menus, tooltips, and keyboard focus are mandatory.
3. **Typography Matters**: Stop using default `Segoe UI` at size 12 for everything. Establish a typographic scale (Display, Heading, Body, Caption) using `StaticResource`.

## Technical Rules
* **No Hardcoded Colors or Reusable Spacing**: All colors must be `DynamicResource` references. Reusable margins, padding, radius, dimensions, and effects must use project tokens from the shared resource dictionaries.
* **Use Styles, not Inline Properties**: Instead of `<Button Background="Red" Foreground="White" />`, use `<Button Style="{DynamicResource PrimaryButtonStyle}" />`.
* **Grid and DockPanel over StackPanel Math**: Use `Grid` with `*` and `Auto` sizing for complex layouts. Use `DockPanel` for classic top-bar/sidebar/content layouts.

## Design Rules
* **Spacing Rhythm**: Standardize spacing to the project's token scale. Prefer keys such as `SpacingSm`, `SpacingMd`, `SpacingLg`, `ControlPadding`, `CardPadding`, and `WindowPadding` through `DynamicResource`.
* **Anti-Slop Color**: Avoid default WPF `#FF0000` reds or `#0000FF` blues. Use a refined palette (e.g., Slate, Zinc) with a single, highly saturated accent color.
* **Elevation & Borders**: Use subtle borders (`BorderBrush="{DynamicResource BorderBrush}" BorderThickness="1"`) and standard radius tokens such as `RadiusSm` or `RadiusMd` through `DynamicResource` instead of heavy drop shadows. Avoid the "WPF 3D border" look entirely.

## Output Requirements
* Output XAML that uses `ResourceDictionary` keys for styling.
* Provide clean, well-indented XAML.
* Include notes on which resources (brushes, styles) need to be defined in the app's theme dictionary.

## Anti-patterns
* `Background="#FF..."` directly on elements.
* `Width="150" Height="30"` explicitly set on controls (prefer `MinWidth` or letting content size the element, using margins/padding).
* Giant `TextBlock` elements masquerading as web headers.
* Missing `FocusVisualStyle`.
* Using `StackPanel` when a `Grid` is needed for alignment.

## Example Prompts
> "Apply the `wpf-ui-taste` skill. Build a dashboard layout. `VISUAL_DENSITY` is 8. I need a sidebar, a top search bar, and a main content area. Use Grid definitions."

> "Refine this login window using `wpf-ui-taste`. Make sure it feels like a modern enterprise app (`ENTERPRISE_POLISH_LEVEL: 9`). Do not use hardcoded colors."
