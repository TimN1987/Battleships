using Battleships.MVVM.Enums;
using Battleships.MVVM.Model.DataTransferObjects;

namespace Battleships.MVVM.Model
{
    public class Destroyer : Ship, IShip
    {
        public override ShipType ShipType => ShipType.Destroyer;

        public Destroyer((int gridPosition, bool isHorizontal) startPosition) : base(startPosition, 2)
        {

        }

        public Destroyer(ShipDTO shipDTO) : base(shipDTO) { }
    }
}
