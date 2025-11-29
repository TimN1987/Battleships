using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Battleships.MVVM.Enums;

namespace Battleships.MVVM.Converters;

public class GridPositionToExplosionPositionConverter : IValueConverter
{
    /// <summary>
    /// Uses the grid position that has been targeted to position the explosion animation, centered over the 
    /// targeted cell.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int position && parameter is ExplosionPosition explosionPosition)
        {
            int row = position / 10;
            int col = position % 10;

            // Calculations based on tests to find corners of the computer grid.
            // Estimated at (122,70) to (358,300).
            // Aim to calculate a good estimate for the centre of the target cell.

            double targetTop = 90 + 23 * row; // Base value adjusted from 81.5 to 90 based on visual tests.
            double targetLeft = 133.8 + 23.6 * col + 100;

            // Explosion starts at size 0x0 at (targetLeft,targetTop).
            // It grows to size 300x300, so reduces targetLeft and targetTop by 150 each to keep centered.
            double targetTopEnd = targetTop - 150;
            double targetLeftEnd = targetLeft - 150;

            double returnPosition = explosionPosition switch
            {
                ExplosionPosition.StartLeft => targetLeft,
                ExplosionPosition.StartTop => targetTop,
                ExplosionPosition.EndLeft => targetLeftEnd,
                ExplosionPosition.EndTop => targetTopEnd,
                _ => throw new NotImplementedException()
            };

            return returnPosition;
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
