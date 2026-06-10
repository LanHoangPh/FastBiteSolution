using System;
using System.Globalization;
using System.Windows.Data;
using FastBiteGroup.Desktop.UI.Views.Components;

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
        throw new NotImplementedException();
    }
}
