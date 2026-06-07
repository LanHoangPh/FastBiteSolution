# UI_GUIDELINES.md - FastBite Desktop

## Product Feel

FastBite Desktop should feel like a focused communication/workspace tool:

- Quiet, utilitarian, and fast to scan.
- Dense enough for repeated daily work.
- Avoid marketing-style landing pages inside the app.
- Prefer practical shell/navigation surfaces over decorative layouts.

## WPF Resource Rules

- Use `DynamicResource` for colors and brushes that must react to theme changes.
- Use `StaticResource` only for stable values such as font sizes or non-theme constants.
- Keep color tokens in `Resources/Themes/LightColors.xaml` and `Resources/Themes/DarkColors.xaml`.
- Keep shared typography in `Resources/Fonts.xaml`.
- Keep reusable control styles in `Resources/ButtonStyles.xaml` or a new focused resource dictionary.

## Theme Tokens

Important brush keys:

```text
PrimaryBrush
SecondaryBrush
BackgroundBrush
SurfaceBrush
SurfaceMutedBrush
BorderBrush
HoverBrush
TextPrimaryBrush
TextSecondaryBrush
TextOnPrimaryBrush
SuccessBrush
WarningBrush
ErrorBrush
```

Any new themed UI should use these keys first. Add new tokens only when the existing set cannot express the required state clearly.

## Theme UX

Theme selection lives under:

```text
Settings -> Appearance
```

Supported choices:

- System
- Light
- Dark

Default is `System`.

Use a custom WPF popup or explicit control templates for themed menus. Default WPF menus can render with mismatched popup colors.

## Syncfusion Guidance

- Prefer Syncfusion controls for complex UI surfaces such as chat, rich lists, and enterprise controls.
- Keep app shell theme tokens independent from Syncfusion skin names.
- Apply Syncfusion theme/skin from `ThemeService` when a Syncfusion control or window needs it.
- Do not globally override Syncfusion styles with implicit WPF styles unless the change is intentional and tested.

## Layout Guidance

- Avoid nested cards.
- Keep cards/panels at 8px corner radius or less.
- Make text fit at common desktop widths.
- Use clear controls for settings: buttons, popup menus, radio/segmented choices, check marks.
- Prefer concise labels over explanatory paragraphs in the app UI.
