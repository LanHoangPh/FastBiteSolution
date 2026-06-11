using FastBiteGroup.Desktop.UI.Views.Components;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FastBiteGroup.Desktop.UI.Converters;

public class BooleanToBadgeStatusConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isOnline)
        {
            return isOnline ? BadgeStatus.Online : null;
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return DependencyProperty.UnsetValue;
    }
}
