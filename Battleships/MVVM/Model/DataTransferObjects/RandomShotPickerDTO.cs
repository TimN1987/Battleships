using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleships.MVVM.Model.DataTransferObjects
{
    
    /// <summary>
    /// A class for storing <see cref="RandomShotPicker"/> state information to enable simple Json serialization 
    /// and data persistance.
    /// </summary>
    public class RandomShotPickerDTO
    {
        public List<int> AvailableShots { get; set; } = [];
        public List<int> AvailableDiagonalSpacingTwoShots { get; set; } = [];
        public List<int> AvailableDiagonalSpacingThreeShots { get; set; } = [];
        public List<int> AvailableDiagonalSpacingFourShots { get; set; } = [];
        public List<int> AvailableDiagonalSpacingFiveShots { get; set; } = [];
    }
}
