using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Battleships.MVVM.Converters;

public class GridPositionToBomberEndPositionConverter : IValueConverter
{
    /// <summary>
    /// Given the grid position for the shot, the converter calculates the end Canvas.Left position, so that 
    /// the bomber passes over the targeted cell.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int position)
        {
            int row = position / 10;
            int col = position % 10;

            // Calculations based on tests to find corners of the computer grid.
            // Estimated at (122,-30) to (358,200).
            // Aim to calculate a good estimate for the centre of the target cell.

            double targetTop = -18.5 + 23 * row;
            double targetLeft = 133.8 + 23.6 * col;

            // The bomber travels 950 on the y-axis (from 550 to -400). It starts at 0 on the x-axis.
            // The bomber should travel through the target position.
            // Calculate what fraction of the journey has been made to the target position - use this to 
            // calculate what the end position should be.

            double fractionToTarget = (550 - targetTop) / 950; // fractionToTarget * endPoint = targetLeft

            return targetLeft / fractionToTarget;
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
