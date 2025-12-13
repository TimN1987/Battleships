using System.Globalization;
using System.Windows.Data;
using Battleships.MVVM.Enums;

namespace Battleships.MVVM.Converters
{
    public class ShotTypeToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;

            if (value is ShotType selectedShotType && parameter is ShotType buttonShotType)
                return selectedShotType == buttonShotType;

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
                return ShotType.Single;

            if (value is bool isSelected && parameter is ShotType buttonShotType)
                return isSelected ? buttonShotType : ShotType.Single;

            return ShotType.Single;
        }
    }
}
