using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FastBiteGroup.Desktop.UI.Views.Components;

public class FormTextBox : TextBox
{
    public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(
        nameof(PlaceholderText),
        typeof(string),
        typeof(FormTextBox),
        new PropertyMetadata(""));

    public string PlaceholderText
    {
        get => (string)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon),
        typeof(Geometry),
        typeof(FormTextBox),
        new PropertyMetadata(null));

    public Geometry Icon
    {
        get => (Geometry)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
        nameof(CornerRadius),
        typeof(CornerRadius),
        typeof(FormTextBox),
        new PropertyMetadata(new CornerRadius(8)));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    static FormTextBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(FormTextBox),
            new FrameworkPropertyMetadata(typeof(FormTextBox)));
    }
}
