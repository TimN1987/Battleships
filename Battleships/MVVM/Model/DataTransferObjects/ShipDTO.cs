using Battleships.MVVM.Enums;

namespace Battleships.MVVM.Model.DataTransferObjects
{
    /// <summary>
    /// A class for storing <see cref="Ship"/> state information to enable simple Json serialization 
    /// and data persistance.
    /// </summary>
    public class ShipDTO
    {
        public ShipType ShipType { get; set; } = ShipType.Ship;
        public int Size { get; set; } = 0;
        public bool[] Damage { get; set; } = [];
        public int[] Positions { get; set; } = [];
        public bool IsHorizontal { get; set; } = false;
    }
}
