using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battleships.MVVM.Enums;
using Battleships.MVVM.Model.DataTransferObjects;

namespace Battleships.MVVM.Model
{
    public class Cruiser : Ship
    {
        public override ShipType ShipType => ShipType.Cruiser;

        public Cruiser((int gridPosition, bool isHorizontal) startPosition) : base(startPosition, 3)
        {
            
        }

        public Cruiser(ShipDTO shipDTO) : base(shipDTO) { }
    }
}
