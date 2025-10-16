using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battleships.MVVM.Enums;

namespace Battleships.MVVM.Structs
{
    public readonly struct SingleTurnReport
    {
        private readonly int _hits;

        public bool FirstTurn { get; }
        public IReadOnlyList<int> PositionsHit { get; }
        public IReadOnlyList<int> PositionsMissed { get; }
        public IReadOnlyList<(int startPosition, bool isHorizontal, ShipType shipType)> ShipsSunk { get; }
        public bool IsGameOver { get; }

        public SingleTurnReport()
        {
            FirstTurn = true;
            
            PositionsHit = [];
            PositionsMissed = [];
            ShipsSunk = [];
            IsGameOver = false;

            _hits = PositionsHit.Count;
        }

        public SingleTurnReport(
            IReadOnlyList<int> positionsHit,
            IReadOnlyList<int> positionsMissed,
            IReadOnlyList<(int startPosition, bool isHorizontal, ShipType shipType)> shipsSunk,
            bool isGameOver)
        {
            FirstTurn = false;
            
            PositionsHit = positionsHit ?? [];
            PositionsMissed = positionsMissed ?? [];
            ShipsSunk = shipsSunk ?? new List<(int, bool, ShipType)>();
            IsGameOver = isGameOver;

            _hits = PositionsHit.Count;
        }

        public ShotOutcome ShotOutcome
        {
            get
            {
                return _hits switch
                {
                    0 => ShotOutcome.Miss,
                    1 => ShotOutcome.Hit,
                    _ => ShotOutcome.MultipleHit,
                };
            }
        }
    }
}
