using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Battleships.MVVM.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return Visibility.Collapsed;

        if (value is bool isGameInProgress && bool.TryParse(parameter.ToString(), out bool isReturnToGame))
        {
            if (isGameInProgress && isReturnToGame)
                return Visibility.Visible;
            else if (isGameInProgress || isReturnToGame)
                return Visibility.Collapsed;
            else
                return Visibility.Visible;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
