using Battleships.MVVM.Enums;
using Battleships.MVVM.ViewModel.Base;

namespace Battleships.MVVM.ViewModel.GridCells
{
    public class ShipPlacementGridCell(int row, int column) : GridCellBase
    {
        #region Fields
        private bool _isHighlighted = (row == 0 && column == 0);
        private bool _isOccupied = false;
        private bool _isTouching = false;
        #endregion //Fields

        #region Properties
        public bool IsHighlighted
        {
            get => _isHighlighted;
            set => SetProperty(ref _isHighlighted, value);
        }
        public bool IsOccupied
        {
            get => _isOccupied;
            set => SetProperty(ref _isOccupied, value);
        }
        public bool IsTouching
        {
            get => _isTouching;
            set => SetProperty(ref _isTouching, value);
        }
        public int Row { get; set; } = row;
        public int Column { get; set; } = column;
        public int Index => 10 * Row + Column;
        #endregion //Properties

        /// <summary>
        /// Sets the <see cref="IsOccupied"/>, <see cref="IsTouching"/> and <see cref="IsHighlighted"/> 
        /// properties to the given boolean value <paramref name="isHighlighted"/>. Used to update the 
        /// cell highlighting during ship placement.
        /// </summary>
        /// <param name="highlightingType">The <see cref="CellHighlighting"/> type to be updated.</param>
        /// <param name="isHighlighted">A boolean value indicating the new value for the <see 
        /// cref="CellHighlighting"/>.</param>
        public void SetHighlighting(CellHighlighting highlightingType, bool isHighlighted)
        {
            switch (highlightingType)
            {
                case CellHighlighting.IsHighlighted:
                    IsHighlighted = isHighlighted;
                    break;

                case CellHighlighting.IsOccupied:
                    IsOccupied = isHighlighted;
                    break;

                case CellHighlighting.IsTouching:
                    IsTouching = isHighlighted;
                    break;
            }
        }
    }
}
