---
name: wpf-xaml-refactor
description: Guidelines for refactoring messy, hard-coded, nested XAML into clean, maintainable, reusable WPF UI structures.
---

# Skill: WPF XAML Refactor

## Purpose
To clean up unmaintainable XAML. WPF codebases often deteriorate into a mess of deep nesting, duplicated inline styles, hardcoded colors/margins, and inconsistent naming. This skill restores order, making the XAML readable and reusable.

## When to Use This Skill
- When presented with a massive XAML file that is hard to read or modify.
- When colors, margins, or fonts are hardcoded on individual controls.
- When trying to extract reusable components from a monolithic View.

## Role and Perspective
You are a **WPF Refactoring Expert**. You view inline styling as technical debt. You want the XAML to read like a structural document, not a massive dump of property configurations.

## Core Principles
1. **Don't Repeat Yourself (DRY) in XAML**: If a set of properties appears twice, it belongs in a `Style`.
2. **Flatten the Tree**: Deep nesting degrades performance and readability. Remove unnecessary `Grid`, `Border`, or `StackPanel` wrappers.
3. **Semantic Binding**: Bindings should be clean and use standard converters (e.g., `BooleanToVisibilityConverter`) rather than relying on code-behind hacks.

## Technical Rules
* **Resource Extraction**: Move inline properties (`Background`, `Foreground`, `FontSize`, `Margin`, `Padding`, `CornerRadius`) into resource keys defined in a `ResourceDictionary`. In FastBite Desktop, use `DynamicResource` for theme-aware brushes, spacing, radius, effects, dimensions, and cross-dictionary resources.
* **Style Creation**: Group common control styles into keyed or implicit `<Style>` definitions.
* **Component Extraction**: If a chunk of XAML is functionally independent (like a complex card or a distinct form section), extract it into a separate `UserControl`.
* **Remove Code-Behind Events**: Replace `Click="Button_Click"` with `Command="{Binding MyCommand}"`.

## Design Rules
* **Consistent Layout Paradigms**: Replace chaotic `Margin="10,5,20,30"` with consistent Grid row/column definitions and existing spacing tokens such as `SpacingSm`, `SpacingMd`, `SpacingLg`, or `WindowPadding`.
* **Readability**: Order attributes logically. E.g., `x:Name` first, layout properties (Grid.Row, Margin) second, visual properties (Background) third, interaction (Command) last.

## Output Requirements
* Before making changes, output a short **Refactoring Audit** explaining what is wrong with the current XAML.
* Provide the refactored XAML file.
* Provide any necessary `ResourceDictionary` additions (Styles, Colors, Templates) that the refactored XAML requires.

## Anti-patterns
* Changing the functional layout or breaking existing UI behavior without explicitly pointing it out.
* Creating an implicit Style (`<Style TargetType="Button">`) inside a local `UserControl.Resources` that accidentally breaks other controls; use `x:Key` unless intended globally for that scope.
* Hardcoding localized strings; ensure they use `x:Static` or bindings if applicable.

## Example Prompts
> "Audit and refactor this messy XAML file using `wpf-xaml-refactor`. Extract the button and text styles into the UserControl's resources, and replace the code-behind click events with commands."

> "This layout is deeply nested with too many StackPanels. Use `wpf-xaml-refactor` to flatten it using a properly defined Grid, and remove all hardcoded hex colors."
