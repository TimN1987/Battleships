using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battleships.MVVM.Enums;
using Battleships.MVVM.Model.DataTransferObjects;

namespace Battleships.MVVM.Model
{
    public class Carrier : Ship
    {
        public override ShipType ShipType => ShipType.Carrier;

        public Carrier((int gridPosition, bool isHorizontal) startPosition) : base(startPosition, 5)
        {
            
        }

        public Carrier(ShipDTO shipDTO) : base(shipDTO) { }

    }
}
