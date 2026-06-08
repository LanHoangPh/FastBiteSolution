using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FastBiteGroup.Desktop.UI.Views.Components;

public enum AvatarSize
{
    Small,
    Medium,
    Large,
    ExtraLarge
}

public class Avatar : Control
{
    public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
        nameof(ImageSource),
        typeof(ImageSource),
        typeof(Avatar),
        new PropertyMetadata(null));

    public ImageSource? ImageSource
    {
        get => (ImageSource?)GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    public static readonly DependencyProperty InitialsProperty = DependencyProperty.Register(
        nameof(Initials),
        typeof(string),
        typeof(Avatar),
        new PropertyMetadata(string.Empty));

    public string Initials
    {
        get => (string)GetValue(InitialsProperty);
        set => SetValue(InitialsProperty, value);
    }

    public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
        nameof(Size),
        typeof(AvatarSize),
        typeof(Avatar),
        new PropertyMetadata(AvatarSize.Medium));

    public AvatarSize Size
    {
        get => (AvatarSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
        nameof(Status),
        typeof(BadgeStatus?),
        typeof(Avatar),
        new PropertyMetadata(null));

    public BadgeStatus? Status
    {
        get => (BadgeStatus?)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    static Avatar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Avatar),
            new FrameworkPropertyMetadata(typeof(Avatar)));
    }
}
