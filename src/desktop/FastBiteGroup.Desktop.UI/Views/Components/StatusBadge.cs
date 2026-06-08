using System.Windows;
using System.Windows.Controls;

namespace FastBiteGroup.Desktop.UI.Views.Components;

public enum BadgeStatus
{
    Neutral,
    Success,
    Warning,
    Error,
    Online,
    Away,
    Busy,
    Offline
}

public class StatusBadge : Control
{
    public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
        nameof(Status),
        typeof(BadgeStatus),
        typeof(StatusBadge),
        new PropertyMetadata(BadgeStatus.Neutral));

    public BadgeStatus Status
    {
        get => (BadgeStatus)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    static StatusBadge()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(StatusBadge),
            new FrameworkPropertyMetadata(typeof(StatusBadge)));
    }
}
