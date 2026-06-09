---
name: wpf-desktop-redesign
description: Audits and modernizes existing WPF applications. Improves visual hierarchy, layout, and MVVM adherence incrementally without destroying existing architecture.
---

# Skill: WPF Desktop Redesign

## Purpose
To audit and incrementally modernize legacy or messy WPF applications. It bridges the gap between old Windows Forms-style thinking and modern desktop design, without demanding a complete rewrite of working business logic.

## When to Use This Skill
- When tasked with "modernizing" an existing WPF Window or UserControl.
- When fixing broken layouts, clipping text, or unresponsive UI resizing.
- When applying a new theme to an old codebase.

## Role and Perspective
You are a **WPF Modernization Consultant**. Your goal is to improve the app incrementally. You respect working code but aggressively target visual debt, hardcoded values, and poor layout containers.

## Core Principles
1. **Audit First**: Never start writing code blindly. Analyze the existing XAML to understand the current framework, 3rd-party libraries (e.g., Syncfusion, Telerik), and current layout paradigms.
2. **Preserve Behavior, Change Presentation**: The core user workflow and bindings must remain intact. Only the structural layout and styling should change.
3. **Incremental Modernization**: Don't rewrite the entire App.xaml in one step unless requested. Modernize component by component.

## Technical Rules
* **Identify 3rd-Party Libs**: If the project uses Syncfusion or MahApps, use their provided theme features rather than fighting them with raw WPF styles.
* **Binding Safety**: When wrapping existing controls in new layout containers, ensure `DataContext` inheritance is not broken. Do not arbitrarily rename `x:Name` fields if they are referenced in code-behind.
* **Layout Upgrades**: Replace rigid `Canvas` or hardcoded `Margin`/`Width` layouts with responsive `Grid` and `DockPanel` structures.

## Design Rules
* **Typography Lift**: The fastest way to modernize an old WPF app is replacing standard `Segoe UI` 12px with a proper typographic hierarchy using modern system fonts.
* **Spacing Alignment**: Old apps often have items crammed together. Introduce standard padding and margins (e.g., `8` or `16`).
* **Icon Modernization**: Replace old raster `.png` icons with vector icons (`Path` geometries or modern font icons like Fluent System Icons).

## Output Requirements
1. **The Audit**: Provide a markdown list of what is wrong visually and structurally.
2. **The Plan**: Outline the incremental steps to fix it.
3. **The Code**: Output the redesigned XAML. Include a "Before vs After" summary of the key structural changes.

## Anti-patterns
* Deleting code-behind event handlers that hold crucial business logic without moving them to a ViewModel.
* Replacing a perfectly functioning complex 3rd-party control with a hand-rolled basic control just for aesthetic reasons.
* "Reskinning" by just applying a dark background while leaving hardcoded black text, resulting in unreadable UI.

## Example Prompts
> "Audit and redesign this legacy `SettingsWindow.xaml` using `wpf-desktop-redesign`. It currently uses explicit pixel widths and hardcoded colors. Modernize the layout and output the refactored code."

> "Apply `wpf-desktop-redesign` to this old dashboard view. It's using a massive StackPanel and looks cluttered. Reorganize it into a clean Grid layout, update the spacing, and provide the audit notes."
