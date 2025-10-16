using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battleships.MVVM.Enums;
using Battleships.MVVM.Model.DataTransferObjects;

namespace Battleships.MVVM.Model
{
    public class Submarine : Ship, IShip
    {
        public override ShipType ShipType => ShipType.Submarine;

        public Submarine((int gridPosition, bool isHorizontal) startPosition) : base(startPosition, 3)
        {
            
        }

        public Submarine(ShipDTO shipDTO) : base(shipDTO) { }
    }
}
