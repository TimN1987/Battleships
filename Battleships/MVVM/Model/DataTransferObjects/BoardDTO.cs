using Battleships.MVVM.Enums;

namespace Battleships.MVVM.Model.DataTransferObjects
{
    /// <summary>
    /// A class for storing <see cref="Board"/> state information to enable simple Json serialization 
    /// and data persistance.
    /// </summary>
    public class BoardDTO
    {
        public GridCellState[] Grid { get; set; } = [];
        public ShipDTO[] ShipsDTO { get; set; } = [];
        public bool ShipsCanTouch { get; set; } = false;
    }
}
