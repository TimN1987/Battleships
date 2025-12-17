using Battleships.MVVM.Converters;
using Battleships.MVVM.Model;
using Battleships.MVVM.ViewModel;

namespace Battleships.MVVM.Enums
{
    /// <summary>
    /// Defines the available Ship types that can be placed on the board.
    /// </summary>
    public enum ShipType
    {
        Ship,
        Battleship,
        Carrier,
        Cruiser,
        Destroyer,
        Submarine
    }

    /// <summary>
    /// Defines the available Game types that can be instantiated and played.
    /// </summary>
    public enum GameType
    {
        Classic,
        Salvo
    }

    /// <summary>
    /// Defines the different difficulty settings used in setting up the Game instance and 
    /// using the correct computer move model.
    /// </summary>
    public enum GameDifficulty
    {
        Easy,
        Medium,
        Hard,
        AI
    }

    /// <summary>
    /// Defines the different types of shots that can be used in a <see cref="SalvoGame"/>.
    /// </summary>
    public enum SalvoShots
    {
        Fixed,
        EqualsUnsunkShips,
        EqualsUndamagedShips,
        EqualsLargestUndamagedShip,
        EqualsLargestUnsunkShip,
        None
    }

    /// <summary>
    /// Defines the different types of shots that can be used in a <see cref="Game"/> to enable clear 
    /// information to be passed from the <see cref="PlayGameViewModel"/> when a shot is fired.
    /// </summary>
    public enum ShotType
    {
        Single,
        AirstrikeUpRight,
        AirstrikeDownRight,
        Bombardment
    }

    /// <summary>
    /// Defines the different outcomes of a shot fired in the game to enable clear information sharing between 
    /// the <see cref="Board"/> and <see cref="Game"/> classes.
    /// </summary>
    public enum ShotOutcome
    {
        Miss,
        Hit,
        MultipleHit
    }

    /// <summary>
    /// Defines the different states of a cell during a Battleships game.
    /// </summary>
    public enum GridCellState
    {
        Unattacked,
        Miss,
        Hit,
        Sunk
    }

    /// <summary>
    /// Defines the names of table in the database.
    /// </summary>
    public enum SaveGameTable
    {
        AutosaveGame,
        SaveGames
    }

    /// <summary>
    /// Defines the names of the different themes available in the application.
    /// </summary>
    public enum ThemeNames
    {
        Classic,
        Dark,
        Light,
        Neon,
        Neutral
    }

    /// <summary>
    /// Defines the different methods for navigating grids and moving the highlighted cells. Used in the 
    /// <see cref="ShipPlacementViewModel"/> and <see cref="PlayGameViewModel"/>.
    /// </summary>
    public enum GridNavigationMethod
    {
        Mouse,
        Keyboard
    }

    /// <summary>
    /// Defines the different directions used for grid navigation with the keyboard. Used in the <see 
    /// cref="ShipPlacementViewModel"/> and <see cref="PlayGameViewModel"/>.
    /// </summary>
    public enum KeyboardDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    /// <summary>
    /// Defines the types of cell highlighting that can be used in the <see cref="ShipPlacementViewModel"/> and 
    /// <see cref="PlayGameViewModel"/>.
    /// </summary>
    public enum CellHighlighting
    {
        IsHighlighted,
        IsOccupied,
        IsTouching
    }

    /// <summary>
    /// Defines the types of events that can happen during a game. Used in EventAggregator publications to 
    /// request display messages and sound effects.
    /// </summary>
    public enum GameEvent
    {
        GameStart,
        GameLoaded,
        PlayerSunkShip,
        PlayerHitShip,
        PlayerMissed,
        ComputerSunkShip,
        ComputerHitShip,
        ComputerMissed,
        PlayerTurn,
        ComputerTurn,
        GameOver,
        EventFinished,
        BomberAnimationStart
    }

    /// <summary>
    /// Defines the positions to be returned by the <see cref="GridPositionToExplosionPositionConverter"/>.
    /// </summary>
    public enum ExplosionPosition
    {
        StartTop,
        EndTop,
        StartLeft,
        EndLeft
    }
}
