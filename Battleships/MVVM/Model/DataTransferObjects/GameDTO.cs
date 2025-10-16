using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battleships.MVVM.Enums;
using Battleships.MVVM.Structs;

namespace Battleships.MVVM.Model.DataTransferObjects
{

    /// <summary>
    /// A class for storing <see cref="Game"/> state information to enable simple Json serialization 
    /// and data persistance.
    /// </summary>
    public class GameDTO
    {
        public GameType GameType { get; set; } = GameType.Classic;
        public BoardDTO PlayerBoardDTO { get; set; } = new BoardDTO();
        public BoardDTO ComputerBoardDTO { get; set; } = new BoardDTO();
        public ComputerPlayerDTO ComputerPlayerDTO { get; set; } = new ComputerPlayerDTO();
        public GameDifficulty GameDifficulty { get; set; } = GameDifficulty.Easy;
        public bool AirstrikeAllowed { get; set; } = false;
        public bool BombardmentAllowed { get; set; } = false;
        public bool HideSunkShips { get; set; } = false;
        public int ShotsRemaining { get; set; } = 0;
        public bool IsPlayerTurn { get; set; } = false;
        public int AirstrikeHitCount { get; set; } = 0;
        public int BombardmentHitCount { get; set; } = 0;
        public SingleTurnReport LastComputerMove { get; set; } = new SingleTurnReport();
        public bool FireUntilMiss {get; set; } = false;
        public bool BonusShotIfSunk { get; set; } = false;
        public SalvoShots SalvoShotType { get; set; } = SalvoShots.None;
    }
}
