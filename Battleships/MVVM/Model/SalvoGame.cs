using Battleships.MVVM.Enums;
using Battleships.MVVM.Factories;
using Battleships.MVVM.Model.DataTransferObjects;
using Battleships.MVVM.Structs;

namespace Battleships.MVVM.Model
{
    public class SalvoGame : Game
    {
        private const int FixedSalvoShotsTotal = 5;
        private readonly SalvoShots _salvoShotType;

        public SalvoGame(ILoggerFactory loggerFactory, GameSetUpInformation information) : base(loggerFactory, information)
        {
            _salvoShotType = information.SalvoShotType;
        }

        public SalvoGame(ILoggerFactory loggerFactory, GameDTO gameDTO) : base(loggerFactory, gameDTO)
        {
            _salvoShotType = gameDTO.SalvoShotType;
        }

        /// <summary>
        /// Override method. Uses the <see cref="SalvoShots"/> type and <see cref="Board"/> information to 
        /// calculate the correct number of shots for the turn in a <see cref="SalvoGame"/>.
        /// </summary>
        /// <returns>The number of shots for the turn.</returns>
        protected override int CalculateShotsThisTurn()
        {
            if (_salvoShotType == SalvoShots.Fixed)
                return FixedSalvoShotsTotal;

            return (_isPlayerTurn) ? _playerBoard.CheckShipStatusToCalculateShots(_salvoShotType)
                : _computerBoard.CheckShipStatusToCalculateShots(_salvoShotType);
        }

        /// <summary>
        /// Uses current <see cref="Game"/> data to create a <see cref="GameDTO"/> for game state persistance. 
        /// Includes the <see cref="SalvoShots"/> type for a <see cref="SalvoGame"/>.
        /// </summary>
        /// <returns>A <see cref="GameDTO"/> containing current game data.</returns>
        public override GameDTO GetDTO()
        {
            var dto = base.GetDTO();

            dto.GameType = GameType.Salvo;
            dto.SalvoShotType = _salvoShotType;

            return dto;
        }
    }
}
