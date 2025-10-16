using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battleships.MVVM.Enums;
using Battleships.MVVM.Model;
using Battleships.MVVM.Model.DataTransferObjects;
using Battleships.MVVM.Structs;

namespace BattleshipsTests.Tests.Models
{
    public class BoardTests
    {
        #region Fields
        private const int GridSize = 100;
        private const int ShipsTotal = 5;
        private const int MultipleTestsCount = 50;
        private const int TotalShipSize = 17;
        private readonly Dictionary<ShipType, int> _shipSizes = new()
        {
            { ShipType.Battleship, 4 },
            { ShipType.Carrier, 5 },
            { ShipType.Cruiser, 3 },
            { ShipType.Destroyer, 2 },
            { ShipType.Submarine, 3 }
        };
        private static readonly GameSetUpInformation _gameSetUpInformation = new(
            GameType.Classic,
            GameDifficulty.Easy,
            true, true, true, true, true, true, true,
            SalvoShots.None,
            new()
            {
                {ShipType.Battleship, (0, true) },
                {ShipType.Carrier, (20, true) },
                {ShipType.Cruiser, (40, true) },
                {ShipType.Destroyer, (9, false) },
                {ShipType.Submarine, (39, false) }
            });
        private static readonly GameSetUpInformation _gameSetUpInformationNoShipsTouching = new(
            GameType.Classic,
            GameDifficulty.Easy,
            true, false, true, true, true, true, true, //ShipsCanTouch is false.
            SalvoShots.None,
            new()
            {
                {ShipType.Battleship, (0, true) },
                {ShipType.Carrier, (20, true) },
                {ShipType.Cruiser, (40, true) },
                {ShipType.Destroyer, (9, false) },
                {ShipType.Submarine, (39, false) }
            });
        private static readonly BoardDTO _boardDTO = new()
        {
            Grid = new GridCellState[100],
            ShipsDTO =
            [
                new()
                {
                    ShipType = ShipType.Battleship,
                    Size = 4,
                    Damage = [false, false, false, false],
                    Positions = [7, 17, 27, 37],
                    IsHorizontal = false
                },
                new()
                {
                    ShipType = ShipType.Carrier,
                    Size = 5,
                    Damage = [true, true, false, false, false],
                    Positions = [10, 11, 12, 13, 14],
                    IsHorizontal = true
                },
                new()
                {
                    ShipType = ShipType.Cruiser,
                    Size = 3,
                    Damage = [true, true, true],
                    Positions = [97, 98, 99],
                    IsHorizontal = true
                },
                new()
                {
                    ShipType = ShipType.Destroyer,
                    Size = 2,
                    Damage = [false, false],
                    Positions = [5, 15],
                    IsHorizontal = false
                },
                new()
                {
                    ShipType = ShipType.Submarine,
                    Size = 3,
                    Damage = [false, false, false],
                    Positions = [90, 91, 92],
                    IsHorizontal = true
                }
            ]
        };
        #endregion //Fields

        #region Constructor Tests
        /// <summary>
        /// Attempts to pass a null value as the parameter of the <see cref="Board"/> constructor. Should throw 
        /// a <see cref="NullReferenceException"/>.
        /// </summary>
        [Fact]
        public void Constructor_NullParameter_ThrowsNullReferenceException()
        {
            //Arrange, Act & Assert
            Assert.Throws<NullReferenceException>(() => new Board(null!));
        }

        /// <summary>
        /// Passes a valid <see cref="GameSetUpInformation"/> instance as the constructor parameter. Should 
        /// create an <see cref="object"/> of type <see cref="Board"/> with the correct Grid and Ships array.
        /// </summary>
        [Fact]
        public void MainConstructor_ValidGameSetUpInformation_CreatesInstanceSuccessfully()
        {
            //Arrange & Act
            var board = new Board(_gameSetUpInformation, true);

            //Assert
            Assert.IsType<Board>(board);
            Assert.Equal(GridSize, board.Grid.Length);
            Assert.Equal(ShipsTotal, board.Ships.Length);
            Assert.Equal([0, 1, 2, 3], board.Ships[0].Positions);
        }

        /// <summary>
        /// Passes a valid <see cref="BoardDTO"/> as the parameter for the constructor. Should create an 
        /// <see cref="object"/> of type <see cref="Board"/> with the correct Grid and Ships array.
        /// </summary>
        [Fact]
        public void DTOConstructor_ValidBoardDTO_CreatesInstanceSuccessfully()
        {
            //Arrange & Act
            var board = new Board(_boardDTO);

            //Assert
            Assert.IsType<Board>(board);
            Assert.Equal(GridSize, board.Grid.Length);
            Assert.Equal(ShipsTotal, board.Ships.Length);
            Assert.Equal([7, 17, 27, 37], board.Ships[0].Positions);
        }
        #endregion //Constructor Tests

        #region Method Tests
        /// <summary>
        /// Creates an instance of <see cref="Board"/> and calls the GenerateSingleTurnReport method. Check that 
        /// the report is of the correct type and contains the correct positions.
        /// </summary>
        /// <param name="gridPosition">The grid position targeted.</param>
        /// <param name="shotType">The type of shot to be reported.</param>
        /// <param name="expectedReportPositions">A list of positions that the selected shot should hit.</param>
        /// <remarks>All positions should be missed in this test.</remarks>
        [Theory]
        [MemberData(nameof(GetSingleTurnReportShotData))]
        public void GenerateSingleTurnReport_ValidShots_ReturnsValidReportWithCorrectPositions
            (int gridPosition, ShotType shotType, List<int> expectedReportPositions)
        {
            //Arrange
            var board = new Board(_gameSetUpInformation, true);

            //Act
            var report = board.GenerateSingleTurnReport(gridPosition, shotType);

            //Assert
            Assert.IsType<SingleTurnReport>(report);
            Assert.Equal(expectedReportPositions, report.PositionsMissed);
        }

        /// <summary>
        /// Calls the GenerateSingleTurnReport method with different shot types and grid positions. Checks that 
        /// the correct hits and misses are recorded as well as the information for any sunk ships.
        /// </summary>
        /// <param name="gridPosition">The index of the targeted cell.</param>
        /// <param name="shotType">The type of shot selected.</param>
        /// <param name="expectedMisses">A list of positions for expected misses.</param>
        /// <param name="expectedHits">A list of positions for expected hits.</param>
        /// <param name="expectedSinkings">A list of information for expected sinkings.</param>
        [Theory]
        [MemberData(nameof(GetSingleTurnReportFullData))]
        public void GenerateSingleTurnReport_HitsAndMisses_ReturnsReportsWithCorrectLists
            (int gridPosition, 
            ShotType shotType, 
            List<int> expectedMisses, 
            List<int> expectedHits, 
            List<(int position, bool isHorizontal, ShipType shipType)> expectedSinkings)
        {
            //Arrange
            var board = new Board(_gameSetUpInformation, true);

            //Act
            var report = board.GenerateSingleTurnReport(gridPosition, shotType);

            //Assert
            Assert.IsType<SingleTurnReport>(report);
            Assert.Equal(expectedMisses, report.PositionsMissed);
            Assert.Equal(expectedHits, report.PositionsHit);
            Assert.Equal(expectedSinkings, report.ShipsSunk);
        }

        /// <summary>
        /// Generates multiple dictionary of random ship positions. Checks each dictionary to ensure that its 
        /// ships cover the expected number of grid cells. Should be the same total number of dictionaries 
        /// created as valid dictionaries created.
        /// </summary>
        [Fact]
        public void GenerateRandomShips_ShipsCanTouch_NoOverlapsOnMultipleGenerations()
        {
            //Arrange
            var board = new Board(_gameSetUpInformation, true);
            var totalValidGenerations = 0;

            //Act
            for (int _ = 0; _ < MultipleTestsCount; _++)
            {
                Dictionary<ShipType, (int position, bool isHorizontal)> ships = board.GenerateRandomShips();

                var totalPositions = ships
                        .SelectMany(ship => 
                            GenerateAllShipPositions(ship.Value.position, ship.Value.isHorizontal, _shipSizes[ship.Key]))
                        .Count();
                if (totalPositions == TotalShipSize)
                    totalValidGenerations++;
            }

            //Assert
            Assert.Equal(MultipleTestsCount, totalValidGenerations);
        }

        /// <summary>
        /// Generates multiple dictionary of random ship positions with no ships touching. Checks each 
        /// dictionary to ensure that its ships cover the expected number of grid cells and that no ship cells 
        /// are touching. Should be the same total number of dictionaries created as valid dictionaries created.
        /// </summary>
        [Fact]
        public void GenerateRandomShips_NoShipsCanTouch_NoOverlapsOrTouchingOnMultipleGenerations()
        {
            //Arrange
            var board = new Board(_gameSetUpInformationNoShipsTouching, true);
            var totalValidGenerations = 0;

            //Act
            for (int _ = 0; _ < MultipleTestsCount; _++)
            {
                Dictionary<ShipType, (int position, bool isHorizontal)> ships = board.GenerateRandomShips();

                var shipLists = ships
                    .Select(ship =>
                            GenerateAllShipPositions(ship.Value.position, ship.Value.isHorizontal, _shipSizes[ship.Key]));

                var shipPositions = shipLists
                    .SelectMany(list => list);

                var touchingPositions = shipLists
                    .SelectMany(list => GenerateAllTouchingPositions(list.ToHashSet()))
                    .Distinct();

                var totalPositions = shipPositions.Count();

                if (totalPositions == TotalShipSize && shipPositions.Intersect(touchingPositions).Count() == 0)
                    totalValidGenerations++;
            }

            //Assert
            Assert.Equal(MultipleTestsCount, totalValidGenerations);
        }

        /// <summary>
        /// Calls the CheckShipStatusToCalculateShots method with different shot calculation rules and checks 
        /// that it returns the expected number of shots.
        /// </summary>
        /// <param name="shotCalculationMethod">The type of <see cref="SalvoShots"/> calculation method.</param>
        /// <param name="expectedShotsNumber">The expected number of shots to check against.</param>
        [Theory]
        [InlineData (SalvoShots.None, 1)]
        [InlineData(SalvoShots.EqualsUnsunkShips, 4)]
        [InlineData(SalvoShots.EqualsUndamagedShips, 3)]
        [InlineData(SalvoShots.EqualsLargestUnsunkShip, 5)]
        [InlineData(SalvoShots.EqualsLargestUndamagedShip, 4)]
        public void CheckShipStatusToCalculateShots_DifferentShotCalculationMethods_ReturnsExpectedNumber(
            SalvoShots shotCalculationMethod, int expectedShotsNumber)
        {
            //Arrange
            var board = new Board(_boardDTO);

            //Act
            var shotsNumber = board.CheckShipStatusToCalculateShots(shotCalculationMethod);

            //Assert
            Assert.Equal(expectedShotsNumber, shotsNumber);
        }

        /// <summary>
        /// Calls the GetDTO method from a valid <see cref="Board"/> instance. Should be of type <see 
        /// cref="BoardDTO"/> and contain the same data.
        /// </summary>
        [Fact]
        public void GetDTO_ValidBoardInstance_CreatesDTOWithCorrectData()
        {
            //Arrange
            var board = new Board(_boardDTO);

            //Act
            var dto = board.GetDTO();

            //Assert
            Assert.IsType<BoardDTO>(dto);
            Assert.Equal(_boardDTO.Grid, dto.Grid);
            Assert.Equal(_boardDTO.ShipsDTO.Length, dto.ShipsDTO.Length);
            Assert.Equal(_boardDTO.ShipsDTO[0].ShipType, dto.ShipsDTO[0].ShipType);
            Assert.Equal(_boardDTO.ShipsDTO[1].IsHorizontal, dto.ShipsDTO[1].IsHorizontal);
            Assert.Equal(_boardDTO.ShipsDTO[2].Positions, dto.ShipsDTO[2].Positions);
            Assert.Equal(_boardDTO.ShipsDTO[3].Size, dto.ShipsDTO[3].Size);
        }
        #endregion //Method Tests

        #region MemberData
        public static IEnumerable<object[]> GetSingleTurnReportShotData()
        {
            yield return new object[] { 10, ShotType.Single, new List<int>() { 10 } };
            yield return new object[] { 97, ShotType.AirstrikeUpRight, new List<int>() { 97, 88, 79 } };
            yield return new object[] { 4, ShotType.AirstrikeDownRight, new List<int>() { 4, 15, 26 } };
            yield return new object[] { 44, ShotType.Bombardment, new List<int>() { 44, 45, 43, 54, 34 } };
        }
        public static IEnumerable<object[]> GetSingleTurnReportFullData()
        {
            yield return new object[] { 0, ShotType.Single, new List<int>(), new List<int>() { 0 }, new List<(int, bool, ShipType)>() };
            yield return new object[] { 5, ShotType.Single, new List<int>() { 5 }, new List<int>(), new List<(int, bool, ShipType)>() };
            yield return new object[] { 20, ShotType.AirstrikeUpRight, new List<int>() { 11 }, new List<int>() { 20, 2 }, new List<(int, bool, ShipType)>() };
            yield return new object[] { 17, ShotType.AirstrikeDownRight, new List<int>() { 17, 28 }, new List<int>() { 39 }, new List<(int, bool, ShipType)>() };
            yield return new object[] { 41, ShotType.Bombardment, new List<int>() { 51, 31 }, new List<int>() { 41, 42, 40 }, new List<(int, bool, ShipType)>() { (40, true, ShipType.Cruiser) } };
        }
        #endregion //MemberData

        #region Helper Methods
        /// <summary>
        /// Generates a list of all positions covered by a ship.
        /// </summary>
        /// <param name="position">The start position.</param>
        /// <param name="isHorizontal">A boolean value indicating whether the ship is horizontal.</param>
        /// <param name="size">The number of grid cells covered by the ship.</param>
        /// <returns>A list of integer positions covered by the ship.</returns>
        private List<int> GenerateAllShipPositions(int position, bool isHorizontal, int size)
        {
            var allPositions = new List<int>();
            var step = isHorizontal ? 1 : 10;

            for (int i = 0; i < size * step; i += step)
                allPositions.Add(position + i);

            return allPositions;
        }
        /// <summary>
        /// Checks each cell covered by a <see cref="Ship"/> to find touching cells. Uses validation to check 
        /// that all touching cells are in bounds and not part of a <see cref="Ship"/>.
        /// </summary>
        /// <param name="shipPositions">The positions covered by a <see cref="Ship"/> to be checked.</param>
        /// <returns>A list of integer positions touching the <see cref="Ship"/>.</returns>
        private List<int> GenerateAllTouchingPositions(HashSet<int> shipPositions)
        {
            var touchingPositions = new List<int>();

            foreach (var position in shipPositions)
            {
                int row = position / 10;
                int column = position % 10;

                if (row >= 10 && !shipPositions.Contains(position - 10))
                    touchingPositions.Add(position - 10);

                if (row < 90 && !shipPositions.Contains(position + 10))
                    touchingPositions.Add(position + 10);

                if (column > 0 && !shipPositions.Contains(position - 1))
                    touchingPositions.Add(position - 1);

                if (column < 9 && !shipPositions.Contains(position + 1))
                    touchingPositions.Add(position + 1);
            }

            return touchingPositions;
        }
        #endregion //Helper Methods
    }
}
