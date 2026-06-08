# UI_GUIDELINES.md - FastBite Desktop

## Product Feel

FastBite Desktop should feel like a focused communication/workspace tool:
- Quiet, utilitarian, and fast to scan.
- Dense enough for repeated daily work.
- Avoid marketing-style landing pages inside the app.
- Prefer practical shell/navigation surfaces over decorative layouts.

---

## WPF Resource Rules

- Use `DynamicResource` for colors, brushes, margins, paddings, and effects that must react to theme changes or depend on other dictionaries.
- Use `StaticResource` only for values declared inside the **same** dictionary file or non-theme constants like font names.
- Keep color tokens in `Resources/Themes/LightColors.xaml` and `Resources/Themes/DarkColors.xaml`.
- Keep shared typography in `Resources/AppTheme.Typography.xaml`.
- Keep spacing, padding, and corner radius tokens in `Resources/AppTheme.Spacing.xaml`.
- Keep drop shadow effects in `Resources/AppTheme.Effects.xaml`.
- Keep reusable control styles and templates in `Resources/AppTheme.Controls.xaml`.

---

## Theme Tokens

Important brush keys and custom groups:

### 1. Core Semantic Brushes
* `PrimaryBrush`: Primary brand color (Mint Green `#FF51F0A8`).
* `PrimaryForegroundBrush`: High-contrast foreground color on primary background (`#FF000000`).
* `SecondaryBrush`: Secondary action color.
* `SecondaryForegroundBrush`: Foreground color on secondary background.
* `BackgroundBrush`: App background.
* `ForegroundBrush`: Core text color.
* `CardBrush` / `CardForegroundBrush`: Color for card borders/panels.
* `PopoverBrush` / `PopoverForegroundBrush`: Dropdown popup and menu backgrounds.
* `MutedBrush` / `MutedForegroundBrush`: Soft background and tertiary/disabled text.
* `AccentBrush` / `AccentForegroundBrush`: Hover state highlight backgrounds and text.
* `BorderBrush`: Main line border color.
* `InputBrush`: Form control input background.
* `RingBrush`: Focus state border rings.

### 2. Sidebar-Specific Brushes
* `SidebarBrush`: Background color of the navigation panel.
* `SidebarForegroundBrush`: Navigation link text.
* `SidebarPrimaryBrush`: Active navigation marker color.
* `SidebarAccentBrush`: Hover background for links.
* `SidebarBorderBrush`: Vertical divider border.

### 3. Chart-Specific Brushes
* `Chart1Brush` to `Chart5Brush`: Standard color swatches for charts and visualizations.

---

## Corner Radius & Spacing (Design Tokens)

Spacing and bo-goc values are derived from `1rem = 16px`:
* `RadiusSm`: `18` (used for small items, CheckBoxes, or inner borders)
* `RadiusMd`: `20` (used for Buttons, TextBoxes, ComboBoxes)
* `RadiusLg`: `22` (used for Cards, Forms, popups)
* `RadiusXl`: `26` (used for large layout panels)
* `SpacingXs`: `4` (base padding)
* `SpacingSm`: `8`
* `SpacingMd`: `12`
* `SpacingLg`: `16`
* `SpacingXl`: `24`
* `ControlPadding`: `14,10`
* `CardPadding`: `20`

---

## Drop Shadow Performance Rules

WPF `DropShadowEffect` is rendered via software/hardware acceleration, but excessive usage (especially on virtualized lists or grids) severely reduces rendering performance:
* **Rule:** Always set `x:Shared="False"` on all `DropShadowEffect` resources. This forces WPF to create a new instance of the effect every time it is referenced, avoiding `XamlParseException` runtime crashes.
* **Avoid:** Do not attach effects to repeating elements like `DataGridRow` or `ListBoxItem`.
* **Alternatives:** For Dark Mode depth, prefer border contrast (`BorderBrush` slightly lighter than background) or background brightness shifting rather than shadows.

---

## Reusable Components (shadcn/ui-style)

WPF supports reusable, custom-styled controls similar to Next.js/React shadcn components. 

### Custom Control Guidelines
1. **Extend Base Controls:** When creating a component (e.g. `ModernButton`), inherit from the closest native WPF control (`Button`).
2. **Expose Dependency Properties:** Declare properties (e.g. `Variant`, `Icon`, `CornerRadius`) in C# as `DependencyProperty` to support MVVM data binding.
3. **ControlTemplate Triggers:** Manage visual state transitions (hover, focus, variants) inside XAML `<ControlTemplate.Triggers>` instead of writing C# code-behind.

---

## Coding Rules & Examples (Quy tắc Viết mã)

### Quy tắc 1: Không được viết cứng (hardcode) mã màu trong XAML
* **Lý do:** Làm hỏng cơ chế chuyển đổi giao diện động (Light/Dark Theme).

```xml
<!-- ❌ SAI (ANTI-PATTERN): Hardcode mã màu -->
<Border Background="#FFFDFDFD" BorderBrush="#FFF4F6F8" />
<Button Foreground="White" Background="#FF51F0A8" />

<!--  ĐÚNG (BEST PRACTICE): Sử dụng DynamicResource -->
<Border Background="{DynamicResource CardBrush}" BorderBrush="{DynamicResource BorderBrush}" />
<Button Foreground="{DynamicResource PrimaryForegroundBrush}" Background="{DynamicResource PrimaryBrush}" />
```

### Quy tắc 2: Phải dùng DynamicResource cho các tài nguyên liên từ điển (Cross-dictionary)
* **Lý do:** Tránh lỗi biên dịch hoặc lỗi khởi chạy `DependencyProperty.UnsetValue` khi các tệp XAML được phân tích độc lập.

```xml
<!-- ❌ SAI (ANTI-PATTERN): StaticResource trỏ tới key ở file khác -->
<Style TargetType="Button">
    <Setter Property="CornerRadius" Value="{StaticResource RadiusMd}" />
    <Setter Property="Padding" Value="{StaticResource ControlPadding}" />
</Style>

<!--  ĐÚNG (BEST PRACTICE): Dùng DynamicResource để trì hoãn phân tích đến runtime -->
<Style TargetType="Button">
    <Setter Property="CornerRadius" Value="{DynamicResource RadiusMd}" />
    <Setter Property="Padding" Value="{DynamicResource ControlPadding}" />
</Style>
```

### Quy tắc 3: Thiết kế Custom Components đồng bộ thuộc tính
* **Ví dụ mẫu:** Sử dụng Custom Component `ModernButton` để chuyển đổi Variant nhanh gọn.

```xml
<!-- ❌ SAI (ANTI-PATTERN): Dùng Style tĩnh khác nhau cho mỗi nút -->
<Button Style="{StaticResource DestructiveButtonStyle}" Content="Delete" />
<Button Style="{StaticResource PrimaryButtonStyle}" Content="Save" />

<!--  ĐÚNG (BEST PRACTICE): Dùng Custom Component với thuộc tính Variant -->
<components:ModernButton Variant="Destructive" Content="Delete" />
<components:ModernButton Variant="Primary" Content="Save" />
```

### Quy tắc 4: Đồng bộ trạng thái Checked của ToggleSwitch mượt mà
* Khi dùng `ToggleSwitch` để chuyển trạng thái logic (như bật tắt theme), luôn hủy đăng ký (unsubscribe) và đăng ký lại (subscribe) sự kiện trong code C# để tránh vòng lặp đệ quy sự kiện.

```csharp
//  ĐÚNG (BEST PRACTICE)
private void RefreshThemeMenu()
{
    ThemeToggleSwitch.Checked -= OnThemeToggleChecked;
    ThemeToggleSwitch.Unchecked -= OnThemeToggleUnchecked;

    ThemeToggleSwitch.IsChecked = _themeService.CurrentResolvedTheme == ResolvedTheme.Dark;

    ThemeToggleSwitch.Checked += OnThemeToggleChecked;
    ThemeToggleSwitch.Unchecked += OnThemeToggleUnchecked;
}
```
