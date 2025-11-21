using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battleships.MVVM.Enums;
using Battleships.MVVM.Factories;
using Battleships.MVVM.Model.DataTransferObjects;
using Battleships.MVVM.Services;
using Battleships.MVVM.Structs;
using Battleships.MVVM.ViewModel;

namespace Battleships.MVVM.Model
{
    /// <summary>
    /// Defines a contract for game management during a Battleships game. This interface provides methods for 
    /// processing a given player shot, which includes keeping track of turns and running computer moves.
    /// </summary>
    public interface IGame
    {
        AttackStatusReport ProcessPlayerShotSelection(int gridPosition, ShotType selectedShot);
        GameDTO GetDTO();
    }

    /// <summary>
    /// This class implements the <see cref="IGame"/> interface and is used for managing turn taking and shot 
    /// selection during a Battleships game. It includes two <see cref="Board"/> instances to model the player 
    /// and computer boards as well as a <see cref="ComputerPlayer"/> to manage computer shot selection. It 
    /// provides methods to process player shot selection, keeping track of turns, and to run the computer 
    /// moves. It enables interaction with an outside class to provide a user interface for the player to 
    /// enter shot selections and receive updates.
    /// </summary>
    public abstract class Game : IGame
    {
        #region Constants
        protected const int AirstrikeRequiredHits = 5;
        protected const int BombardmentRequiredHits = 7;
        #endregion //Constants

        #region Fields
        protected readonly IEventLogger _eventLogger;
        protected readonly Board _playerBoard;
        protected readonly Board _computerBoard;
        protected readonly ComputerPlayer _computerPlayer;
        protected readonly GameDifficulty _gameDifficulty;

        protected readonly HashSet<int> _validPositions;

        protected readonly bool _airstrikeAllowed;
        protected readonly bool _bombardmentAllowed;
        private readonly bool _hideSunkShips;
        private readonly bool _shipsCanTouch;

        protected int _shotsRemaining;
        protected bool _isPlayerTurn;
        protected int _airstrikeHitCount;
        protected int _bombardmentHitCount;

        protected SingleTurnReport _lastComputerMove;
        #endregion //Fields

        #region Events
        public event EventHandler<AttackStatusReport>? ComputerOpeningMoveCompleted;
        #endregion //Events

        #region Properties
        public int HitCountAirstrike 
        {
            get => _airstrikeHitCount;
            set
            {
                if (value != _airstrikeHitCount)
                {
                    _airstrikeHitCount = value;
                }
            }
        }
        public int HitCountBombardment
        {
            get => _bombardmentHitCount;
            set
            {
                if (value != _bombardmentHitCount)
                {
                    _bombardmentHitCount = value;
                }
            }
        }
        #endregion //Properties

        /// <summary>
        /// Primary constructor creates instance using <see cref="GameSetUpInformation"/> as part of the new 
        /// game set up.
        /// </summary>
        /// <param name="information">A <see cref="GameSetUpInformation"/> instance containing the information 
        /// needed to set up a new game.</param>
        protected Game(ILoggerFactory loggerFactory, GameSetUpInformation information)
        {
            _eventLogger = loggerFactory.CreateLogger(nameof(Game));
            
            _gameDifficulty = information.Difficulty;
            _airstrikeAllowed = information.AirstrikeAllowed;
            _bombardmentAllowed = information.BombardmentAllowed;
            _hideSunkShips = information.HideSunkShips;
            _shipsCanTouch = information.ShipsCanTouch;

            _playerBoard = new Board(information, true);
            _computerBoard = new Board(information, false);
            _computerPlayer = new ComputerPlayer(information);

            _validPositions = [.. Enumerable.Range(0, 100)];

            _isPlayerTurn = information.PlayerStarts;
            HitCountAirstrike = 0;
            HitCountBombardment = 0;
            _shotsRemaining = CalculateShotsThisTurn();

            _lastComputerMove = new SingleTurnReport();

            // Opening computer move will be called from PlayGameViewModel if required.
        }

        /// <summary>
        /// <see cref="GameDTO"/> used to load previous game state data during the load game process.
        /// </summary>
        /// <param name="gameDTO">A <see cref="GameDTO"/> containing loaded information 
        /// to return to a previous game state.</param>
        public Game(ILoggerFactory loggerFactory, GameDTO gameDTO)
        {
            _eventLogger = loggerFactory.CreateLogger(nameof(Game));
            
            _playerBoard = new Board(gameDTO.PlayerBoardDTO);
            _computerBoard = new Board(gameDTO.ComputerBoardDTO);
            _computerPlayer = new ComputerPlayer(gameDTO.ComputerPlayerDTO);
            _gameDifficulty = gameDTO.GameDifficulty;
            _airstrikeAllowed = gameDTO.AirstrikeAllowed;
            _bombardmentAllowed = gameDTO.BombardmentAllowed;
            _shotsRemaining = gameDTO.ShotsRemaining;
            _isPlayerTurn = gameDTO.IsPlayerTurn;
            _airstrikeHitCount = gameDTO.AirstrikeHitCount;
            _bombardmentHitCount = gameDTO.BombardmentHitCount;
            _lastComputerMove = gameDTO.LastComputerMove;

            _validPositions = [.. Enumerable.Range(0, 100)];
        }

        #region Handling Moves Methods
        /// <summary>
        /// Handles a player shot selection and returns a report on the outcome of that shot and any subsequent 
        /// computer moves. Updates the turn taking boolean and any multi-target shot counters.
        /// </summary>
        /// <param name="gridPosition">The targeted position on the grid.</param>
        /// <param name="shotType">The selected <see cref="ShotType"/>.</param>
        /// <returns>An <see cref="AttackStatusReport"/> containing information about the outcome of the shot.</returns>
        /// <remarks>Either processes one player shot and waits for the next selection or runs any subsequent 
        /// computer moves if the player turn is over.</remarks>
        public AttackStatusReport ProcessPlayerShotSelection(int gridPosition, ShotType shotType)
        {
            try
            {
                if (!_validPositions.Contains(gridPosition))
                    throw new InvalidOperationException("Invalid player shot position selected.");

                var turnReportList = new List<SingleTurnReport>();

                if (!_isPlayerTurn)
                {
                    _isPlayerTurn = true;
                    _shotsRemaining = CalculateShotsThisTurn(); //Only needed at the start of a player turn
                }

                var turnReport = GenerateTurnReport(gridPosition, shotType, _computerBoard);

                turnReportList.Add(turnReport);

                HitCountAirstrike += turnReport.PositionsHit.Count;
                HitCountBombardment += turnReport.PositionsHit.Count;

                if (turnReport.IsGameOver)
                    return new AttackStatusReport(true, turnReportList);

                UpdateShotsRemainingThisTurn(turnReport.ShotOutcome, turnReport.ShipsSunk.Count);

                if (_shotsRemaining == 0)
                {
                    _isPlayerTurn = false;
                    var computerTurnReports = RunComputerMove(out bool gameOver);
                    turnReportList.AddRange(computerTurnReports);

                    if (gameOver)
                        return new AttackStatusReport(true, turnReportList);
                }

                return new AttackStatusReport(false, turnReportList);
            }
            catch (InvalidOperationException)
            {
                _eventLogger.LogWarning("Player able to enter invalid shot position.");
                throw;
            }
        }

        /// <summary>
        /// Generates a list of <see cref="SingleTurnReport"/> instances containing information about each 
        /// computer shot in a turn. Ensures the correct number of shots are taken.
        /// </summary>
        /// <param name="gameOver">A boolean value indicating if the game is over.</param>
        /// <returns>A list of <see cref="SingleTurnReport"/> instances with information about each shot 
        /// outcome.</returns>
        /// <remarks>Called from the ProcessPlayerShotSelection method at the end of a player turn to run the 
        /// complete comouter turn. The <see cref="SingleTurnReport"/> instances are compiled into an <see 
        /// cref="AttackStatusReport"/> along with the player move.</remarks>
        internal List<SingleTurnReport> RunComputerMove(out bool gameOver)
        {
            var reportsList = new List<SingleTurnReport>();
            gameOver = false;

            _shotsRemaining = CalculateShotsThisTurn();

            while (_shotsRemaining > 0)
            {
                var targetGridCell = _computerPlayer.ChooseNextMove(_playerBoard.Grid, _playerBoard.DeclareRemainingShipSizes(), _lastComputerMove, out ShotType shotType);

                var turnReport = GenerateTurnReport(targetGridCell, shotType, _playerBoard);
                reportsList.Add(turnReport);
                _lastComputerMove = turnReport;

                gameOver = turnReport.IsGameOver;

                if (gameOver) return reportsList;

                UpdateShotsRemainingThisTurn(turnReport.ShotOutcome, turnReport.ShipsSunk.Count);
            }
            
            return reportsList;
        }

        /// <summary>
        /// Runs a computer move on start up if the player does not start the game. Invokes an action to allow 
        /// the calling class to access the <see cref="AttackStatusReport"/>.
        /// </summary>
        public void PlayComputerOpeningMove()
        {
            var turnReports = RunComputerMove(out bool gameOver);
            var attackReport = new AttackStatusReport(gameOver, turnReports);

            ComputerOpeningMoveCompleted?.Invoke(this, attackReport);
        }

        /// <summary>
        /// Calls the GenerateSingleTurnReport() method from the board class to process the selected shot and 
        /// return a <see cref="SingleTurnReport"/> containing the result of the attack.
        /// </summary>
        /// <param name="gridPosition">An integer value representing the grid position attacked.</param>
        /// <param name="shotType">The type of shot selected.</param>
        /// <param name="board">The board that the shot targets.</param>
        /// <returns>A <see cref="SingleTurnReport"/> containing information about the result of the shot.</returns>
        /// <remarks>Note that the player targets the computer board and vice versa.</remarks>
        internal SingleTurnReport GenerateTurnReport(int gridPosition, ShotType shotType, Board board)
        {
            return board.GenerateSingleTurnReport(gridPosition, shotType);
        }

        /// <summary>
        /// Uses information from the appropriate <see cref="Board"/> instance to calculate how many shots the 
        /// player or computer has for their turn based on the game type. In the base class, this is set to one 
        /// shot per turn for a standard game of Battleships.
        /// </summary>
        /// <returns>An integer representing the number of shots remaining at the start of the turn.</returns>
        protected virtual int CalculateShotsThisTurn()
        {
            return 1;
        }

        /// <summary>
        /// Updates the total <see cref="ShotsRemaining"/> to check whether or not the turn is over.
        /// </summary>
        protected virtual void UpdateShotsRemainingThisTurn(ShotOutcome shotOutcome, int shipsSunk)
        {
            _shotsRemaining--;
        }
     
        #endregion //Handling Moves Methods

        /// <summary>
        /// Uses current <see cref="Game"/> data to create a <see cref="GameDTO"/> for game state persistance.
        /// </summary>
        /// <returns>A <see cref="GameDTO"/> containing current game data.</returns>
        public virtual GameDTO GetDTO()
        {
            return new GameDTO
            {
                PlayerBoardDTO = _playerBoard.GetDTO(),
                ComputerBoardDTO = _computerBoard.GetDTO(),
                ComputerPlayerDTO = _computerPlayer.GetDTO(),
                GameDifficulty = _gameDifficulty,
                AirstrikeAllowed = _airstrikeAllowed,
                BombardmentAllowed = _bombardmentAllowed,
                HideSunkShips = _hideSunkShips,
                ShipsCanTouch = _shipsCanTouch,
                ShotsRemaining = _shotsRemaining,
                IsPlayerTurn = _isPlayerTurn,
                AirstrikeHitCount = _airstrikeHitCount,
                BombardmentHitCount = _bombardmentHitCount,
                LastComputerMove = _lastComputerMove
            };
        }
    }
}