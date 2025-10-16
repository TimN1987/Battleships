using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battleships.MVVM.Enums;
using Battleships.MVVM.Model.DataTransferObjects;
using Battleships.MVVM.Structs;
using Battleships.MVVM.Utilities;
using Battleships.MVVM.ViewModel;

namespace Battleships.MVVM.Model
{
    /// <summary>
    /// Defines a contract for board management during a Battleships game. This interface is used for modelling 
    /// the grid and ships, as well as managing shots fired at the board.
    /// </summary>
    public interface IBoard
    {
        GridCellState[] Grid { get; set; }
        Ship[] Ships { get; set; }

        SingleTurnReport GenerateSingleTurnReport(int gridPosition, ShotType shotType);
        Dictionary<ShipType, (int gridPosition, bool isHorizontal)> GenerateRandomShips();
        List<int> DeclareRemainingShipSizes();
    }

    /// <summary>
    /// This class implements the <see cref="IBoard"/> interface and is used for managing either a player or 
    /// computer board during a Battleships game. It includes an array of integers to represent the state of the 
    /// game board and an array of <see cref="Ship"/> instances to model the ships on the board. It provides 
    /// methods for generating ship positions if necessary and for handling different types of shots during 
    /// a game, returning the result of these in a manageable format.
    /// </summary>
    public class Board : IBoard // need a computer player instance if computer board
    {
        private readonly Dictionary<ShipType, int> _shipSizes = new()
        {
            { ShipType.Battleship, 4 },
            { ShipType.Carrier, 5 },
            { ShipType.Cruiser, 3 },
            { ShipType.Destroyer, 2 },
            { ShipType.Submarine, 3 }
        };
        private readonly bool _shipsCanTouch;
        
        public GridCellState[] Grid { get; set; }
        public Ship[] Ships { get; set; }

        /// <summary>
        /// Primary constructor uses <see cref="GameSetUpInformation"/> to instantiate a board as part of the 
        /// new game set up.
        /// </summary>
        /// <param name="information">A <see cref="GameSetUpInformation"/> instance containing information 
        /// needed to see up the <see cref="Board"/>.</param>
        public Board(GameSetUpInformation information, bool isPlayer)
        {
            var shipPlacements = isPlayer 
                ? information.ShipPositions ??= GenerateRandomShips() 
                : GenerateRandomShips();

            _shipsCanTouch = information.ShipsCanTouch;
          
            Ships = new Ship[5];
            Ships[0] = new Battleship(shipPlacements[ShipType.Battleship]);
            Ships[1] = new Carrier(shipPlacements[ShipType.Carrier]);
            Ships[2] = new Cruiser(shipPlacements[ShipType.Cruiser]);
            Ships[3] = new Destroyer(shipPlacements[ShipType.Destroyer]);
            Ships[4] = new Submarine(shipPlacements[ShipType.Submarine]);

            Grid = new GridCellState[100];
        }

        /// <summary>
        /// <see cref="BoardDTO"/> used to load previous game state data during the load game process.
        /// </summary>
        /// <param name="boardDTO">A <see cref="BoardDTO"/> containing loaded information 
        /// to return to a previous game state.</param>
        public Board(BoardDTO boardDTO)
        {
            _shipsCanTouch = boardDTO.ShipsCanTouch;
            
            Grid = boardDTO.Grid;

            Ships = new Ship[5];
            Ships[0] = new Battleship(boardDTO.ShipsDTO[0]);
            Ships[1] = new Carrier(boardDTO.ShipsDTO[1]);
            Ships[2] = new Cruiser(boardDTO.ShipsDTO[2]);
            Ships[3] = new Destroyer(boardDTO.ShipsDTO[3]);
            Ships[4] = new Submarine(boardDTO.ShipsDTO[4]);
        }

        /// <summary>
        /// Checks the shot type and calls the appropriate method to retrieve relevant information from the 
        /// <see cref="Ship"/> instances on the board. Uses this to compile the information into a <see 
        /// cref="SingleTurnReport"/> to be used for game management in the <see cref="Game"/> class and for 
        /// the GUI in the <see cref="PlayGameViewModel"/>.
        /// </summary>
        /// <param name="gridPosition">The integer representing the position in the array of the shot 
        /// to be evaluated.</param>
        /// <param name="shotType">The type of shot played to enable the correct number and position of shots 
        /// to be evaluated.</param>
        /// <returns>A <see cref="SingleTurnReport"/> containing the relevant information for continued 
        /// game management.</returns>
        public SingleTurnReport GenerateSingleTurnReport(int gridPosition, ShotType shotType)
        {
            var shotsFired = new List<int>();
            
            switch (shotType)
            {
                case ShotType.Single:
                    shotsFired.Add(gridPosition);
                    return ProcessShots(shotsFired);

                case ShotType.AirstrikeDownRight:
                    for (int i = 0; i < 3; i++)
                        shotsFired.Add(gridPosition + 11 * i); //Adding 11 moves down one row and across one column
                    return ProcessShots(shotsFired);

                case ShotType.AirstrikeUpRight:
                    for (int i = 0; i < 3; i++)
                        shotsFired.Add(gridPosition - 9 * i); //Subtracting 10 moves up one row. Adding 1 moves across one column
                    return ProcessShots(shotsFired);

                case ShotType.Bombardment:
                    shotsFired.Add(gridPosition);
                    shotsFired.Add(gridPosition + 1);
                    shotsFired.Add(gridPosition - 1);
                    shotsFired.Add(gridPosition + 10);
                    shotsFired.Add(gridPosition - 10);
                    return ProcessShots(shotsFired);

                default:
                    return new SingleTurnReport();
            }
        }

        /// <summary>
        /// Iterates over each <see cref="Ship"/> instance for each shot fired to check for hits, misses and 
        /// sinkings. Uses this information to generate a <see cref="SingleTurnReport"/>.
        /// </summary>
        /// <param name="shotsFired">A list of integer values representing the grid position of each shot fired.</param>
        /// <returns>A SingleTurnReport compiling the results of each of the shots.</returns>
        private SingleTurnReport ProcessShots(List<int> shotsFired)
        {
            var positionsHit = new List<int>();
            var positionsMissed = new List<int>();
            var shipsSunk = new List<(int startPosition, bool isHorizontal, ShipType shipType)>();

            bool hitFound = false;

            foreach (var shot in shotsFired)
            {
                hitFound = false;

                foreach (Ship ship in Ships)
                {
                    if (ship.IsSunk) continue;

                    if (ship.CheckForHit(shot))
                    {
                        hitFound = true;
                        positionsHit.Add(shot);
                        Grid[shot] = GridCellState.Hit;

                        if (ship.IsSunk)
                        {
                            shipsSunk.Add((ship.Positions[0], ship.IsHorizontal, ship.ShipType));

                            foreach (var sunkPosition in ship.Positions)
                                Grid[sunkPosition] = GridCellState.Sunk;
                        }
                    }

                    if (hitFound) break;
                }

                if (!hitFound)
                {
                    positionsMissed.Add(shot);
                    Grid[shot] = GridCellState.Miss;
                }
            }


            var isGameOver = Ships.All(ship => ship.IsSunk == true);
            return new SingleTurnReport(positionsHit, positionsMissed, shipsSunk, isGameOver);
        }

        /// <summary>
        /// Generates a dictionary of random positions for each of the five <see cref="Ship"/> types. Uses 
        /// validation to ensure no overlaps and, if necessary, no touching.
        /// </summary>
        /// <returns>A dictionary of ship types and positions.</returns>
        public Dictionary<ShipType, (int gridPosition, bool isHorizontal)> GenerateRandomShips()
        {
            var shipPositions = new Dictionary<ShipType, (int gridPosition, bool isHorizontal)>();

            var occupiedPositions = new HashSet<int>();

            var random = RandomProvider.Instance;

            foreach(var (shipType, size) in _shipSizes)
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

            return shipPositions;
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
        /// Checks the ships on the board against the type of <see cref="SalvoShots"/> requested to calculate 
        /// the number of shots for the turn in a <see cref="SalvoGame"/>.
        /// </summary>
        /// <param name="turnShotCountMethod">The type of <see cref="SalvoShots"/> used in the <see 
        /// cref="SalvoGame"/> to calculate the number of shots each turn.</param>
        /// <returns></returns>
        public int CheckShipStatusToCalculateShots(SalvoShots turnShotCountMethod)
        {
            int shots = 0;

            foreach (Ship ship in Ships)
            {
                if (ship.IsSunk) continue;
                switch (turnShotCountMethod)
                {
                    case SalvoShots.EqualsUnsunkShips:
                        shots++;
                        break;
                    case SalvoShots.EqualsUndamagedShips:
                        if (ship.IsUndamaged) shots++;
                        break;
                    case SalvoShots.EqualsLargestUndamagedShip:
                        if (ship.IsUndamaged && ship.Size > shots) shots = ship.Size;
                        break;
                    case SalvoShots.EqualsLargestUnsunkShip:
                        if (ship.Size > shots) shots = ship.Size;
                        break;
                    default:
                        break;
                }
            }

            if (shots == 0)
                shots = 1;

            return shots;
        }

        public List<int> DeclareRemainingShipSizes()
        {
            return Ships.Where(ship => ship.IsSunk == false).Select(unsunk => unsunk.Size).ToList();
        }

        /// <summary>
        /// Generates a <see cref="BoardDTO"/> for Json serialization to ensure simple data storage.
        /// </summary>
        /// <returns>A <see cref="BoardDTO"/> instance containing key information in its 
        /// current state.</returns>
        public BoardDTO GetDTO()
        {
            var boardDTO = new BoardDTO
            {
                ShipsCanTouch = _shipsCanTouch,
                Grid = this.Grid,
                ShipsDTO = new ShipDTO[5]
            };

            boardDTO.ShipsDTO[0] = Ships[0].GetDTO();
            boardDTO.ShipsDTO[1] = Ships[1].GetDTO();
            boardDTO.ShipsDTO[2] = Ships[2].GetDTO();
            boardDTO.ShipsDTO[3] = Ships[3].GetDTO();
            boardDTO.ShipsDTO[4] = Ships[4].GetDTO();

            return boardDTO;
        }
    }
}
