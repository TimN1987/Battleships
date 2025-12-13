using Battleships.MVVM.Enums;
using Battleships.MVVM.Factories;
using Battleships.MVVM.Model;
using Battleships.MVVM.Model.DataTransferObjects;
using Battleships.MVVM.Services;
using Battleships.MVVM.Structs;
using Moq;

namespace BattleshipsTests.Tests.Models
{
    public class GameTests
    {
        #region Constants
        private const int AirstrikeRequiredHits = 5;
        private const int BombardmentRequiredHits = 7;
        private const int InvalidShotPosition = -1;
        #endregion //Constants

        #region Fields
        private readonly Mock<ILoggerFactory> _loggerFactory;
        private readonly Mock<IEventLogger> _eventLogger;

        private readonly GameSetUpInformation _gameSetUpInformationClassic;
        private readonly GameSetUpInformation _gameSetUpInformationSalvo;
        private readonly RandomShotPickerDTO _randomShotPickerDTO;
        private readonly BoardDTO _boardDTO;
        private readonly ComputerPlayerDTO _computerPlayerDTO;
        private readonly GameDTO _gameDTO;
        private readonly BoardDTO _lastShotBoardDTO;
        private readonly GameDTO _lastShotGameDTO;
        #endregion //Fields

        public GameTests()
        {
            _loggerFactory = new Mock<ILoggerFactory>();
            _eventLogger = new Mock<IEventLogger>();

            _loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(_eventLogger.Object);

            _gameSetUpInformationClassic = new(
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
            _gameSetUpInformationSalvo = new(
            GameType.Salvo,
            GameDifficulty.Easy,
            true, true, true, true, false, false, false,
            SalvoShots.Fixed,
            new()
            {
                {ShipType.Battleship, (0, true) },
                {ShipType.Carrier, (20, true) },
                {ShipType.Cruiser, (40, true) },
                {ShipType.Destroyer, (9, false) },
                {ShipType.Submarine, (39, false) }
            });
            _randomShotPickerDTO = new()
            {
                AvailableShots = [.. Enumerable.Range(0, 100)],
                AvailableDiagonalSpacingTwoShots = [.. Enumerable.Range(0, 100)
                .Where(number => (number / 10 + number % 10) % 2 == 0)],
                AvailableDiagonalSpacingThreeShots = [.. Enumerable.Range(0, 100)
                .Where(number => (number / 10 + number % 10) % 3 == 0)],
                AvailableDiagonalSpacingFourShots = [.. Enumerable.Range(0, 100)
                .Where(number => (number / 10 + number % 10) % 4 == 0)],
                AvailableDiagonalSpacingFiveShots = [.. Enumerable.Range(0, 100)
                .Where(number => (number / 10 + number % 10) % 5 == 0)]
            };
            _boardDTO = new()
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
            _computerPlayerDTO = new()
            {
                RandomShotPickerDTO = _randomShotPickerDTO,
                GameDifficulty = GameDifficulty.Easy,
                ShipsCanTouch = true,
                ProbabilityDensityMap = new int[100].Select(value => 100).ToArray(),
                Directions = [1, -1, 10, -10],
                AvailablePositions = [.. Enumerable.Range(0, 100)],
                AirstrikeAllowed = true,
                BombardmentAllowed = true,
                AirstrikeHitCount = 0,
                BombardmentHitCount = 0,
                MaximumShipSize = 5,
                CompletedShots = [],
            };
            _gameDTO = new()
            {
                PlayerBoardDTO = _boardDTO,
                ComputerBoardDTO = _boardDTO,
                ComputerPlayerDTO = _computerPlayerDTO,
                GameDifficulty = GameDifficulty.Easy,
                AirstrikeAllowed = true,
                BombardmentAllowed = true,
                ShotsRemaining = 1,
                IsPlayerTurn = false,
                AirstrikeHitCount = 0,
                BombardmentHitCount = 0,
                LastComputerMove = new SingleTurnReport(),
                BonusShotIfSunk = false,
                FireUntilMiss = false,
                SalvoShotType = SalvoShots.EqualsUnsunkShips
            };
            _lastShotBoardDTO = new()
            {
                Grid = new GridCellState[100],
                ShipsDTO =
                [
                    new()
                {
                    ShipType = ShipType.Battleship,
                    Size = 4,
                    Damage = [true, true, true, true],
                    Positions = [7, 17, 27, 37],
                    IsHorizontal = false
                },
                new()
                {
                    ShipType = ShipType.Carrier,
                    Size = 5,
                    Damage = [true, true, true, true, true],
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
                    Damage = [true, true],
                    Positions = [5, 15],
                    IsHorizontal = false
                },
                new()
                {
                    ShipType = ShipType.Submarine,
                    Size = 3,
                    Damage = [true, true, false],
                    Positions = [90, 91, 92],
                    IsHorizontal = true
                }
                ]
            };
            _lastShotGameDTO = new()
            {
                PlayerBoardDTO = _boardDTO,
                ComputerBoardDTO = _lastShotBoardDTO,
                ComputerPlayerDTO = _computerPlayerDTO,
                GameDifficulty = GameDifficulty.Easy,
                AirstrikeAllowed = true,
                BombardmentAllowed = true,
                ShotsRemaining = 1,
                IsPlayerTurn = false,
                AirstrikeHitCount = 0,
                BombardmentHitCount = 0,
                LastComputerMove = new SingleTurnReport(),
                BonusShotIfSunk = false,
                FireUntilMiss = false,
                SalvoShotType = SalvoShots.EqualsUnsunkShips
            };
        }

        #region Constructor Tests
        /// <summary>
        /// Attempts to pass a null parameter to the <see cref="Game"/> subclass constructors. Should throw a 
        /// <see cref="NullReferenceException"/>.
        /// </summary>
        [Fact]
        public void Constructor_NullParameter_ThrowsNullReferenceException()
        {
            //Arrange, Act & Assert
            Assert.Throws<NullReferenceException>(() => new ClassicGame(_loggerFactory.Object, null!));
            Assert.Throws<NullReferenceException>(() => new SalvoGame(_loggerFactory.Object, null!));
        }

        /// <summary>
        /// Passes <see cref="GameSetUpInformation"/> as the parameter for the <see cref="Game"/> subclass 
        /// constructors. Should create an instance of the correct type with the correct data.
        /// </summary>
        [Fact]
        public void MainConstructor_ValidGameSetUpInformation_CreatesInstanceSuccessfully()
        {
            //Arrange & Act
            var classicGame = new ClassicGame(_loggerFactory.Object, _gameSetUpInformationClassic);
            var salvoGame = new SalvoGame(_loggerFactory.Object, _gameSetUpInformationSalvo);

            //Assert
            Assert.IsType<ClassicGame>(classicGame);
            Assert.IsType<SalvoGame>(salvoGame);

            Assert.Equal(0, classicGame.HitCountAirstrike);
            Assert.Equal(0, classicGame.HitCountBombardment);

            Assert.Equal(0, salvoGame.HitCountAirstrike);
            Assert.Equal(0, salvoGame.HitCountBombardment);
        }

        /// <summary>
        /// Passes a valid <see cref="GameDTO"/> as the parameter for the <see cref="Game"/> subclass 
        /// constructors. Should create a valid instance with the correct data.
        /// </summary>
        [Fact]
        public void DTOConstructor_ValidDTOParameter_CreatesInstanceSuccessfully()
        {
            //Arrange & Act
            var classicGame = new ClassicGame(_loggerFactory.Object, _gameDTO);
            var salvoGame = new SalvoGame(_loggerFactory.Object, _gameDTO);

            //Assert
            Assert.IsType<ClassicGame>(classicGame);
            Assert.IsType<SalvoGame>(salvoGame);

            Assert.Equal(0, classicGame.HitCountAirstrike);
            Assert.Equal(0, classicGame.HitCountBombardment);

            Assert.Equal(0, salvoGame.HitCountAirstrike);
            Assert.Equal(0, salvoGame.HitCountBombardment);
        }
        #endregion //Constructor Tests

        #region Method Tests
        /// <summary>
        /// Attempts to pass an invalid grid position to the ProcessPlayerShotSelection method. Checks that it 
        /// throws an <see cref="InvalidOperationException"/> and logs a warning.
        /// </summary>
        [Fact]
        public void ProcessPlayerShotSelection_InvalidPosition_LogsWarningThrowsInvalidOperationException()
        {
            //Arrange
            var classicGame = new ClassicGame(_loggerFactory.Object, _gameSetUpInformationClassic);

            //Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                classicGame.ProcessPlayerShotSelection(InvalidShotPosition, ShotType.Single));
            _eventLogger.Verify(message => message.LogWarning("Player able to enter invalid shot position.", null));

        }

        /// <summary>
        /// Calls GenerateTurnReport method for a <see cref="ClassicGame"/> and a <see cref="SalvoGame"/>. Checks 
        /// that a <see cref="SingleTurnReport"/> is generated by each with the correct number of shots recorded.
        /// </summary>
        [Fact]
        public void GenerateTurnReport_ValidShotsEntered_CorrectReportGenerated()
        {
            //Arrange
            var classicGame = new ClassicGame(_loggerFactory.Object, _gameSetUpInformationClassic);
            var salvoGame = new SalvoGame(_loggerFactory.Object, _gameSetUpInformationSalvo);
            var board = new Board(_boardDTO);

            //Act
            var classicReport = classicGame.GenerateTurnReport(0, ShotType.Single, board);
            var salvoReport = salvoGame.GenerateTurnReport(0, ShotType.Single, board);

            //Assert
            Assert.IsType<SingleTurnReport>(classicReport);
            Assert.Equal(1, classicReport.PositionsHit.Count + classicReport.PositionsMissed.Count);

            Assert.IsType<SingleTurnReport>(salvoReport);
            Assert.Equal(1, salvoReport.PositionsHit.Count + salvoReport.PositionsMissed.Count);
        }

        /// <summary>
        /// Calls RunComputerMove for a <see cref="ClassicGame"/>. Checks that a list of <see 
        /// cref="SingleTurnReport"/>s is returned with the correct number of shots recorded in the first report.
        /// </summary>
        [Fact]
        public void RunComputerMove_ClassicGame_ValidReportCreated()
        {
            //Arrange
            var classicGame = new ClassicGame(_loggerFactory.Object, _gameSetUpInformationClassic);

            //Act
            var reportsList = classicGame.RunComputerMove(out bool gameOver);

            //Assert
            Assert.NotEmpty(reportsList);
            Assert.IsType<SingleTurnReport>(reportsList[0]);
            Assert.Equal(1, reportsList[0].PositionsHit.Count + reportsList[0].PositionsMissed.Count);
        }

        /// <summary>
        /// Calls RunComputerMove for a <see cref="SalvoGame"/>. Checks that a list of <see 
        /// cref="SingleTurnReport"/>s is returned with the correct number of shots recorded in the first report. 
        /// The list should have five items on for the five computer salvo shots from an opening turn.
        /// </summary>
        [Fact]
        public void RunComputerMove_SalvoGame_ValidReportCreated()
        {
            //Arrange
            var salvoGame = new SalvoGame(_loggerFactory.Object, _gameSetUpInformationSalvo);

            //Act
            var reportsList = salvoGame.RunComputerMove(out bool gameOver);

            //Assert
            Assert.NotEmpty(reportsList);
            Assert.IsType<SingleTurnReport>(reportsList[0]);
            Assert.Equal(1, reportsList[0].PositionsHit.Count + reportsList[0].PositionsMissed.Count);
            Assert.False(gameOver);
            Assert.True(reportsList.Count > 1);
            Assert.Equal(5, reportsList.Count);
        }

        /// <summary>
        /// Calls RunComputerMove for a <see cref="SalvoGame"/> with some ships sunk. Checks that a list of <see 
        /// cref="SingleTurnReport"/>s is returned with the correct number of shots recorded in the first report. 
        /// The list should have the correct number of reports for the number of shots.
        /// </summary>
        [Fact]
        public void RunComputerMove_ReducedShots_CorrectNumberOfReportsCreated()
        {
            //Arrange
            var salvoGame = new SalvoGame(_loggerFactory.Object, _gameDTO); //_gameDTO has four unsunk ships on each board.

            //Act
            var reportsList = salvoGame.RunComputerMove(out bool gameOver);

            //Assert
            Assert.NotEmpty(reportsList);
            Assert.IsType<SingleTurnReport>(reportsList[0]);
            Assert.Equal(1, reportsList[0].PositionsHit.Count + reportsList[0].PositionsMissed.Count);
            Assert.False(gameOver);
            Assert.Equal(4, reportsList.Count);
        }


        /// <summary>
        /// Calls the ProcessPlayerShotSelection with different player shots. Checks that a valid <see 
        /// cref="AttackStatusReport"/> is returned.
        /// </summary>
        /// <param name="gridPosition">The targeted grid position to test.</param>
        /// <param name="shotType">The shot type to test.</param>
        /// <param name="expectedClassicReport">The minimum expected <see cref="AttackStatusReport"/> for the 
        /// <see cref="ClassicGame"/>.</param>
        /// <param name="expectedSalvoReport">The expected <see cref="AttackStatusReport"/> for the <see 
        /// cref="SalvoGame"/>.</param>
        /// <remarks>Depending on conditions, the computer player may make moves and generates extra reports.</remarks>
        [Theory]
        [MemberData(nameof(GetProcessPlayerShotData))]
        public void ProcessPlayerShotSelection_DifferentShotTypesAndPositions_ReturnsCorrectAttackStatusReport(
            int gridPosition,
            ShotType shotType,
            int expectedShotsTotal,
            AttackStatusReport expectedClassicReport,
            AttackStatusReport expectedSalvoReport)
        {
            //Arrange
            var classicGame = new ClassicGame(_loggerFactory.Object, _gameSetUpInformationClassic);
            var salvoGame = new SalvoGame(_loggerFactory.Object, _gameSetUpInformationSalvo);

            //Act
            var classicReport = classicGame.ProcessPlayerShotSelection(gridPosition, shotType);
            var salvoReport = salvoGame.ProcessPlayerShotSelection(gridPosition, shotType);

            var classicShotsTotal = classicReport.Reports[0].PositionsMissed.Count + classicReport.Reports[0].PositionsHit.Count;
            var salvoShotsTotal = salvoReport.Reports[0].PositionsMissed.Count + salvoReport.Reports[0].PositionsHit.Count;

            //Assert
            Assert.IsType<AttackStatusReport>(classicReport);
            Assert.IsType<AttackStatusReport>(salvoReport);

            Assert.Equal(expectedShotsTotal, classicShotsTotal);
            Assert.Equal(expectedShotsTotal, salvoShotsTotal);

            Assert.Equal(expectedClassicReport.IsGameOver, classicReport.IsGameOver);
            Assert.Equal(expectedSalvoReport.IsGameOver, salvoReport.IsGameOver);

            Assert.True(expectedClassicReport.Reports.Count <= classicReport.Reports.Count);
            Assert.True(expectedSalvoReport.Reports.Count <= salvoReport.Reports.Count);

            Assert.NotEmpty(classicReport.Reports);
            Assert.NotEmpty(salvoReport.Reports);
        }

        /// <summary>
        /// Sets up a <see cref="ClassicGame"/> and a <see cref="SalvoGame"/> instance with only grid cell 92 
        /// to hit to win the game. Calls the ProcessPlayerShotSelection with the shot including cell 92. Checks 
        /// that the game over bool returned is true and that no computer moves are run.
        /// </summary>
        [Theory]
        [InlineData(92, ShotType.Single)]
        [InlineData(92, ShotType.AirstrikeUpRight)]
        [InlineData(70, ShotType.AirstrikeDownRight)]
        [InlineData(82, ShotType.Bombardment)]
        public void ProcessPlayerShotSelection_FinalShot_GameOverReturnedNoComputerMove(
            int targetPosition,
            ShotType shotType)
        {
            //Arrange
            var classicGame = new ClassicGame(_loggerFactory.Object, _lastShotGameDTO);
            var salvoGame = new SalvoGame(_loggerFactory.Object, _lastShotGameDTO);

            //Act
            var classicReport = classicGame.ProcessPlayerShotSelection(targetPosition, shotType);
            var salvoReport = salvoGame.ProcessPlayerShotSelection(targetPosition, shotType);

            //Assert
            Assert.IsType<AttackStatusReport>(classicReport);
            Assert.IsType<AttackStatusReport>(salvoReport);

            Assert.True(classicReport.IsGameOver);
            Assert.True(salvoReport.IsGameOver);

            Assert.Single(classicReport.Reports);
            Assert.Single(salvoReport.Reports);
        }

        /// <summary>
        /// Calls GetDTO method on a <see cref="ClassicGame"/> and a <see cref="SalvoGame"/> instance. Should 
        /// create a valid <see cref="GameDTO"/> with the same data as the set up DTO.
        /// </summary>
        [Fact]
        public void GetDTO_ValidGameInstance_CreatesDTOWithCorrectData()
        {
            //Arrange
            var classicGame = new ClassicGame(_loggerFactory.Object, _gameDTO);
            var salvoGame = new SalvoGame(_loggerFactory.Object, _gameDTO);

            //Act
            var classicDTO = classicGame.GetDTO();
            var salvoDTO = salvoGame.GetDTO();

            //Assert
            Assert.IsType<GameDTO>(classicDTO);
            Assert.IsType<GameDTO>(salvoDTO);

            Assert.Equal(_gameDTO.GameDifficulty, classicDTO.GameDifficulty);
            Assert.Equal(_gameDTO.GameDifficulty, salvoDTO.GameDifficulty);

            Assert.Equal(_gameDTO.BombardmentAllowed, classicDTO.BombardmentAllowed);
            Assert.Equal(_gameDTO.BombardmentAllowed, salvoDTO.BombardmentAllowed);

            Assert.Equal(_gameDTO.AirstrikeHitCount, classicDTO.AirstrikeHitCount);
            Assert.Equal(_gameDTO.AirstrikeHitCount, salvoDTO.AirstrikeHitCount);
        }

        #endregion //Method Tests

        #region MemberData
        public static IEnumerable<object[]> GetProcessPlayerShotData()
        {
            yield return new object[] { 0, ShotType.Single, 1,
                new AttackStatusReport(false, [new SingleTurnReport()] ),
                new AttackStatusReport(false, [new SingleTurnReport()]) };
            yield return new object[] { 20, ShotType.AirstrikeUpRight, 3,
                new AttackStatusReport(false, [new SingleTurnReport()] ),
                new AttackStatusReport(false, [new SingleTurnReport()]) };
            yield return new object[] { 75, ShotType.AirstrikeDownRight, 3,
                new AttackStatusReport(false, [new SingleTurnReport()] ),
                new AttackStatusReport(false, [new SingleTurnReport()]) };
            yield return new object[] { 87, ShotType.Bombardment, 5,
                new AttackStatusReport(false, [new SingleTurnReport()] ),
                new AttackStatusReport(false, [new SingleTurnReport()]) };
        }
        #endregion //MemberData
    }
}
