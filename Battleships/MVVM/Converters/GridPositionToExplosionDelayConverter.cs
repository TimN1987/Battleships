using Battleships.MVVM.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Battleships.MVVM.Converters;

public class GridPositionToExplosionDelayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int position && parameter is bool isDelay)
        {
            int row = position / 10;
            int col = position % 10;

            // Calculations based on tests to find corners of the computer grid.
            // Estimated at (122,70) to (358,300).
            // Aim to calculate a good estimate for the centre of the target cell.

            double targetTop = 81.5 + 23 * row;
            double targetLeft = 133.8 + 23.6 * col + 100;

            // The bomber travels 950 on the y-axis (from 550 to -400). It starts at 0 on the x-axis.
            // The bomber should travel through the target position.
            // Calculate what fraction of the journey has been made to the target position - use this to 
            // calculate what the end position should be.

            double fractionToTarget = (550 - targetTop) / 950;

            // The animation time is 2s. The delay is 2s * fractionToTarget. The run time is the rest of the 2s.
            double returnTime = isDelay ? fractionToTarget * 2.0 : 2.0 - (fractionToTarget * 2.0);
            return TimeSpan.FromSeconds(returnTime);
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
