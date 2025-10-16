using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battleships.MVVM.Model;
using Battleships.MVVM.Enums;
using System.Printing;
using Battleships.MVVM.Structs;
using Battleships.MVVM.View;
using Battleships.MVVM.ViewModel;
using System.Diagnostics;

namespace Battleships.MVVM.Services
{
    /// <summary>
    /// Defines a contract for the GameSetUpService. This service is responsible for setting up the game using 
    /// properties that are set during the game setup process. It is also responsible for creating a new Game 
    /// instance for the PlayGameViewModel to access.
    /// </summary>
    public interface IGameSetUpService
    {
        public void OnNewGame();
        public void SetGameType(GameType gameType);
        public GameType GetGameType();
        public void SetDifficulty(GameDifficulty difficulty);
        public void SetGeneralRules(bool airstrikeAllowed, bool bombardmentAllowed, bool shipsCanTouch);
        public void SetClassicRules(bool fireUntilMiss, bool bonusShotOnHit, bool hideSunkShips);
        public void SetSalvoRules(SalvoShots salvoShotsType);
        public bool GetShipsCanTouchValue();
        public void SetShipPlacements(Dictionary<ShipType, (int, bool)> shipPositions);
        public void ProvideGameSetUpInformation();
    }

    /// <summary>
    /// The GameSetUpService is responsible for setting up the game using properties that are set during the 
    /// game setup process. It provides methods to create a new Game instance and to set the ship placements 
    /// for the new Game instance. It also provides properties to access the game type, difficulty, and other 
    /// game settings which are used to instantiate the new Game. It implements the IGameSetUpService interface.
    /// </summary>
    public class GameSetUpService : IGameSetUpService
    {
        #region Fields
        private readonly IEventAggregator _eventAggregator;
        private GameType _gameType;
        private GameDifficulty _difficulty;

        //Classic rules
        private bool _playerStarts;
        private bool _fireUntilMiss;
        private bool _bonusShotOnHit;
        private bool _shipsCanTouch;
        private bool _hideSunkShips;

        //Salvo rules
        private SalvoShots _salvoShotsType;

        //General rules
        private bool _airstrikeAllowed;
        private bool _bombardmentAllowed;
        private Dictionary<ShipType, (int, bool)> _playerShipPositions;
        #endregion //Fields

        public GameSetUpService(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator
                ?? throw new ArgumentNullException(nameof(eventAggregator));

            _eventAggregator.GetEvent<StartGameEvent>().Subscribe(OnNewGame);

            _gameType = GameType.Classic;
            _difficulty = GameDifficulty.Easy;

            _playerStarts = true;
            _fireUntilMiss = false;
            _bonusShotOnHit = false;
            _shipsCanTouch = true;
            _hideSunkShips = false;

            _salvoShotsType = SalvoShots.None;

            _airstrikeAllowed = false;
            _bombardmentAllowed = false;
            _playerShipPositions = [];
        }

        /// <summary>
        /// Called when a NewGameEvent is received. Used to clear the set up information once the 
        /// PlayGameViewModel has stored the Game instance correctly.
        /// </summary>
        public void OnNewGame()
        {
            _gameType = GameType.Classic;
            _difficulty = GameDifficulty.Easy;

            _playerStarts = true;
            _fireUntilMiss = false;
            _bonusShotOnHit = false;
            _shipsCanTouch = true;
            _hideSunkShips = false;

            _salvoShotsType = SalvoShots.None;

            _airstrikeAllowed = false;
            _bombardmentAllowed = false;
            _playerShipPositions = null;
        }

        /// <summary>
        /// Sets the GameType property to the given value based on the user decision.
        /// </summary>
        /// <param name="gameType">The GameType that the user selects.</param>
        public void SetGameType(GameType gameType)
        {
            _gameType = gameType;
        }

        /// <summary>
        /// Returns the <see cref="GameType"/> stored in the _gameType field. Used to navigate to the correct 
        /// rules set up page.
        /// </summary>
        /// <returns>The <see cref="GameType"/> stored in the _gameType field.</returns>
        public GameType GetGameType()
        {
            return _gameType;
        }

        /// <summary>
        /// Sets the GameDifficulty property to the given value based on the user decision.
        /// </summary>
        /// <param name="difficulty">The GameDifficulty that the user selectes.</param>
        public void SetDifficulty(GameDifficulty difficulty)
        {
            _difficulty = difficulty;
        }

        /// <summary>
        /// Sets the general rules for a new game based on user choices in the game rule set up stage. Can 
        /// be called before navigating to the <see cref="ClassicRulesSetUpView"/> or the <see 
        /// cref="SalvoRulesSetUpView"/> to continue game set up.
        /// </summary>
        /// <param name="airstrikeAllowed">A boolean value indicating whther an airstrike is allowed.</param>
        /// <param name="bombardmentAllowed">A boolean value indicating whether a bombardment is allowed.</param>
        /// <param name="shipsCanTouch">A boolean value indicating whether the ships can touch.</param>
        public void SetGeneralRules(bool airstrikeAllowed, bool bombardmentAllowed, bool shipsCanTouch)
        {
            _airstrikeAllowed = airstrikeAllowed;
            _bombardmentAllowed = bombardmentAllowed;
            _shipsCanTouch = shipsCanTouch;
        }

        /// <summary>
        /// Sets the CLassic Rules values based on user choices. Passed to the <see cref="PlayGameViewModel"/> 
        /// in the <see cref="GameSetUpInformation"/> struct.
        /// </summary>
        /// <param name="fireUntilMiss">A boolean value indicating whether the players will continue to 
        /// fire until they miss.</param>
        /// <param name="bonusShotOnHit">A boolean value indicating whether the players will get a bonus 
        /// shot after a hit.</param>
        /// <param name="hideSunkShips">A boolean value indicating whether ships will be revealed when 
        /// they are sunk.</param>
        public void SetClassicRules(bool fireUntilMiss, bool bonusShotOnHit, bool hideSunkShips)
        {
            _fireUntilMiss = fireUntilMiss;
            _bonusShotOnHit = bonusShotOnHit;
            _hideSunkShips = hideSunkShips;
        }

        /// <summary>
        /// Sets the <see cref="SalvoShots"/> type based on the user choice in the <see cref="SalvoRulesSetUpView"/>.
        /// </summary>
        /// <param name="salvoShotsType">A <see cref="SalvoShots"/> type selected by the user.</param>
        public void SetSalvoRules(SalvoShots salvoShotsType)
        {
            _salvoShotsType = salvoShotsType;
        }

        /// <summary>
        /// Returns the value of the ships can touch field. Used to retrieve the value for the <see 
        /// cref="ShipPlacementView"/>.
        /// </summary>
        /// <returns>A boolean value indicating whether ships can touch.</returns>
        public bool GetShipsCanTouchValue()
        {
            return _shipsCanTouch;
        }

        /// <summary>
        /// Updates the Game instance with the given ship positions from the ShipPlacementViewModel ready to 
        /// start a new game.
        /// </summary>
        /// <param name="shipPositions">A dictionary containing the ShipType as a key and the ship starting 
        /// position and direction as a tuple for the value.</param>
        public void SetShipPlacements(Dictionary<ShipType, (int, bool)> shipPositions)
        {
            _playerShipPositions = shipPositions;
        }

        /// <summary>
        /// Creates an instance of the <see cref="GameSetUpInformation"/> struct using the stored information 
        /// and publishes this with an <see cref="EventAggregator"/> <see cref="CreateGameEvent"/> to ensure 
        /// that the <see cref="Game"/> instance is created with the correct information.
        /// </summary>
        public void ProvideGameSetUpInformation()
        {
            var newGameInformation = new GameSetUpInformation(
                _gameType,
                _difficulty,
                _playerStarts,
                _shipsCanTouch,
                _airstrikeAllowed,
                _bombardmentAllowed,
                _fireUntilMiss,
                _bonusShotOnHit,
                _hideSunkShips,
                _salvoShotsType,
                _playerShipPositions.ToDictionary());

            _eventAggregator.GetEvent<NavigationEvent>().Publish(typeof(PlayGameView));

            Debug.WriteLine("Publish game information");
            _eventAggregator.GetEvent<CreateGameEvent>().Publish(newGameInformation);
            Debug.WriteLine($"Information published: {newGameInformation.Difficulty}");

        }
    }
}
