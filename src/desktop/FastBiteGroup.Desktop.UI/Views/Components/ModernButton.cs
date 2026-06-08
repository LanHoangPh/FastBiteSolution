using System.Windows;
using System.Windows.Controls;

namespace FastBiteGroup.Desktop.UI.Views.Components;

public enum ButtonVariant
{
    Primary,
    Secondary,
    Outline,
    Ghost,
    Destructive
}

public enum IconPosition
{
    Left,
    Right
}

public class ModernButton : Button
{
    public static readonly DependencyProperty VariantProperty = DependencyProperty.Register(
        nameof(Variant),
        typeof(ButtonVariant),
        typeof(ModernButton),
        new PropertyMetadata(ButtonVariant.Primary));

    public ButtonVariant Variant
    {
        get => (ButtonVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon),
        typeof(object),
        typeof(ModernButton),
        new PropertyMetadata(null));

    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly DependencyProperty IconPositionProperty = DependencyProperty.Register(
        nameof(IconPosition),
        typeof(IconPosition),
        typeof(ModernButton),
        new PropertyMetadata(IconPosition.Left));

    public IconPosition IconPosition
    {
        get => (IconPosition)GetValue(IconPositionProperty);
        set => SetValue(IconPositionProperty, value);
    }

    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
        nameof(CornerRadius),
        typeof(CornerRadius),
        typeof(ModernButton),
        new PropertyMetadata(new CornerRadius(8))); // Default bo góc 8px

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    static ModernButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ModernButton),
            new FrameworkPropertyMetadata(typeof(ModernButton)));
    }
}
