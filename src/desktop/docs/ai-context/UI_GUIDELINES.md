# UI_GUIDELINES.md - FastBite Desktop

## Product Feel

FastBite Desktop should feel like a focused communication/workspace tool:

- Quiet, utilitarian, and fast to scan.
- Dense enough for repeated daily work.
- Clear hierarchy without marketing-style landing pages inside the app.
- Practical shell/navigation surfaces before decorative layouts.
- Professional Light and Dark mode support for every UI surface.

## Theme-First Rule

Every UI change must be built and reviewed for both Light and Dark mode.

- Do not depend on default WPF control colors.
- Do not hardcode colors in views or component styles.
- Do not add a semantic brush to only one theme file.
- Popups, dropdowns, menus, context menus, and flyouts need explicit styling/templates because WPF defaults often break dark mode.
- `System` mode is handled by `IThemeService` and resolves to an effective Light or Dark theme.

## Resource Files

Keep design tokens in these files:

- Colors and brushes: `Resources/Themes/LightColors.xaml` and `Resources/Themes/DarkColors.xaml`
- Typography: `Resources/AppTheme.Typography.xaml`
- Spacing, radius, dimensions: `Resources/AppTheme.Spacing.xaml`
- Effects: `Resources/AppTheme.Effects.xaml`
- Reusable control styles/templates: `Resources/AppTheme.Controls.xaml`

## Color Rules

Raw hex colors are allowed only in `LightColors.xaml` and `DarkColors.xaml`.

Wrong:

```xml
<Border Background="#FFFDFDFD" BorderBrush="#FFF4F6F8" />
<Button Foreground="White" Background="#FF51F0A8" />
```

Correct:

```xml
<Border Background="{DynamicResource CardBrush}"
        BorderBrush="{DynamicResource BorderBrush}" />

<Button Foreground="{DynamicResource PrimaryForegroundBrush}"
        Background="{DynamicResource PrimaryBrush}" />
```

When adding a new semantic color:

- Add the raw `Color` key to both `LightColors.xaml` and `DarkColors.xaml`.
- Add the matching `SolidColorBrush` key to both files.
- Use the brush key from views/templates.
- Pick separate Light and Dark values intentionally; do not copy light colors blindly into dark mode.

## Resource Usage Rules

Use `DynamicResource` for:

- Brushes and colors used by UI.
- Spacing, margins, padding, radius, dimensions, and effects from shared dictionaries.
- Any resource referenced across dictionary boundaries.
- Anything that must react to theme changes at runtime.

Use `StaticResource` only for:

- Immutable resources declared in the same dictionary.
- Local templates, converters, and non-theme constants.
- Cases where runtime theme refresh is not relevant and the resource dependency is local.

Wrong:

```xml
<Setter Property="CornerRadius" Value="{StaticResource RadiusMd}" />
<Setter Property="Padding" Value="{StaticResource ControlPadding}" />
```

Correct:

```xml
<Setter Property="CornerRadius" Value="{DynamicResource RadiusMd}" />
<Setter Property="Padding" Value="{DynamicResource ControlPadding}" />
```

## Core Theme Tokens

Core semantic brushes:

- `PrimaryBrush`, `PrimaryForegroundBrush`
- `SecondaryBrush`, `SecondaryForegroundBrush`
- `BackgroundBrush`, `ForegroundBrush`
- `CardBrush`, `CardForegroundBrush`
- `PopoverBrush`, `PopoverForegroundBrush`
- `MutedBrush`, `MutedForegroundBrush`
- `AccentBrush`, `AccentForegroundBrush`
- `DestructiveBrush`, `DestructiveForegroundBrush`
- `BorderBrush`
- `InputBrush`
- `RingBrush`

Status and presence brushes:

- `SuccessBrush`, `SuccessForegroundBrush`
- `WarningBrush`, `WarningForegroundBrush`
- `ErrorBrush`, `ErrorForegroundBrush`
- `OnlineBrush`
- `AwayBrush`
- `BusyBrush`
- `OfflineBrush`

Sidebar brushes:

- `SidebarBrush`
- `SidebarForegroundBrush`
- `SidebarPrimaryBrush`
- `SidebarPrimaryForegroundBrush`
- `SidebarAccentBrush`
- `SidebarAccentForegroundBrush`
- `SidebarBorderBrush`
- `SidebarRingBrush`

Chart brushes:

- `Chart1Brush` to `Chart5Brush`

## Spacing, Radius, and Dimensions

Use existing spacing and radius tokens instead of literal repeated values when building reusable styles:

- `RadiusSm`, `RadiusMd`, `RadiusLg`, `RadiusXl`
- `SpacingXs`, `SpacingSm`, `SpacingMd`, `SpacingLg`, `SpacingXl`, `SpacingXxl`
- `ControlPadding`, `CardPadding`, `WindowPadding`
- `IconSizeSm`, `IconSizeMd`, `IconSizeLg`
- `IconButtonSizeSm`, `IconButtonSizeMd`, `IconButtonSizeLg`
- `FocusRingThickness`

Literal one-off layout values are acceptable in a view when they are local layout composition, but reusable components and styles should use tokens.

## Drop Shadow Rules

WPF `DropShadowEffect` can hurt rendering performance when overused.

- Always set `x:Shared="False"` on `DropShadowEffect` resources.
- Do not attach effects to repeating elements such as `DataGridRow`, `ListBoxItem`, chat messages, or virtualized items.
- For Dark mode depth, prefer border contrast or background brightness shifts before shadows.

## MVVM Rules

Views should not own business or display state beyond WPF-specific wiring.

Wrong:

```xml
<Button Content="Save" Click="OnSaveClick" />
```

```csharp
private void OnSaveClick(object sender, RoutedEventArgs e)
{
    SaveUserInput();
}
```

Correct:

```xml
<components:ModernButton Content="Save"
                         Variant="Primary"
                         Command="{Binding SaveCommand}" />
```

```csharp
[RelayCommand]
private void Save()
{
    // UI orchestration or call into an Application use case.
}
```

Code-behind may be used for:

- `InitializeComponent`
- setting `DataContext`
- window-specific service calls that require a view instance
- WPF interop that cannot be expressed cleanly in XAML or a ViewModel

## Component Rules

Reusable WPF components live under `FastBiteGroup.Desktop.UI/Views/Components`.

- Custom controls inherit from the closest native WPF control.
- Public component options must be `DependencyProperty` values.
- Visual state changes belong in `ControlTemplate.Triggers`.
- Component styles/templates live in `Resources/AppTheme.Controls.xaml`.
- Components must not call backend/API clients or Application services directly.
- Components must use `DynamicResource` for theme-aware values.
- Components must render correctly in Light and Dark mode.

Current reusable components:

- `ModernButton`: Button with variants, icon placement, size/loading support, and custom corner radius.
- `IconButton`: Compact icon-only button with variant and size support.
- `Avatar`: Initials/image avatar with optional presence status.
- `StatusBadge`: Small semantic status/presence indicator.
- `SearchBox`: TextBox-based search input with placeholder and clear behavior.
- `EmptyState`: Reusable empty content surface.
- `SectionHeader`: Reusable title/subtitle/action header.

## Custom Control Style Location

This app keeps implicit custom-control styles in:

```text
FastBiteGroup.Desktop.UI/Resources/AppTheme.Controls.xaml
```

Example:

```xml
<Style TargetType="{x:Type components:IconButton}">
    ...
</Style>
```

Because the style has no `x:Key`, WPF applies it automatically to all `components:IconButton` instances while `AppTheme.Controls.xaml` is merged through `MainTheme.xaml`.

If components are later moved into a standalone control library, move default control templates into `Themes/Generic.xaml`.

## Syncfusion Guidance

- Prefer WPF-native controls and custom templates for shell, simple forms, buttons, badges, avatars, search, and empty states.
- Use Syncfusion when it provides clear value for complex controls such as chat, rich data grids, or advanced editors.
- Do not couple app theme tokens to Syncfusion skin names.
- Apply Syncfusion skinning best-effort through `IThemeService`.

## Visual QA Checklist

Before finishing UI work:

- Check Light mode.
- Check Dark mode.
- Check `System` mode if theme behavior changed.
- Verify popup/menu/dropdown surfaces use app tokens.
- Confirm no direct hex colors outside theme dictionaries.
- Confirm no `Foreground="White"` or similar fixed colors in views/templates.
- Confirm reusable components use `DynamicResource`.
- Confirm build passes after closing any running app instance that may lock DLLs.
