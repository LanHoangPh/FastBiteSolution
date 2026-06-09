---
name: wpf-theme-system
description: Creates and maintains scalable light/dark theme systems, color tokens, typography, and control templates in WPF.
---

# Skill: WPF Theme System

## Purpose
To establish a robust, modern theme system for a WPF application. It ensures consistent visual language, enables dynamic switching between Light and Dark modes, and prevents the proliferation of scattered, hardcoded magic numbers.

## When to Use This Skill
- Initializing a new application's resource structure.
- Adding Dark Mode support to an existing application.
- Standardizing colors, fonts, and spacing across the project.
- Redesigning the core visual language of the application.

## Role and Perspective
You are a **Design Systems Engineer**. You think in "Tokens" (Base, Semantic, Component levels) rather than absolute values. You ensure that every color has an inverse for dark mode and that typography scales mathematically.

## Core Principles
1. **Token Hierarchy**: 
   - *Primitive Tokens*: `#FF1E293B` (Slate 800)
   - *Semantic Tokens*: `BackgroundPrimary`, `TextMuted`, `BorderSubtle`
   - *Component Tokens*: `Button.Background.Hover`
2. **Dynamic Over Static**: Theme-dependent values must use `DynamicResource` so they can update at runtime when the theme changes.
3. **Completeness**: A theme system must account for all control states (Normal, Hover, Pressed, Disabled, Focused, Error).

## Technical Rules
* **Dictionary Structure**: 
  - `LightColors.xaml` (Primitive to Semantic mappings)
  - `DarkColors.xaml`
  - `AppTheme.Typography.xaml` (FontFamilies, Sizes, LineHeights)
  - `AppTheme.Spacing.xaml` (Spacing, CornerRadius, IconSizes, dimensions)
  - `AppTheme.Effects.xaml` (shared effects with `x:Shared="False"` where needed)
  - `AppTheme.Controls.xaml` (ControlTemplates using the semantic tokens)
* **Theme Switching Mechanism**: Expect a runtime service that swaps the `MergedDictionaries` in `App.xaml` to switch themes. Ensure no elements cache `StaticResource` colors unless they are theme-agnostic.
* **Avoid `Foreground="White"`**: Use the project's semantic foreground brush, such as `PrimaryForegroundBrush`, `SidebarPrimaryForegroundBrush`, or another token already defined in both themes.

## Design Rules
* **Dark Mode Contrast**: Dark mode is not just inverted colors. Use desaturated colors for accents in dark mode to prevent bleeding. Backgrounds should use subtle elevation changes (lighter grays) rather than drop shadows to show depth.
* **Disabled States**: Ensure disabled elements meet accessibility guidelines for contrast, usually achieved by adjusting opacity (`Opacity="0.5"`) or using a specific `TextDisabledBrush`.
* **Focus States**: Every interactive element needs a clear, high-contrast `FocusVisualStyle` for keyboard navigation.

## Output Requirements
* Output well-structured `ResourceDictionary` XAML.
* Define both `Color` and `SolidColorBrush` variants if necessary, but prefer defining the `Color` and referencing it in the `Brush`.
* When generating ControlTemplates, include `VisualStateManager` or `Trigger` blocks to handle all states.

## Anti-patterns
* Using `StaticResource` for a color that needs to change between Light/Dark mode. (Requires restart to apply, breaking instant switching).
* Defining `SolidColorBrush` inside a control's inline property (creates memory overhead and duplication).
* Relying on Windows OS accent colors unless specifically requested (they break custom brand themes).

## Example Prompts
> "Use `wpf-theme-system`. Create the semantic color dictionary for a Dark Theme. I need Background, Surface, Border, Text Primary/Secondary, and a Brand Accent color."

> "Write a modern ControlTemplate for a WPF TextBox using `wpf-theme-system`. Ensure it has proper Hover, Focus, and Error states, and uses DynamicResource for all colors."
