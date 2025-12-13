using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Battleships.MVVM.Converters;

public class StandardBooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return Visibility.Collapsed;

        if (value is bool input)
            return input ? Visibility.Visible : Visibility.Collapsed;

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return false;

        if (value is Visibility visibility)
            return visibility == Visibility.Visible;

        return DependencyProperty.UnsetValue;
    }
}
