using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Battleships.MVVM.Enums;
using Battleships.MVVM.Structs;
using Battleships.MVVM.Factories;
using Battleships.MVVM.Services.Database;
using Battleships.MVVM.ViewModel.Base;
using Battleships.MVVM.Services;
using Battleships.MVVM.Model;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Runtime.CompilerServices;
using Battleships.MVVM.Model.DataTransferObjects;
using Battleships.MVVM.ViewModel.GridCells;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using Newtonsoft.Json.Bson;
using Battleships.MVVM.Utilities;
using Microsoft.VisualBasic;

namespace Battleships.MVVM.ViewModel
{
    public class PlayGameViewModel : ViewModelBase
    {
        #region Constants
        private const int RequiredHitsForAirstrike = 5;
        private const int RequiredHitsForBombardment = 7;

        private const int SingleShotLeftLimit = 0;
        private const int SingleShotRightLimit = 9;
        private const int SingleShotTopLimit = 0;
        private const int SingleShotBottomLimit = 9;

        private const int AirstrikeUpRightLeftLimit = 0;
        private const int AirstrikeUpRightRightLimit = 7;
        private const int AirstrikeUpRightTopLimit = 2;
        private const int AirstrikeUpRightBottomLimit = 9;

        private const int AirstrikeDownRightLeftLimit = 0;
        private const int AirstrikeDownRightRightLimit = 7;
        private const int AirstrikeDownRightTopLimit = 0;
        private const int AirstrikeDownRightBottomLimit = 7;

        private const int BombardmentLeftLimit = 1;
        private const int BombardmentRightLimit = 8;
        private const int BombardmentTopLimit = 1;
        private const int BombardmentBottomLimit = 8;

        private const int AnimationRunTimeDelay = 2000;
        private const int MessageDisplayTime = 2000;
        #endregion //Constants

        #region Theme Resource
        private static readonly ThemeNames[] DarkBackgroundThemes =
        [
            ThemeNames.Dark,
        ];

        private static readonly ThemeNames[] NeonBackgroundThemes =
        [
            ThemeNames.Neon
        ];

        private static readonly ThemeNames[] LightBackgroundThemes =
        [
            ThemeNames.Classic,
            ThemeNames.Light,
            ThemeNames.Neutral,
        ];
        #endregion //Theme Resources

        #region Image Fields
        private Uri _captainImage;
        private Uri[] _captainImageArray;
        private Uri _bomberImage;
        private Uri[] _bomberImageArray;
        private Uri? _gameOverImage;
        private Uri[] _gameOverImageArray;
        private int _bomberIndex;
        private readonly Uri _airstrikeUpRightImage;
        private readonly Uri _airstrikeUpRightWhiteImage;
        private readonly Uri _airstrikeDownRightImage;
        private readonly Uri _airstrikeDownRightWhiteImage;
        private readonly Uri _bombardmentImage;
        private readonly Uri _bombardmentWhiteImage;

        private Uri _airstrikeUpRightButtonImage;
        private Uri _airstrikeDownRightButtonImage;
        private Uri _bombardmentButtonImage;
        #endregion //Image Fields

        #region Fields
        private readonly IEventAggregator _eventAggregator;
        private readonly ISaveService _saveService;
        private readonly IMessageService _messageService;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IEventLogger _eventLogger;
        private GameSetUpInformation _gameSetUpInformation;
        private Game? _currentGame;
        private string _gameStatusMessage;
        private CancellationTokenSource? _statusMessageCts;


        private bool _airstrikeAllowed;
        private bool _airstrikeAvailable;
        private int _airstrikeHitCount;
        private bool _bombardmentAllowed;
        private bool _bombardmentAvailable;
        private int _bombardmentHitCount;

        private bool _shipsCanTouch; //if true set adjacent cells on sinking
        private bool _hideSunkShips; //if true, don't set any cells on sinking

        private ObservableCollection<PlayGameGridCell> _computerGrid;
        private ObservableCollection<PlayerBoardGridCell> _playerGrid;

        private (int row, int column) _focusedCell;
        private bool _setFocusedCellOnMouseMove;

        private ShotType _selectedShotType;
        private readonly Dictionary<ShotType, int[]> _shotTypeDeltas;

        private ThemeNames _theme;

        private ICommand? _selectGridCellCommand;
        private ICommand? _moveFocusCommand;
        private ICommand? _mouseEnterCommand;
        private ICommand? _mouseMoveCommand;
        #endregion //Fields

        #region Properties
        public Uri CaptainImage
        {
            get => _captainImage;
            set => SetProperty(ref _captainImage, value);
        }
        public Uri BomberImage
        {
            get => _bomberImage;
            set => SetProperty(ref _bomberImage, value);
        }
        public Uri AirstrikeUpRightButtonImage
        {
            get => _airstrikeUpRightButtonImage;
            set => SetProperty(ref _airstrikeUpRightButtonImage, value);
        }
        public Uri AirstrikeDownRightButtonImage
        {
            get => _airstrikeDownRightButtonImage;
            set => SetProperty(ref _airstrikeDownRightButtonImage, value);
        }
        public Uri BombardmentButtonImage
        {
            get => _bombardmentButtonImage;
            set => SetProperty(ref _bombardmentButtonImage, value);
        }

        public bool PlayerCanClick { get; set; }
        public string GameStatusMessage
        { 
            get => _gameStatusMessage; 
            set => SetProperty(ref _gameStatusMessage, value); 
        }

        public bool AirstrikeAllowed
        {
            get => _airstrikeAllowed;
            set => SetProperty(ref _airstrikeAllowed, value);
        }
        public bool AirstrikeAvailable
        {
            get => _airstrikeAvailable;
            set
            {
                if (_airstrikeAllowed)
                    SetProperty(ref _airstrikeAvailable, value);
            }
        }
        public bool BombardmentAvailable
        {
            get => _bombardmentAvailable;
            set
            {
                if (_bombardmentAllowed)
                    SetProperty(ref _bombardmentAvailable, value);
            }
        }
        public int AirstrikeHitCount
        {
            get => _airstrikeHitCount;
            set
            {
                SetProperty(ref _airstrikeHitCount, value);
                AirstrikeAvailable = _airstrikeHitCount >= RequiredHitsForAirstrike;
            }
        }
        public static int AirstrikeHitTarget => RequiredHitsForAirstrike;
        public bool BombardmentAllowed
        {
            get => _bombardmentAllowed;
            set => SetProperty(ref _bombardmentAllowed, value);
        }
        public int BombardmentHitCount
        {
            get => _bombardmentHitCount ;
            set
            {
                SetProperty(ref _bombardmentHitCount, value);
                BombardmentAvailable = _bombardmentHitCount >= RequiredHitsForBombardment;
            }
        }
        public static int BombardmentHitTarget => RequiredHitsForBombardment;
        public ObservableCollection<PlayGameGridCell> ComputerGrid
        {
            get => _computerGrid;
            set => SetProperty(ref _computerGrid, value);
        }
        public ObservableCollection<PlayerBoardGridCell> PlayerGrid
        {
            get => _playerGrid;
            set => SetProperty(ref _playerGrid, value);
        }
        public (int row, int column) FocusedCell
        {
            get => _focusedCell;
            set => SetProperty(ref _focusedCell, value);
        }
        public ShotType SelectedShotType
        {
            get => _selectedShotType;
            set
            {
                UpdateCellHighlighting(false);
                SetProperty(ref _selectedShotType, value);
                FocusedCell = AdjustFocusedCellToInbounds(FocusedCell);
                UpdateCellHighlighting(true);
            }
        }
        #endregion //Properties

        #region Commands
        public ICommand SelectGridCellCommand
        {
            get
            {
                _selectGridCellCommand ??= new RelayCommand(async param => await OnGridCellClick((int)param), param => CanExecuteGridCellClick((int)param));
                return _selectGridCellCommand;
            }
        }
        public ICommand MoveFocusCommand
        {
            get
            {
                _moveFocusCommand ??= new RelayCommand(param =>
                {
                    if (param is KeyboardDirection direction)
                        MoveFocusCell(direction);
                },
                param =>
                {
                    if (param is KeyboardDirection direction)
                        return CanMoveFocusCell(direction);
                    return false;
                });
                return _moveFocusCommand;
            }
        }
        public ICommand MouseEnterCommand
        {
            get
            {
                _mouseEnterCommand ??= new RelayCommand(param =>
                {
                    if (param is int gridPosition)
                        OnMouseEnterCell(gridPosition);
                });
                return _mouseEnterCommand;
            }
        }
        public ICommand MouseMoveCommand
        {
            get
            {
                _mouseMoveCommand ??= new RelayCommand(param =>
                {
                    if (param is int gridPosition)
                        OnMouseOverGridCell(gridPosition);
                });
                return _mouseMoveCommand;
            }
        }
        #endregion //Commands

        public PlayGameViewModel(IEventAggregator eventAggregator, ISaveService saveService, IMessageService messageService, ILoggerFactory loggerFactory)
        {
            _eventAggregator = eventAggregator
                ?? throw new ArgumentNullException(nameof(eventAggregator));
            _saveService = saveService
                ?? throw new ArgumentNullException(nameof(saveService));
            _messageService = messageService 
                ?? throw new ArgumentNullException(nameof(messageService));
            _loggerFactory = loggerFactory
                ?? throw new ArgumentNullException(nameof(loggerFactory));
            _eventLogger = loggerFactory.CreateLogger(nameof(PlayGameViewModel))
                ?? throw new InvalidOperationException("Event logger could not be created.");

            _eventAggregator.GetEvent<CreateGameEvent>().Subscribe(param =>
            {
                if (param is GameSetUpInformation gameSetUpInformation)
                {
                    SetUpNewGame(gameSetUpInformation);
                }
            });
            _eventAggregator.GetEvent<SaveEvent>().Subscribe(async () => await OnSaveRequest(false));
            _eventAggregator.GetEvent<SaveAsEvent>().Subscribe(async param => await OnSaveRequest(false, param));
            _eventAggregator.GetEvent<AutosaveEvent>().Subscribe(async () => await OnSaveRequest());
            _eventAggregator.GetEvent<LoadAutosaveEvent>().Subscribe(async () => await LoadGame(("Autosave", 0)));
            _eventAggregator.GetEvent<LoadGameEvent>().Subscribe(async param => await LoadGame(param));
            _eventAggregator.GetEvent<GameLoadedEvent>().Subscribe(async param => await InitializeLoadedGame(param));
            _eventAggregator.GetEvent<ThemeUpdateEvent>().Subscribe(theme => UpdateTheme(theme));

            _gameStatusMessage = string.Empty;
            _captainImage = new(@"pack://application:,,,/MVVM/Resources/Images/PlayGameView/captain.png", UriKind.Absolute);
            _captainImageArray = [];
            _bomberImage = new(@"pack://application:,,,/MVVM/Resources/Images/PlayGameView/bomberone.png", UriKind.Absolute);
            _bomberImageArray = [
                new(@"pack://application:,,,/MVVM/Resources/Images/PlayGameView/bomberone.png", UriKind.Absolute),
                new(@"pack://application:,,,/MVVM/Resources/Images/PlayGameView/bombertwo.png", UriKind.Absolute), 
                new(@"pack://application:,,,/MVVM/Resources/Images/PlayerGameView/bomberthree.png", UriKind.Absolute)
                ];
            _gameOverImageArray = [];

            _airstrikeUpRightImage = new(@"pack://application:,,,/MVVM/Resources/Images/PlayGameView/airstrikeupright.png", UriKind.Absolute);
            _airstrikeUpRightWhiteImage = new(@"pack://application:,,,/MVVM/Resources/Images/PlayGameView/airstrikeuprightwhite.png", UriKind.Absolute);
            _airstrikeDownRightImage = new(@"pack://application:,,,/MVVM/Resources/Images/PlayGameView/airstrikedownright.png", UriKind.Absolute);
            _airstrikeDownRightWhiteImage = new(@"pack://application:,,,/MVVM/Resources/Images/PlayGameView/airstrikedownrightwhite.png", UriKind.Absolute);
            _bombardmentImage = new(@"pack://application:,,,/MVVM/Resources/Images/PlayGameView/bombardment.png", UriKind.Absolute);
            _bombardmentWhiteImage = new(@"pack://application:,,,/MVVM/Resources/Images/PlayGameView/bombardmentwhite.png", UriKind.Absolute);

            _airstrikeUpRightButtonImage = _airstrikeUpRightImage;
            _airstrikeDownRightButtonImage = _airstrikeDownRightImage;
            _bombardmentButtonImage = _bombardmentImage;

            _computerGrid = [];
            _playerGrid = [];
            InitializeGrids();

            _focusedCell = (0, 0);
            _setFocusedCellOnMouseMove = false;

            _selectedShotType = ShotType.Single;
            _shotTypeDeltas = new Dictionary<ShotType, int[]>
            {
                { ShotType.Single, new[] { 0 } },
                { ShotType.AirstrikeUpRight, new[] { 0, -9, -18 } },
                { ShotType.AirstrikeDownRight, new[] {0, 11, 22 } },
                { ShotType.Bombardment, new[] {0, -1, 1, -10, 10} }
            };

            GameStatusMessage = _messageService.GetGameStartMessage();
            _eventAggregator.GetEvent<ThemeRequestEvent>().Publish();
        }

        #region Game Data Methods
        private void SetUpNewGame(GameSetUpInformation gameSetUpInformation)
        {
            _gameSetUpInformation = gameSetUpInformation;

            Task.Run(async () => await InitializeGame(gameSetUpInformation));
        }

        private async Task InitializeGame(GameSetUpInformation gameSetUpInformation)
        {
            // Ensure game grids and settings are reset for all new games

            AirstrikeAllowed = _gameSetUpInformation.AirstrikeAllowed;
            BombardmentAllowed = _gameSetUpInformation.BombardmentAllowed;
            _shipsCanTouch = _gameSetUpInformation.ShipsCanTouch;
            _hideSunkShips = _gameSetUpInformation.HideSunkShips;

            PlayerCanClick = _gameSetUpInformation.PlayerStarts;

            ComputerGrid = [];
            PlayerGrid = [];
            InitializeGrids();

            FocusedCell = (0, 0);
            _setFocusedCellOnMouseMove = false;

            SelectedShotType = ShotType.Single;
            AirstrikeHitCount = 0;
            BombardmentHitCount = 0;

            // Set up the new game once grid and settings initialized

            _currentGame = (_gameSetUpInformation.Type == GameType.Classic)
                ? new ClassicGame(_loggerFactory, _gameSetUpInformation)
                : new SalvoGame(_loggerFactory, _gameSetUpInformation);

            _currentGame.ComputerOpeningMoveCompleted += OnComputerOpeningMoveCompleted;

            _saveService.CurrentGame = _currentGame;
            _eventAggregator.GetEvent<StartGameEvent>().Publish();

            await Autosave();

            if (!gameSetUpInformation.PlayerStarts)
                _currentGame.PlayComputerOpeningMove();
        }

        private void InitializeGrids()
        {
            for (int row = 0; row < 10; row++)
            {
                for (int column = 0; column < 10; column++)
                {
                    _computerGrid.Add(new PlayGameGridCell(row, column, _theme));
                    _playerGrid.Add(new PlayerBoardGridCell(row, column));
                }
            }
        }

        internal async Task InitializeLoadedGame(GameDTO gameDTO)
        {
            _currentGame = (gameDTO.GameType == GameType.Classic) ?
                new ClassicGame(_loggerFactory, gameDTO) : 
                new SalvoGame(_loggerFactory, gameDTO);

            _currentGame.ComputerOpeningMoveCompleted += OnComputerOpeningMoveCompleted;

            _airstrikeAllowed = gameDTO.AirstrikeAllowed;
            _airstrikeHitCount = gameDTO.AirstrikeHitCount;
            _bombardmentAllowed = gameDTO.BombardmentAllowed;
            _bombardmentHitCount = gameDTO.BombardmentHitCount;
            _hideSunkShips = gameDTO.HideSunkShips;

            // Update the UI with the loaded game state
            for (int i = 0; i < 100; i++)
            {
                GridCellState playerState = gameDTO.PlayerBoardDTO.Grid[i];
                GridCellState computerState = gameDTO.ComputerBoardDTO.Grid[i];
                PlayerGrid[i].UpdateCellState(playerState);
                ComputerGrid[i].UpdateCellState(computerState);
            }

            PlayerCanClick = true;

            _saveService.CurrentGame = _currentGame;
            _eventAggregator.GetEvent<StartGameEvent>().Publish();

            await Autosave();
        }

        private async Task Autosave()
        {
            try
            {
                await _saveService.SaveGame();
            }
            catch (Exception ex)
            {
                _eventLogger.LogCritical("PlayGameViewModel: Autosave failed.", ex);
            }
        }

        private async Task OnSaveRequest(bool isAutosave = true, (string gameName, int saveSlot)? saveGame = null)
        {
            bool isSaveAs = saveGame is not null;
            
            _saveService.CurrentGame = _currentGame;

            if (isSaveAs)
            {
                _saveService.CurrentGameName = saveGame?.gameName;
                _saveService.CurrentSaveSlot = saveGame?.saveSlot;
            }

            try
            {
                await _saveService.SaveGame(isAutosave, isSaveAs);
            }
            catch (Exception ex)
            {
                _eventLogger.LogCritical("PlayGameViewModel: Save failed.", ex);
            }
        }

        internal async Task LoadGame((string gameName, int saveSlot) saveGame)
        {
            var isAutosave = saveGame.gameName == "Autosave";

            if (!isAutosave)
            {
                _saveService.CurrentGameName = saveGame.gameName;
                _saveService.CurrentSaveSlot = saveGame.saveSlot;
            }

            var isLoaded = await _saveService.LoadGame(isAutosave);

            if (isLoaded)
                _eventAggregator.GetEvent<StartGameEvent>().Publish();
                
        }

        /// <summary>
        /// Updates the current theme when a <see cref="ThemeUpdateEvent"/> is received.
        /// </summary>
        /// <param name="theme">The <see cref="ThemeNames"/> value to update.</param>
        private void UpdateTheme(ThemeNames theme)
        {
            if (theme is ThemeNames newTheme)
            {
                _theme = newTheme;

                foreach (var cell in ComputerGrid)
                    cell.UpdateTheme(newTheme);
                foreach (var cell in PlayerGrid)
                    cell.UpdateTheme(newTheme);
            }

            if (LightBackgroundThemes.Contains(theme))
            {
                AirstrikeUpRightButtonImage = _airstrikeUpRightImage;
                AirstrikeDownRightButtonImage = _airstrikeDownRightImage;
                BombardmentButtonImage = _bombardmentImage;
            }
            else
            {
                AirstrikeUpRightButtonImage = _airstrikeUpRightWhiteImage;
                AirstrikeDownRightButtonImage = _airstrikeDownRightWhiteImage;
                BombardmentButtonImage = _bombardmentWhiteImage;
            }
        }
        #endregion // Game Data Methods

        #region Grid Navigation Methods
        /// <summary>
        /// Uses the given keyboard direction to move the <see cref="FocusedCell"/> to enable keyboard 
        /// navigation of the grid.
        /// </summary>
        /// <param name="direction">The selected <see cref="KeyboardDirection"/>.</param>
        internal void MoveFocusCell(KeyboardDirection direction)
        {
            if (!Enum.IsDefined(typeof(KeyboardDirection), direction))
                return;

            (int row, int column) cell = FocusedCell;

            switch (direction)
            {
                case KeyboardDirection.Left:
                    cell.column--;
                    break;

                case KeyboardDirection.Right:
                    cell.column++;
                    break;

                case KeyboardDirection.Up:
                    cell.row--;
                    break;

                case KeyboardDirection.Down:
                    cell.row++;
                    break;
            }

            UpdateCellHighlighting(false);
            FocusedCell = cell;
            UpdateCellHighlighting(true);

            _setFocusedCellOnMouseMove = true;
        }

        /// <summary>
        /// Checks if there if sufficient space on the grid to make a move in the given <paramref name="direction"/> 
        /// when using the keyboard based on the FocusedCell position and SelectedShotTyoe. 
        /// Used to enable keyboard navigation of the grid without going out of bounds.
        /// </summary>
        /// <param name="direction">The selected <see cref="KeyboardDirection"/>.</param>
        /// <returns>A boolean value indicating whether a move is possible without going out of bounds.</returns>
        private bool CanMoveFocusCell(KeyboardDirection direction)
        {
            return direction switch
            {
                KeyboardDirection.Left => FocusedCell.column > SelectedShotType switch
                {
                    ShotType.AirstrikeUpRight => AirstrikeUpRightLeftLimit,
                    ShotType.AirstrikeDownRight => AirstrikeDownRightLeftLimit,
                    ShotType.Bombardment => BombardmentLeftLimit,
                    _ => SingleShotLeftLimit
                },
                KeyboardDirection.Right => FocusedCell.column < SelectedShotType switch 
                { 
                    ShotType.AirstrikeUpRight => AirstrikeUpRightRightLimit,
                    ShotType.AirstrikeDownRight => AirstrikeDownRightRightLimit,
                    ShotType.Bombardment => BombardmentRightLimit,
                    _ => SingleShotRightLimit
                },
                KeyboardDirection.Up => FocusedCell.row > SelectedShotType switch
                { 
                    ShotType.AirstrikeUpRight => AirstrikeUpRightTopLimit,
                    ShotType.AirstrikeDownRight => AirstrikeDownRightTopLimit,
                    ShotType.Bombardment => BombardmentTopLimit,
                    _ => SingleShotTopLimit
                },
                KeyboardDirection.Down => FocusedCell.row < SelectedShotType switch
                { 
                    ShotType.AirstrikeUpRight => AirstrikeUpRightBottomLimit,
                    ShotType.AirstrikeDownRight => AirstrikeDownRightBottomLimit,
                    ShotType.Bombardment => BombardmentBottomLimit,
                    _ => SingleShotBottomLimit

                },
                _ => false
            };
        }

        /// <summary>
        /// If the keyboard has been used, resets the <see cref="FocusedCell"/> to the current mouse grid 
        /// position when it is moved. Used to manage keyboard and mouse controls.
        /// </summary>
        /// <param name="gridPosition">An integer value representing the current mouse grid position.</param>
        private void OnMouseOverGridCell(int gridPosition)
        {
            if (_setFocusedCellOnMouseMove)
            {
                var (row, column) = (gridPosition / 10, gridPosition % 10);
                UpdateCellHighlighting(false);
                FocusedCell = AdjustFocusedCellToInbounds((row, column));
                UpdateCellHighlighting(true);
                _setFocusedCellOnMouseMove = false;
            }
        }

        /// <summary>
        /// Updates the FocusedCell when the mouse enters a cell. Used for ship placement using the mouse.
        /// </summary>
        /// <param name="gridPosition">An integer value representing the grid position entered.</param>
        private void OnMouseEnterCell(int gridPosition)
        {
            var (row, column) = (gridPosition / 10, gridPosition % 10);

            UpdateCellHighlighting(false);
            FocusedCell = AdjustFocusedCellToInbounds((row, column));
            UpdateCellHighlighting(true);
        }

        /// <summary>
        /// Adjusts the row and column values for the given cell to ensure that they are inbounds for the 
        /// SelectedShotType. Used to ensure that the the highlighting stays inbounds for mouse movement and 
        /// when the shot type is changed. Ensures that a shot that goes out of bounds cannot be selected.
        /// </summary>
        /// <param name="selectedCell">The cell to be adjusted to keep the shot inbounds.</param>
        /// <returns>A value tuple containing the integer values for the adjusted position.</returns>
        private (int, int) AdjustFocusedCellToInbounds((int row, int column) selectedCell)
        {
            int inboundsRow = SelectedShotType switch
            {
                ShotType.AirstrikeUpRight => Math.Max(selectedCell.row, AirstrikeUpRightTopLimit),
                ShotType.AirstrikeDownRight => Math.Min(selectedCell.row, AirstrikeDownRightBottomLimit),
                ShotType.Bombardment => Math.Clamp(selectedCell.row, BombardmentTopLimit, BombardmentBottomLimit),
                _ => selectedCell.row
            };
            int inboundsColumn = SelectedShotType switch
            { 
                ShotType.AirstrikeUpRight => Math.Min(selectedCell.column, AirstrikeUpRightRightLimit),
                ShotType.AirstrikeDownRight => Math.Min(selectedCell.column, AirstrikeDownRightRightLimit),
                ShotType.Bombardment => Math.Clamp(selectedCell.column, BombardmentLeftLimit, BombardmentRightLimit),
                _ => selectedCell.column
            };

            return (inboundsRow, inboundsColumn);
        }
        #endregion //Grid Navigation Methods

        #region Shot Selection Methods
        private async Task OnGridCellClick(int gridPosition)
        {
            PlayerCanClick = false;

            (int row, int column) = (gridPosition / 10, gridPosition % 10);
            (row, column) = AdjustFocusedCellToInbounds((row, column));
            gridPosition = 10 * row + column;

            AttackStatusReport newReport = _currentGame?.ProcessPlayerShotSelection(gridPosition, SelectedShotType) 
                    ?? new();
            await ProcessAttackStatusReport(newReport);

            _saveService.CurrentGame = _currentGame;
            await Autosave();
        }

        internal async Task ProcessAttackStatusReport(AttackStatusReport attackStatusReport, bool computerOpeningMove = false)
        {
            //run animation, update message and grid, delay, update player/cpu turn message or run GameOver
            int listLength = attackStatusReport.Reports.Count;

            SetBomberImage();
            await Task.Delay(AnimationRunTimeDelay);

            // Update computer grid (first report is always player shot)
            UpdateCellState(attackStatusReport.Reports[0].PositionsHit.ToList(), GridCellState.Hit);
            UpdateCellState(attackStatusReport.Reports[0].PositionsMissed.ToList(), GridCellState.Miss);

            _setFocusedCellOnMouseMove = true;

            // Add the player hits to the appropriate special attack hit count if active
            int totalHits = attackStatusReport.Reports[0].PositionsHit.Count;
            if (_airstrikeAllowed)
                AirstrikeHitCount += totalHits;
            if (_bombardmentAllowed)
                BombardmentHitCount += totalHits;

            // Update player grid (subsequent reports are always computer shots)
            for (int i = 1; i < listLength; i++)
            {
                UpdateCellState(attackStatusReport.Reports[i].PositionsHit.ToList(), GridCellState.Hit, true);
                UpdateCellState(attackStatusReport.Reports[i].PositionsMissed.ToList(), GridCellState.Miss, true);
            }

            if (attackStatusReport.IsGameOver)
            {
                PlayerCanClick = false;
                OnGameOver();
            }
            else
                PlayerCanClick = true;
        }

        private void UpdateCellState(List<int> indexes, GridCellState state, bool playerGrid = false)
        {
            foreach (int index in indexes)
            {
                if (playerGrid)
                    _playerGrid[index].UpdateCellState(state);
                else
                    _computerGrid[index].UpdateCellState(state);
            }
        }

        private bool CanExecuteGridCellClick(int gridPosition)
        {
            if (!PlayerCanClick)
                return false;

            int rowMax = SelectedShotType switch
            {
                ShotType.AirstrikeUpRight => AirstrikeUpRightBottomLimit,
                ShotType.AirstrikeDownRight => AirstrikeDownRightBottomLimit,
                ShotType.Bombardment => BombardmentBottomLimit,
                _ => SingleShotBottomLimit
            };

            int rowMin = SelectedShotType switch
            {
                ShotType.AirstrikeUpRight => AirstrikeUpRightTopLimit,
                ShotType.AirstrikeDownRight => AirstrikeDownRightTopLimit,
                ShotType.Bombardment => BombardmentTopLimit,
                _ => SingleShotTopLimit
            };

            int colMax = SelectedShotType switch
            {
                ShotType.AirstrikeUpRight => AirstrikeUpRightRightLimit,
                ShotType.AirstrikeDownRight => AirstrikeDownRightRightLimit,
                ShotType.Bombardment => BombardmentRightLimit,
                _ => SingleShotRightLimit
            };

            int colMin = SelectedShotType switch
            {
                ShotType.AirstrikeUpRight => AirstrikeUpRightLeftLimit,
                ShotType.AirstrikeDownRight => AirstrikeDownRightLeftLimit,
                ShotType.Bombardment => BombardmentLeftLimit,
                _ => SingleShotLeftLimit
            };

            if (FocusedCell.row < rowMin
                || FocusedCell.row > rowMax
                || FocusedCell.column < colMin
                || FocusedCell.column > colMax)
                return false;

            return PlayerCanClick;
        }
        #endregion //Shot Selection Methods

        #region Message and Animation Methods

        private void SetBomberImage()
        {
            int length = _bomberImageArray.Length;
            if (length <= 1)
                return; 
            
            int newIndex;

            do
            {
                newIndex = RandomProvider.Instance.Next(length);
            } while (newIndex == _bomberIndex);
                

            _bomberIndex = newIndex;
            BomberImage = _bomberImageArray[_bomberIndex];
        }

        /// <summary>
        /// Updates the <see cref="GameStatusMessage"/> and clears it after a delay to inform the player 
        /// what is happening. Uses CancellationTokens to manage multiple messages before previous message 
        /// is cleared.
        /// </summary>
        private async Task UpdateStatusMessage(string message)
        {
            // Cancel any previous message update
            _statusMessageCts?.Cancel();
            _statusMessageCts = new CancellationTokenSource();

            var token = _statusMessageCts.Token;
            GameStatusMessage = message;

            try
            {
                await Task.Delay(MessageDisplayTime, token);
                GameStatusMessage = string.Empty;
            }
            catch (TaskCanceledException)
            {
                // Task cancellation expected
            }
        }
        private void OnGameOver()
        {

        }
        
        #endregion //Message and Animation Methods

        #region Event Handler Methods
        /// <summary>
        /// Processes the computer opening move if required and updates the grid.
        /// </summary>
        private void OnComputerOpeningMoveCompleted(object? sender, AttackStatusReport e)
        {
            AttackStatusReport attackStatusReport = e;
            int listLength = attackStatusReport.Reports.Count;
            
            for (int i = 0; i < listLength; i++)
            {
                UpdateCellState(attackStatusReport.Reports[i].PositionsHit.ToList(), GridCellState.Hit, true);
                UpdateCellState(attackStatusReport.Reports[i].PositionsMissed.ToList(), GridCellState.Miss, true);
            }

            PlayerCanClick = true;
        }

        #endregion //Event Handler Methods

        #region Display Methods
        private void UpdateCellHighlighting(bool isHighlighted)
        {
            var shotDeltas = _shotTypeDeltas[SelectedShotType];
            int index = FocusedCell.row * 10 + FocusedCell.column;

            foreach (var delta in shotDeltas)
                ComputerGrid[index + delta].SetHighlighting(CellHighlighting.IsHighlighted, isHighlighted);
        }
        #endregion //Display Methods
    }
}
