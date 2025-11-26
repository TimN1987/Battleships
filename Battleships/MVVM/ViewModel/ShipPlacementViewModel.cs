using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Battleships.MVVM.Enums;
using Battleships.MVVM.Factories;
using Battleships.MVVM.Services;
using Battleships.MVVM.Services.Database;
using Battleships.MVVM.Structs;
using Battleships.MVVM.ViewModel.Base;
using Battleships.MVVM.View;
using Battleships.MVVM.Model;
using Battleships.MVVM.ViewModel.GridCells;
using Battleships.MVVM.Utilities;

namespace Battleships.MVVM.ViewModel;

public class ShipPlacementViewModel : ViewModelBase
{
    #region Constant Error Messages
    private const int ErrorMessageDisplayTime = 2000;
    private const string DictionaryNotCompleteMessage = "You must place all ships before starting the game";
    private const string OverlappingOccupiedCellsMessage = "You cannot overlap another ship";
    private const string OverlappingTouchingCellsMessage = "You cannot place one ship touching another";
    #endregion //Constant Error Messages

    #region Tooltip Messages
    public string BattleshipTooltip => "Battleship - size 4.";
    public string CarrierTooltip => "Carrier - size 5.";
    public string CruiserTooltip => "Cruiser - size 3";
    public string DestroyerTooltip => "Destroyer - size 2.";
    public string SubmarineTooltip => "Submarine - size 3";
    public string RotateShipTooltip => "Rotate the ship before placing.";
    #endregion //Tooltip Messages

    #region Theme Resource
    private static readonly ThemeNames[] DarkBackgroundThemes =
    [
        ThemeNames.Dark,
        ThemeNames.Neon,
    ];

    private static readonly ThemeNames[] LightBackgroundThemes =
    [
        ThemeNames.Classic,
        ThemeNames.Light,
        ThemeNames.Neutral,
    ];

    private static readonly Uri _rotationImage = new(@"pack://application:,,,/MVVM/Resources/Images/ShipPlacementView/rotationimage.png", UriKind.Absolute);
    private static readonly Uri _rotationImageRotated = new Uri(@"pack://application:,,,/MVVM/Resources/Images/ShipPlacementView/rotationimagerotated.png", UriKind.Absolute);
    private static readonly Uri _rotationImageWhite = new Uri(@"pack://application:,,,/MVVM/Resources/Images/ShipPlacementView/rotationimagewhite.png", UriKind.Absolute);
    private static readonly Uri _rotationImageWhiteRotated = new Uri(@"pack://application:,,,/MVVM/Resources/Images/ShipPlacementView/rotationimagewhiterotated.png", UriKind.Absolute);

    private static readonly Uri _battleshipImage = new(@"pack://application:,,,/MVVM/Resources/Images/ShipPlacementView/battleship.png", UriKind.Absolute);
    private static readonly Uri _battleshipImageWhite = new(@"pack://application:,,,/MVVM/Resources/Images/ShipPlacementView/battleshipwhite.png", UriKind.Absolute);
    private static readonly Uri _carrierImage = new(@"pack://application:,,,/MVVM/Resources/Images/ShipPlacementView/carrier.png", UriKind.Absolute);
    private static readonly Uri _carrierImageWhite = new(@"pack://application:,,,/MVVM/Resources/Images/ShipPlacementView/carrierwhite.png", UriKind.Absolute);
    private static readonly Uri _cruiserImage = new(@"pack://application:,,,/MVVM/Resources/Images/ShipPlacementView/cruiser.png", UriKind.Absolute);
    private static readonly Uri _cruiserImageWhite = new(@"pack://application:,,,/MVVM/Resources/Images/ShipPlacementView/cruiserwhite.png", UriKind.Absolute);
    private static readonly Uri _destroyerImage = new(@"pack://application:,,,/MVVM/Resources/Images/ShipPlacementView/destroyer.png", UriKind.Absolute);
    private static readonly Uri _destroyerImageWhite = new(@"pack://application:,,,/MVVM/Resources/Images/ShipPlacementView/destroyerwhite.png", UriKind.Absolute);
    private static readonly Uri _submarineImage = new(@"pack://application:,,,/MVVM/Resources/Images/ShipPlacementView/submarine.png", UriKind.Absolute);
    private static readonly Uri _submarineImageWhite = new(@"pack://application:,,,/MVVM/Resources/Images/ShipPlacementView/submarinewhite.png", UriKind.Absolute);

    private static readonly Uri _helpPage = new(@"pack://application:,,,/MVVM/Resources/Images/ShipPlacementView/placeshipshelp.jpg", UriKind.Absolute);
    #endregion //Theme Resources

    #region Fields
    private readonly IEventAggregator _eventAggregator;
    private readonly IGameSetUpService _gameSetUpService;
    private readonly Dictionary<ShipType, int> _shipSizes;

    private (int row, int column) _focusedCell;
    private (int row, int column) _previousFocusedCell;
    private bool _setFocusedCellOnMouseMove;

    private Dictionary<ShipType, (int startPosition, bool isHorizontal)> _shipPositions;
    private readonly bool _shipsCanTouch;
    private bool _isShipHorizontal;
    private ShipType _currentShip;

    private ObservableCollection<ShipPlacementGridCell> _gridCells;
    private CancellationTokenSource? _errorCTS;
    private string _errorMessage;

    private ThemeNames _theme;
    private Uri _rotationButtonImage;
    private Uri _battleshipButtonImage;
    private Uri _carrierButtonImage;
    private Uri _cruiserButtonImage;
    private Uri _destroyerButtonImage;
    private Uri _submarineButtonImage;

    private ICommand? _moveFocusCommand;
    private ICommand? _changeShipAlignmentCommand;
    private ICommand? _chooseShipToPlaceCommand;
    private ICommand? _placeShipCommand;
    private ICommand? _startGameCommand;
    private ICommand? _mouseEnterCommand;
    private ICommand? _mouseMoveCommand;
    private ICommand? _setRandomShipPlacementsCommand;
    #endregion //Fields

    #region Properties
    public (int row, int column) FocusedCell //handle cell highlighting etc in set accessor
    {
        get => _focusedCell;
        set
        {
            UpdateCellHighlighting(CellHighlighting.IsHighlighted, false);
            PreviousFocusedCell = _focusedCell;

            SetProperty(ref _focusedCell, value);

            UpdateCellHighlighting(CellHighlighting.IsHighlighted, true);
        }
    }
    public (int row, int column) PreviousFocusedCell
    {
        get => _previousFocusedCell;
        set => SetProperty(ref _previousFocusedCell, value);
    }

    public bool IsShipHorizontal
    {
        get => _isShipHorizontal;
        set => SetProperty(ref _isShipHorizontal, value);
    }
    public ShipType CurrentShip
    {
        get => _currentShip;
        set => SetProperty(ref _currentShip, value);
    }
    public int CurrentShipSize => _shipSizes[CurrentShip];

    public ObservableCollection<ShipPlacementGridCell> GridCells
    {
        get => _gridCells;
        set => SetProperty(ref _gridCells, value);
    }
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }
    public Uri RotationButtonImage
    {
        get => _rotationButtonImage;
        set => SetProperty(ref _rotationButtonImage, value);
    }
    public Uri BattleshipButtonImage
    {
        get => _battleshipButtonImage;
        set => SetProperty(ref _battleshipButtonImage, value);
    }
    public Uri CarrierButtonImage
    {
        get => _carrierButtonImage;
        set => SetProperty(ref _carrierButtonImage, value);
    }
    public Uri CruiserButtonImage
    {
        get => _cruiserButtonImage;
        set => SetProperty(ref _cruiserButtonImage, value);
    }
    public Uri DestroyerButtonImage
    {
        get => _destroyerButtonImage;
        set => SetProperty(ref _destroyerButtonImage, value);
    }
    public Uri SubmarineButtonImage
    {
        get => _submarineButtonImage;
        set => SetProperty(ref _submarineButtonImage, value);
    }

    public Uri HelpPageImage => _helpPage;
    #endregion //Properties

    #region Commands
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
    public ICommand ChangeShipAlignmentCommand
    {
        get
        {
            _changeShipAlignmentCommand ??= new RelayCommand(param => ChangeShipAlignment());
            return _changeShipAlignmentCommand;
        }
    }
    public ICommand ChooseShipToPlaceCommand
    {
        get
        {
            _chooseShipToPlaceCommand ??= new RelayCommand(param =>
            {
                if (param is ShipType shipType)
                    ChooseShipToPlace(shipType);
            });
            return _chooseShipToPlaceCommand;
        }
    }
    public ICommand PlaceShipCommand
    {
        get
        {
            _placeShipCommand ??= new RelayCommand(param =>
            {
                if (param is int gridPosition)
                    PlaceShip(gridPosition);
                else
                    PlaceShip();
            });
            return _placeShipCommand;
        }
    }
    public ICommand StartGameCommand
    {
        get
        {
            _startGameCommand ??= new RelayCommand(param => StartGame(), param => CanStartGame());
            return _startGameCommand;
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
    public ICommand SetRandomShipPlacementsCommand
    {
        get
        {
            _setRandomShipPlacementsCommand ??= new RelayCommand(param => SetRandomShipPlacements());
            return _setRandomShipPlacementsCommand;
        }
    }
    #endregion //Commands

    public ShipPlacementViewModel(IEventAggregator eventAggregator, IGameSetUpService gameSetUpService)
    {
        _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
        _gameSetUpService = gameSetUpService ?? throw new ArgumentNullException(nameof(gameSetUpService));

        _eventAggregator.GetEvent<ThemeUpdateEvent>().Subscribe(theme => UpdateTheme(theme));

        _shipSizes = new Dictionary<ShipType, int>
        {
            { ShipType.Battleship, 4 },
            { ShipType.Carrier, 5 },
            { ShipType.Cruiser, 3 },
            { ShipType.Destroyer, 2 },
            { ShipType.Submarine, 3 }
        };

        _focusedCell = (0, 0);
        _previousFocusedCell = (0, 0);
        _setFocusedCellOnMouseMove = false;

        _shipPositions = [];
        _shipsCanTouch = _gameSetUpService.GetShipsCanTouchValue();
        _isShipHorizontal = true;
        _currentShip = ShipType.Battleship;

        _gridCells = SetUpGridCells();
        _errorMessage = string.Empty;

        _theme = ThemeNames.Classic;
        _rotationButtonImage = _rotationImage;
        _battleshipButtonImage = _battleshipImage;
        _carrierButtonImage = _carrierImage;
        _cruiserButtonImage = _cruiserImage;
        _destroyerButtonImage = _destroyerImage;
        _submarineButtonImage = _submarineImage;

        _eventAggregator.GetEvent<ThemeRequestEvent>().Publish();

        InitializeGrid();
    }

    #region Methods
    /// <summary>
    /// Sets up the <see cref="ShipPlacementGridCell"/> collection, creating 100 indexed cells to display 
    /// in the <see cref="ShipPlacementView"/>.
    /// </summary>
    /// <returns>An ObservableCollection of <see cref="ShipPlacementGridCell"/> instances with indexes 
    /// from 0 - 99.</returns>
    internal static ObservableCollection<ShipPlacementGridCell> SetUpGridCells()
    {
        var gridCells = new ObservableCollection<ShipPlacementGridCell>();

        for (int row = 0; row < 10; row++)
            for (int column = 0; column < 10; column++)
                gridCells.Add(new ShipPlacementGridCell(row, column));

        return gridCells;
    }

    /// <summary>
    /// Ensures that the <see cref="FocusedCell"/> is set to 0, 0 and the appropriate grid highlighting 
    /// is applied when the <see cref="ShipPlacementViewModel"/> is loaded.
    /// </summary>
    private void InitializeGrid()
    {
        FocusedCell = (0, 0);
        IsShipHorizontal = true;

        UpdateCellHighlighting(CellHighlighting.IsHighlighted, true);
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

            if (LightBackgroundThemes.Contains(newTheme))
            {
                RotationButtonImage = IsShipHorizontal ? _rotationImage : _rotationImageRotated;
                BattleshipButtonImage = _battleshipImage;
                CarrierButtonImage = _carrierImage;
                CruiserButtonImage = _cruiserImage;
                DestroyerButtonImage = _destroyerImage;
                SubmarineButtonImage = _submarineImage;
            }
            else if (DarkBackgroundThemes.Contains(newTheme))
            {
                RotationButtonImage = IsShipHorizontal ? _rotationImageWhite : _rotationImageWhiteRotated;
                BattleshipButtonImage = _battleshipImageWhite;
                CarrierButtonImage = _carrierImageWhite;
                CruiserButtonImage = _cruiserImageWhite;
                DestroyerButtonImage = _destroyerImageWhite;
                SubmarineButtonImage = _submarineImageWhite;
            }
        }
    }

    /// <summary>
    /// Sets the error message by calling the async task <see cref="HandleErrorMessageAsync(string)"/> to 
    /// ensure that error messages are displayed for an appropriate amount of time without developing a 
    /// queue of messages that becomes unmanagable.
    /// </summary>
    /// <param name="errorMessage">A string representing the message to be displayed.</param>
    private void SetErrorMessage(string errorMessage)
    {
        if (errorMessage == _errorMessage)
            return;
        
        _ = HandleErrorMessageAsync(errorMessage);
    }

    /// <summary>
    /// Cancels any in progress error messages and sets the new error message. Runs a delay to ensure that 
    /// the message is only displayed temporarily. Uses Cancellation Tokens to avoid developing a queue of 
    /// messages that would be unmanagable.
    /// </summary>
    /// <param name="errorMessage">A string representing the error message to be displayed.</param>
    private async Task HandleErrorMessageAsync(string errorMessage)
    {
        _errorCTS?.Cancel();
        _errorCTS = new CancellationTokenSource();
        var token = _errorCTS.Token;

        ErrorMessage = errorMessage;

        try
        {
            await Task.Delay(ErrorMessageDisplayTime, token);
        }
        catch (TaskCanceledException)
        {
            return;
        }
        finally
        {
            ErrorMessage = string.Empty;
        }
    }
    #endregion //Methods

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

        FocusedCell = cell;
        _setFocusedCellOnMouseMove = true;
    }

    /// <summary>
    /// Checks if there if sufficient space on the grid to make a move in the given <paramref name="direction"/> 
    /// when using the keyboard based on the <see cref="FocusedCell"/> position and <see 
    /// cref="CurrentShipSize"/>. Used to enabled keyboard navigation of the grid without going out of bounds.
    /// </summary>
    /// <param name="direction">The selected <see cref="KeyboardDirection"/>.</param>
    /// <returns>A boolean value indicating whether a move is possible without going out of bounds.</returns>
    private bool CanMoveFocusCell(KeyboardDirection direction)
    {
        return direction switch
        {
            KeyboardDirection.Left => FocusedCell.column > 0,
            KeyboardDirection.Right => FocusedCell.column < (IsShipHorizontal ? 10 - CurrentShipSize : 9),
            KeyboardDirection.Up => FocusedCell.row > 0,
            KeyboardDirection.Down => FocusedCell.row < (IsShipHorizontal ? 9 : 10 - CurrentShipSize),
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
            FocusedCell = (gridPosition / 10, gridPosition % 10);
    }

    /// <summary>
    /// Updates the <see cref="FocusedCell"/> when the mouse enters a cell. Used for ship placement using 
    /// the mouse.
    /// </summary>
    /// <param name="gridPosition">An integer value representing the grid position entered.</param>
    private void OnMouseEnterCell(int gridPosition)
    {
        FocusedCell = (gridPosition / 10, gridPosition % 10);
    }
    #endregion //Grid Navigation Methods

    #region Ship Placement Methods
    /// <summary>
    /// Updates the <see cref="IsShipHorizontal"/> property when the ship alignment is changed and ensures 
    /// that the grid highlighting matches the new position.
    /// </summary>
    private void ChangeShipAlignment()
    {
        UpdateCellHighlighting(CellHighlighting.IsHighlighted, false);
        IsShipHorizontal = !IsShipHorizontal;
        UpdateCellHighlighting(CellHighlighting.IsHighlighted, true);

        if (LightBackgroundThemes.Contains(_theme))
            RotationButtonImage = IsShipHorizontal ? _rotationImage : _rotationImageRotated;
        else if (DarkBackgroundThemes.Contains(_theme))
            RotationButtonImage = IsShipHorizontal ? _rotationImageWhite : _rotationImageWhiteRotated;
    }

    /// <summary>
    /// Updates the <see cref="CurrentShip"/> property when a new <see cref="ShipType"/> is selected in 
    /// the <see cref="ShipPlacementView"/>. Ensures that the highlighting matches the new ship.
    /// </summary>
    /// <param name="shipType">The <see cref="ShipType"/> selected by the user.</param>
    private void ChooseShipToPlace(ShipType shipType)
    {
        UpdateCellHighlighting(CellHighlighting.IsHighlighted, false);
        CurrentShip = shipType;
        UpdateCellHighlighting(CellHighlighting.IsHighlighted, true);
    }

    /// <summary>
    /// Updates the <see cref="_shipPositions"/> Dictionary with the position of the <see cref="CurrentShip"/>. 
    /// Ensures that the Dictionary is not null, checks that the grid position stays in bounds and updates 
    /// highlighting for the ship position and any touching cells as necessary.
    /// </summary>
    /// <param name="gridPosition">An integer value representing the starting grid position for the ship.</param>
    private void PlaceShip(int? gridPosition = null)
    {
        if (!CanPlaceShip(gridPosition))
            return;
        
        gridPosition ??= 10 * FocusedCell.row + FocusedCell.column;
        _shipPositions ??= [];

        if (_shipPositions.TryGetValue(CurrentShip, out _))
            UpdateCellHighlighting(CellHighlighting.IsOccupied, false, CurrentShip);

        gridPosition = AdjustStartPositionToStayInBounds((int)gridPosition, IsShipHorizontal, CurrentShipSize);

        _shipPositions[CurrentShip] = ((int)gridPosition, IsShipHorizontal);

        UpdateCellHighlighting(CellHighlighting.IsOccupied, true, CurrentShip);
        ResetTouchingCells();
    }

    /// <summary>
    /// Checks that the ship is in bounds and not overlapping any occupied or touching cells. Used to ensure 
    /// that only valid ship positions are allowed.
    /// </summary>
    /// <param name="gridPosition">An integer value representing the index of the cell selected.</param>
    /// <returns>A boolean value indicating whether the ship can be placed.</returns>
    /// <remarks><see cref="IsOverlappingOccupiedCells(int)"/> and <see cref="IsOverlappingTouchingCells(int)"/> 
    /// both return true if an overlap is found, and so the ship cannot be placed.</remarks>
    private bool CanPlaceShip(int? gridPosition = null)
    {
        gridPosition ??= 10 * FocusedCell.row + FocusedCell.column;
        
        gridPosition = AdjustStartPositionToStayInBounds((int)gridPosition, IsShipHorizontal, CurrentShipSize);

        return !IsOverlappingOccupiedCells((int)gridPosition) 
            && !IsOverlappingTouchingCells((int)gridPosition);
    }

    /// <summary>
    /// Checks if the ship to be placed is overlapping any occupied cells. Used to check if the ship placement 
    /// is valid.
    /// </summary>
    /// <param name="gridPosition">An integer value representing the starting grid postions for the ship.</param>
    /// <returns>A boolean value indicating whether the ship is overlapping any occupied cells.</returns>
    /// <remarks>A return value of true indicates that the ship cannot be placed as it is overlapping.</remarks>
    private bool IsOverlappingOccupiedCells(int gridPosition)
    {
        var step = IsShipHorizontal ? 1 : 10;
        var lastPosition = gridPosition + (CurrentShipSize - 1) * step;

        if (lastPosition > 99) //Fallback to avoid going out of bounds.
            return true;

        for (int i = gridPosition; i <= lastPosition; i += step)
        {
            if (GridCells[i].IsOccupied)
            {
                SetErrorMessage(OverlappingOccupiedCellsMessage);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if the ship to be placed is overlapping any cells with IsTouching property set to true. Used to
    /// check if the ship placement is valid.
    /// </summary>
    /// <param name="gridPosition">An integer value representing the starting grid position for the ship.</param>
    /// <returns>A boolean value indicating whether the ship is overlapping touching cells.</returns>
    /// <remarks>A return value of true indicates that the ship cannot be placed as it is overlapping.</remarks>
    private bool IsOverlappingTouchingCells(int gridPosition)
    {
        if (_shipsCanTouch)
            return false;

        var step = IsShipHorizontal ? 1 : 10;
        var lastPosition = gridPosition + (CurrentShipSize - 1) * step;

        if (lastPosition > 99) //Fallback to avoid going out of bounds.
            return true;

        for (int i = gridPosition; i <= lastPosition; i += step)
        {
            if (GridCells[i].IsTouching)
            {
                SetErrorMessage(OverlappingTouchingCellsMessage);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Sets the given <see cref="CellHighlighting"/> type to <paramref name="isHighlighted"/>. Used to 
    /// update the cell highlighting for possible and confirmed ship placements.
    /// </summary>
    /// <param name="shipType">The <see cref="ShipType"/> to be added or cleared.</param>
    /// <param name="highlightingType">The <see cref="CellHighlighting"/> type to be updated.</param>
    /// <param name="isHighlighted">A boolean value indicating whether the cells should be highlighted.</param>
    /// <remarks>Ensure that the <see cref="_shipPositions"/> Dictionary contains a key for a non-null 
    /// <paramref name="shipType"/> parameter before calling method.</remarks>
    private void UpdateCellHighlighting(CellHighlighting highlightingType, bool isHighlighted, ShipType? shipType = null)
    {
        int startPosition;
        int shipSize;
        bool isHorizontal;
        
        if (shipType is null)
        {
            startPosition = FocusedCell.row * 10 + FocusedCell.column;
            shipSize = CurrentShipSize;
            isHorizontal = IsShipHorizontal;
        }
        else
        {
            if (_shipPositions is null)
                return;

            startPosition = _shipPositions[(ShipType)shipType].startPosition;
            shipSize = _shipSizes[(ShipType)shipType];
            isHorizontal = _shipPositions[(ShipType)shipType].isHorizontal;
        }

        startPosition = AdjustStartPositionToStayInBounds(startPosition, isHorizontal, shipSize);

        var step = isHorizontal ? 1 : 10;
        var lastPosition = startPosition + (shipSize - 1) * step;

        if (lastPosition > 99) //Fallback to avoid going out of bounds.
            return;

        for (int i = startPosition; i <= lastPosition; i += step)
            GridCells[i].SetHighlighting(highlightingType, isHighlighted);
    }

    /// <summary>
    /// Updates the IsTouching property for cells adjacent to a positioned ship to the given value <paramref 
    /// name="isTouching"/>. Used when ships cannot touch to highlight invalid cells around a position ship 
    /// or clear the highlighting if the ship is removed.
    /// </summary>
    /// <param name="shipType">The <see cref="ShipType"/> of the positioned ship with adjacent cells to 
    /// update.</param>
    /// <param name="isTouching">A boolean value indicating what the adjacent cells should be set to.</param>
    /// <remarks>Check that the <see cref="_shipsCanTouch"/> field is set to false before calling 
    /// this method.</remarks>
    private void UpdateTouchingCells(ShipType shipType, bool isTouching)
    {
        if (_shipsCanTouch || _shipPositions is null)
            return;

        if (_shipPositions.TryGetValue(shipType, out var ship))
        {
            int shipSize = _shipSizes[shipType];

            var step = ship.isHorizontal ? 1 : 10;
            var lastPosition = ship.startPosition + (shipSize - 1) * step;

            if (lastPosition > 99) //Fallback to avoid going out of bounds.
                return;

            for (int i = ship.startPosition; i <= lastPosition; i += step)
                SetInBoundsNeighborsTouching(i, isTouching);
        }
    }

    /// <summary>
    /// Checks the four adjacent neighbors for a cell and sets their IsTouching property if they 
    /// are not occupied and are in bounds. Used to set IsTouching property when ships cannot touch.
    /// </summary>
    /// <param name="gridPosition">An integer value representing the grid position of the occupied cell 
    /// with neighbors to update.</param>
    /// <param name="isTouching">A boolean value indicating what the IsTouching property should be 
    /// set to.</param>
    private void SetInBoundsNeighborsTouching(int gridPosition, bool isTouching)
    {
        int row = gridPosition / 10;
        int column = gridPosition % 10;

        if (column > 0 && !GridCells[gridPosition - 1].IsOccupied)
            GridCells[gridPosition - 1].SetHighlighting(CellHighlighting.IsTouching, isTouching);

        if (column < 9 && !GridCells[gridPosition + 1].IsOccupied)
            GridCells[gridPosition + 1].SetHighlighting(CellHighlighting.IsTouching, isTouching);

        if (row > 0 && !GridCells[gridPosition - 10].IsOccupied)
            GridCells[gridPosition - 10].SetHighlighting(CellHighlighting.IsTouching, isTouching);

        if (row < 9 && !GridCells[gridPosition + 10].IsOccupied)
            GridCells[gridPosition + 10].SetHighlighting(CellHighlighting.IsTouching, isTouching);
    }

    /// <summary>
    /// Clears all touching cell highlighting and resets using stored ship positions. Used to ensure a 
    /// clean update of touching cells where two or more ships may share a touching cell.
    /// </summary>
    private void ResetTouchingCells()
    {
        if (_shipPositions is null)
            return;

        ClearAllCellHighlighting(CellHighlighting.IsTouching);

        foreach (var (ship, _) in _shipPositions)
            UpdateTouchingCells(ship, true);
    }

    /// <summary>
    /// Clears all occupied cell highlighting and resets using stored ship positions. Used to ensure a 
    /// clean update of occupied cells when random positions are generated.
    /// </summary>
    private void ResetOccupiedCells()
    {
        if (_shipPositions is null)
            return;

        ClearAllCellHighlighting(CellHighlighting.IsOccupied);

        foreach (var (ship, _) in _shipPositions)
            UpdateCellHighlighting(CellHighlighting.IsOccupied, true, ship);
    }

    /// <summary>
    /// Clears all cell highlighting of the given type. Used to enable clean updates of highlighting, for 
    /// example when setting new random ship placements.
    /// </summary>
    /// <param name="highlightingType">The <see cref="CellHighlighting"/> type to be cleared.</param>
    private void ClearAllCellHighlighting(CellHighlighting highlightingType)
    {
        switch (highlightingType)
        {
            case CellHighlighting.IsHighlighted:
                foreach (var cell in GridCells)
                    cell.IsHighlighted = false;
                break;

            case CellHighlighting.IsOccupied:
                foreach (var cell in GridCells)
                    cell.IsOccupied = false;
                break;

            case CellHighlighting.IsTouching:
                foreach (var cell in GridCells)
                    cell.IsTouching = false;
                break;
        }
    }

    /// <summary>
    /// Checks if the whole ship will be in bounds for a given <paramref name="gridPosition"/>, <paramref 
    /// name="shipSize"/> and orientation. If not, adjusts the start position for the ship to keep it 
    /// within the grid.
    /// </summary>
    /// <param name="gridPosition">An integer value representing the start position of the ship.</param>
    /// <param name="isHorizontal">A boolean value indicating whether the ship is horizontally aligned.</param>
    /// <param name="shipSize">An integer value representing the size of the ship.</param>
    /// <returns>An integer value representing the index of the new start position for the ship.</returns>
    private static int AdjustStartPositionToStayInBounds(int gridPosition, bool isHorizontal, int shipSize)
    {
        int row = gridPosition / 10;
        int column = gridPosition % 10;

        if (isHorizontal && column + shipSize - 1 > 9)
            column = 10 - shipSize;

        if (!isHorizontal && row + shipSize - 1 > 9)
            row = 10 - shipSize;

        return 10 * row + column;
    }

    /// <summary>
    /// Generates random ship placements and updates the <see cref="_shipPositions"/> field. Ensures that 
    /// no ships overlap and only touch if allowed. Updates the grid highlighting. Allows the user to select 
    /// and adjust random ship positions.
    /// </summary>
    private void SetRandomShipPlacements()
    {
        var shipPositions = new Dictionary<ShipType, (int gridPosition, bool isHorizontal)>();

        var occupiedPositions = new HashSet<int>();

        var random = RandomProvider.Instance;

        foreach (var (shipType, size) in _shipSizes)
        {
            bool isValid = false;

            while (!isValid)
            {
                isValid = true;

                var testPosition = random.Next(100);
                var isHorizontal = random.Next(2) == 0;

                testPosition = AdjustStartPositionToStayInBounds(testPosition, isHorizontal, size);

                var possiblePositions = new List<int>();

                for (int i = 0; i < size; i++)
                {
                    int position = (isHorizontal) ? testPosition + i : testPosition + 10 * i;
                    possiblePositions.Add(position);

                    if (occupiedPositions.Contains(position))
                    {
                        isValid = false;
                        break;
                    }

                    if (!_shipsCanTouch &&
                        (occupiedPositions.Contains(position - 1)
                            || occupiedPositions.Contains(position + 1)
                            || occupiedPositions.Contains(position - 10)
                            || occupiedPositions.Contains(position + 10)))
                    {
                        isValid = false;
                        break;
                    }
                }

                if (isValid)
                {
                    shipPositions[shipType] = (testPosition, isHorizontal);
                    foreach (var position in possiblePositions)
                        occupiedPositions.Add(position);
                }
            }
        }

        _shipPositions = shipPositions;
        
        ResetOccupiedCells();
        ResetTouchingCells();
    }
    #endregion //Ship Placement Methods

    #region Start Game Methods
    /// <summary>
    /// Starts a new game by calling <see cref="GameSetUpService"/> methods to finish updating the <see 
    /// cref="GameSetUpInformation"/> and publishing this. Publishes a <see cref="NavigationEvent"/> to the 
    /// <see cref="MainViewModel"/> so a game can be started.
    /// </summary>
    private void StartGame()
    {
        _gameSetUpService.SetShipPlacements(_shipPositions.ToDictionary());
        _gameSetUpService.ProvideGameSetUpInformation();
    }

    /// <summary>
    /// Ensures that a game is not started before the ship positions have been selected unless the user 
    /// has selected random placements.
    /// </summary>
    /// <param name="randomPlacements">A boolean value indicating whether the user has selected random 
    /// ship placement.</param>
    /// <returns>A boolean value indicating whether the game can be started.</returns>
    private bool CanStartGame()
    {
        if (_shipPositions.Count == 5)
            return true;

        SetErrorMessage(DictionaryNotCompleteMessage);
        return false;
    }
    #endregion //Start Game Methods
}
