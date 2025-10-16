using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battleships.MVVM.Enums;
using Battleships.MVVM.ViewModel;

namespace Battleships.MVVM.Structs
{
    /// <summary>
    /// Used to store game set up information as it is collected in different views before the Game instance 
    /// is created in the <see cref="PlayGameViewModel"/>./>
    /// </summary>
    public struct GameSetUpInformation(
        GameType gameType,
        GameDifficulty difficulty,
        bool playerStarts,
        bool shipsCanTouch,
        bool airstrikeAllowed,
        bool bombardmentAllowed,
        bool fireUntilMiss,
        bool bonusShotIfSunk,
        bool hideSunkShips,
        SalvoShots salvoShotType,
        Dictionary<ShipType, (int, bool)> shipPositions)
    {
        //General Game Properties
        public GameType Type { get; set; } = gameType;
        public GameDifficulty Difficulty { get; set; } = difficulty;
        public Dictionary<ShipType, (int, bool)> ShipPositions { get; set; } = shipPositions;
        public bool PlayerStarts { get; set; } = playerStarts;
        public bool ShipsCanTouch { get; set; } = shipsCanTouch;
        public bool AirstrikeAllowed { get; set; } = airstrikeAllowed;
        public bool BombardmentAllowed { get; set; } = bombardmentAllowed;

        //Classic Game Properties
        public bool FireUntilMiss { get; set; } = fireUntilMiss;
        public bool BonusShotIfSunk { get; set; } = bonusShotIfSunk;
        public bool HideSunkShips { get; set; } = hideSunkShips;

        //Salvo Game Properties
        public SalvoShots SalvoShotType { get; set; } = salvoShotType;
    }
}
