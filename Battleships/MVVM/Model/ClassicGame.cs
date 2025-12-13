using Battleships.MVVM.Enums;
using Battleships.MVVM.Factories;
using Battleships.MVVM.Model.DataTransferObjects;
using Battleships.MVVM.Structs;

namespace Battleships.MVVM.Model
{
    public class ClassicGame : Game
    {
        private readonly bool _fireUntilMiss;
        private readonly bool _bonusShotIfSunk;

        public ClassicGame(ILoggerFactory loggerFactory, GameSetUpInformation information) : base(loggerFactory, information)
        {
            _fireUntilMiss = information.FireUntilMiss;
            _bonusShotIfSunk = information.BonusShotIfSunk;
        }

        public ClassicGame(ILoggerFactory loggerFactory, GameDTO gameDTO) : base(loggerFactory, gameDTO)
        {
            _fireUntilMiss = gameDTO.FireUntilMiss;
            _bonusShotIfSunk = gameDTO.BonusShotIfSunk;
        }

        /// <summary>
        /// Override method. Uses the results of the previous shot to update the number of shots remaining in 
        /// a turn.
        /// </summary>
        /// <param name="shotOutcome">The result of the last shot.</param>
        /// <param name="shipsSunk">The number of ships sunk in the last turn.</param>
        protected override void UpdateShotsRemainingThisTurn(ShotOutcome shotOutcome, int shipsSunk)
        {
            _shotsRemaining--;

            if (_bonusShotIfSunk)
            {
                _shotsRemaining += shipsSunk;
            }

            if (_fireUntilMiss && shotOutcome != ShotOutcome.Miss && _shotsRemaining == 0)
                _shotsRemaining = 1;
        }

        /// <summary>
        /// Uses current <see cref="Game"/> data to create a <see cref="GameDTO"/> for game state persistance. 
        /// Includes the fire until miss and bonus shot if sunk boolean values for a <see cref="ClassicGame"/>..
        /// </summary>
        /// <returns>A <see cref="GameDTO"/> containing current game data.</returns>
        public override GameDTO GetDTO()
        {
            var dto = base.GetDTO();

            dto.GameType = GameType.Classic;
            dto.FireUntilMiss = _fireUntilMiss;
            dto.BonusShotIfSunk = _bonusShotIfSunk;

            return dto;
        }
    }
}
