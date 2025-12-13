using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using Battleships.MVVM.View;

namespace Battleships.MVVM.Converters
{
    public class ViewToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;

            if (value is UserControl view)
                return view is PlayGameView;

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
