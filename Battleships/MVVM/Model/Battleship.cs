using Battleships.MVVM.Enums;
using Battleships.MVVM.Model.DataTransferObjects;

namespace Battleships.MVVM.Model
{
    public class Battleship : Ship
    {
        public override ShipType ShipType => ShipType.Battleship;

        public Battleship((int gridPosition, bool isHorizontal) startPosition) : base(startPosition, 4) { }

        public Battleship(ShipDTO shipDTO) : base(shipDTO) { }

    }
}
