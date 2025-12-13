using Battleships.MVVM.Enums;
using Battleships.MVVM.Model;
using Battleships.MVVM.Model.DataTransferObjects;
using Battleships.MVVM.Structs;

namespace BattleshipsTests.Tests.Models
{
    public class ComputerPlayerTests
    {
        #region Fields
        private const int MultipleTestShotsNumber = 50;
        private static readonly GridCellState[] _testGrid = new GridCellState[100];
        private static readonly List<int> _remainingShipsAll = [5, 4, 3, 3, 2];
        private static readonly List<int> _allPossibleShots = [.. Enumerable.Range(0, 100)];
        private static readonly RandomShotPickerDTO _randomShotPickerDTO = new()
        {
            AvailableShots = [0, 1, 2, 10, 11, 12, 20, 21, 22, 30, 31, 32, 99],
            AvailableDiagonalSpacingTwoShots = [0, 11, 20, 22, 31, 99],
            AvailableDiagonalSpacingThreeShots = [0, 12, 21, 30, 99],
            AvailableDiagonalSpacingFourShots = [0, 22, 31],
            AvailableDiagonalSpacingFiveShots = [0, 32]
        };
        private static readonly RandomShotPickerDTO _randomShotPickerDTONewGame = new()
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
        private static readonly ComputerPlayerDTO _computerPlayerDTOEasy = new()
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
        private static readonly ComputerPlayerDTO _computerPlayerDTOMedium = new()
        {
            RandomShotPickerDTO = _randomShotPickerDTO,
            GameDifficulty = GameDifficulty.Medium,
            ShipsCanTouch = true,
            ProbabilityDensityMap = new int[100].Select(value => 100).ToArray(),
            Directions = [1, -1, 10, -10],
            AvailablePositions = [.. Enumerable.Range(0, 100)],
            AirstrikeAllowed = true,
            BombardmentAllowed = true,
            AirstrikeHitCount = 5,
            BombardmentHitCount = 0,
            MaximumShipSize = 5,
            CompletedShots = [],
        };
        private static readonly ComputerPlayerDTO _computerPlayerDTOHard = new()
        {
            RandomShotPickerDTO = _randomShotPickerDTO,
            GameDifficulty = GameDifficulty.Hard,
            ShipsCanTouch = true,
            ProbabilityDensityMap = new int[100].Select(value => 100).ToArray(),
            Directions = [1, -1, 10, -10],
            AvailablePositions = [.. Enumerable.Range(0, 100)],
            AirstrikeAllowed = true,
            BombardmentAllowed = true,
            AirstrikeHitCount = 0,
            BombardmentHitCount = 7,
            MaximumShipSize = 5,
            CompletedShots = [],
        };
        private static readonly ComputerPlayerDTO _computerPlayerDTOAI = new()
        {
            RandomShotPickerDTO = _randomShotPickerDTO,
            GameDifficulty = GameDifficulty.AI,
            ShipsCanTouch = true,
            ProbabilityDensityMap = new int[100].Select(value => 100).ToArray(),
            Directions = [1, -1, 10, -10],
            AvailablePositions = [.. Enumerable.Range(0, 100)],
            AirstrikeAllowed = true,
            BombardmentAllowed = true,
            AirstrikeHitCount = 5,
            BombardmentHitCount = 7,
            MaximumShipSize = 5,
            CompletedShots = [],
        };
        private static readonly ComputerPlayerDTO _computerPlayerDTOEasySingleShots = new()
        {
            RandomShotPickerDTO = _randomShotPickerDTO,
            GameDifficulty = GameDifficulty.Easy,
            ShipsCanTouch = true,
            ProbabilityDensityMap = new int[100].Select(value => 100).ToArray(),
            Directions = [1, -1, 10, -10],
            AvailablePositions = [.. Enumerable.Range(0, 100)],
            AirstrikeAllowed = false,
            BombardmentAllowed = false,
            AirstrikeHitCount = 5,
            BombardmentHitCount = 7,
            MaximumShipSize = 5,
            CompletedShots = [],
        };
        private static readonly ComputerPlayerDTO _computerPlayerDTOMediumSingleShots = new()
        {
            RandomShotPickerDTO = _randomShotPickerDTO,
            GameDifficulty = GameDifficulty.Medium,
            ShipsCanTouch = true,
            ProbabilityDensityMap = new int[100].Select(value => 100).ToArray(),
            Directions = [1, -1, 10, -10],
            AvailablePositions = [.. Enumerable.Range(0, 100)],
            AirstrikeAllowed = false,
            BombardmentAllowed = false,
            AirstrikeHitCount = 5,
            BombardmentHitCount = 7,
            MaximumShipSize = 5,
            CompletedShots = [],
        };
        private static readonly ComputerPlayerDTO _computerPlayerDTOHardSingleShots = new()
        {
            RandomShotPickerDTO = _randomShotPickerDTO,
            GameDifficulty = GameDifficulty.Hard,
            ShipsCanTouch = true,
            ProbabilityDensityMap = new int[100].Select(value => 100).ToArray(),
            Directions = [1, -1, 10, -10],
            AvailablePositions = [.. Enumerable.Range(0, 100)],
            AirstrikeAllowed = false,
            BombardmentAllowed = false,
            AirstrikeHitCount = 5,
            BombardmentHitCount = 7,
            MaximumShipSize = 5,
            CompletedShots = [],
        };
        private static readonly ComputerPlayerDTO _computerPlayerDTOAISingleShots = new()
        {
            RandomShotPickerDTO = _randomShotPickerDTO,
            GameDifficulty = GameDifficulty.AI,
            ShipsCanTouch = true,
            ProbabilityDensityMap = new int[100].Select(value => 100).ToArray(),
            Directions = [1, -1, 10, -10],
            AvailablePositions = [.. Enumerable.Range(0, 100)],
            AirstrikeAllowed = true,
            BombardmentAllowed = true,
            AirstrikeHitCount = 5,
            BombardmentHitCount = 7,
            MaximumShipSize = 5,
            CompletedShots = [],
        };
        private static readonly GameSetUpInformation _gameSetUpInformationEasy = new(
            GameType.Classic,
            GameDifficulty.Easy,
            true, true, true, true, true, true, true,
            SalvoShots.None,
            []);
        #endregion //Fields

        #region Constructor Tests
        /// <summary>
        /// Attempts to pass a null parameter to the <see cref="ComputerPlayer"/> constructor. Should throw a 
        /// <see cref="NullReferenceException"/>.
        /// </summary>
        [Fact]
        public void Constructor_NullParameter_ThrowsNullReferenceException()
        {
            //Arrange, Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                new ComputerPlayer(null!));
        }

        /// <summary>
        /// Attempts to pass a valid <see cref="GameSetUpInformation"/> parameter to the <see 
        /// cref="ComputerPlayer"/> constructor. Should create a valid instance with the correct data. Airstrike 
        /// and Bombardment should not be activated at the start of a game.
        /// </summary>
        [Fact]
        public void MainConstructor_ValidGameSetUpInformation_CreatesInstanceSuccessfully()
        {
            //Arrange & Act
            var computerPlayer = new ComputerPlayer(_gameSetUpInformationEasy);

            //Assert
            Assert.IsType<ComputerPlayer>(computerPlayer);
            Assert.False(computerPlayer.AirstrikeActivated);
            Assert.False(computerPlayer.BombardmentActivated);
        }

        /// <summary>
        /// Attempts to pass a <see cref="ComputerPlayerDTO"/> as the parameter for the <see cref="ComputerPlayer"/> 
        /// constructor. Should create a valid instance with correct data. Passes different values for the 
        /// airstrike and bombardment hit counts to return different values for AirstrikeActivated and 
        /// BombardmentActivated to check that the private fields are correctly set.
        /// </summary>
        /// <param name="dto">The <see cref="ComputerPlayerDTO"/> containing the computer player information.</param>
        /// <param name="expectedAirstrikeActivationStatus">The expected boolean value for the AirstrikeActivated 
        /// property.</param>
        /// <param name="expectedBombardmentActivationStatus">The expected boolean value for the 
        /// BombardmentActivated property.</param>
        [Theory]
        [MemberData(nameof(GetDTOConstructorData))]
        public void DTOConstructor_ValidDTOParameter_CreatesInstanceSuccessfully
            (ComputerPlayerDTO dto, bool expectedAirstrikeActivationStatus, bool expectedBombardmentActivationStatus)
        {
            //Arrange & Act
            var computerPlayer = new ComputerPlayer(dto);

            //Assert
            Assert.IsType<ComputerPlayer>(computerPlayer);
            Assert.Equal(expectedAirstrikeActivationStatus, computerPlayer.AirstrikeActivated);
            Assert.Equal(expectedBombardmentActivationStatus, computerPlayer.BombardmentActivated);
        }
        #endregion //Constructor Tests

        #region Method Tests
        /// <summary>
        /// Calls the ChooseNextMove method on <see cref="ComputerPlayer"/> instances with different difficulty 
        /// settings and multi target shot availability. Checks that the shot position is valid and that the 
        /// shot type is one of the possible types for the shot.
        /// </summary>
        /// <param name="dto">The <see cref="ComputerPlayerDTO"/> containing the computer player information.</param>
        /// <param name="possibleShotTypes">A list of possible shot types that the computer player could pick.</param>
        [Theory]
        [MemberData(nameof(GetShotSelectionData))]
        public void ChooseNextMove_EarlyGameState_ReturnsValidShotExpectedType(ComputerPlayerDTO dto, List<ShotType> possibleShotTypes)
        {
            //Arrange
            var computerPlayer = new ComputerPlayer(dto);

            //Act
            var shotPosition = computerPlayer.ChooseNextMove(_testGrid, _remainingShipsAll, new SingleTurnReport(), out ShotType actualShotType);

            //Assert
            Assert.Contains(shotPosition, _allPossibleShots);
            Assert.Contains(actualShotType, possibleShotTypes);
        }

        /// <summary>
        /// Calls the ChooseNextMove method with computer players who are not allowed Airstrike or Bombardment. 
        /// Checks that every shot is valid and of type single shot.
        /// </summary>
        /// <param name="dto">The <see cref="ComputerPlayerDTO"/> containing the computer player data.</param>
        [Theory]
        [MemberData(nameof(GetSingleShotPlayerDTO))]
        public void ChooseNextShot_MultiTargetShotsNotAllowed_AlwaysReturnsSingleShot(ComputerPlayerDTO dto)
        {
            //Arrange
            var computerPlayer = new ComputerPlayer(dto);

            //Act
            var shotPosition = computerPlayer.ChooseNextMove(_testGrid, _remainingShipsAll, new SingleTurnReport(), out ShotType shotType);

            //Assert
            Assert.Contains(shotPosition, _allPossibleShots);
            Assert.Equal(ShotType.Single, shotType);
        }

        /// <summary>
        /// Runs multiple shots for a computer player on different difficulty settings with different multi-
        /// target shots allowed. Counts the number of valid and unique shots played with penalties for any 
        /// repeated shots or overlapping multi-target shots. Should return one valid shot count for each call 
        /// of the ChooseNextMove method.
        /// </summary>
        /// <param name="dto">The <see cref="ComputerPlayerDTO"/> containing the computer player data.</param>
        [Theory]
        [MemberData(nameof(GetPlayerDTO))]
        public void ChooseNextMove_RepeatedShots_AllPositionsValidNoOverlap(ComputerPlayerDTO dto)
        {
            //Arrange
            dto.RandomShotPickerDTO = _randomShotPickerDTONewGame;
            var computerPlayer = new ComputerPlayer(dto);
            var availablePositions = Enumerable.Range(0, 100).ToList();
            var targetGrid = _testGrid.ToArray();
            var validShotsCount = 0;

            //Act
            for (int _ = 0; _ < MultipleTestShotsNumber; _++)
            {
                var shotPosition = computerPlayer.ChooseNextMove(targetGrid, _remainingShipsAll, new SingleTurnReport(), out ShotType shotType);
                targetGrid[shotPosition] = GridCellState.Miss;

                //Only adds a new valid shot count if the shot is valid and unique.
                if (availablePositions.Remove(shotPosition))
                    validShotsCount++;

                foreach (var position in FindMultiShotPositions(shotPosition, shotType))
                {
                    //Reduce the valid shots count for any invalid positions found. Identifies any overlaps.
                    if (!availablePositions.Remove(position))
                        validShotsCount--;
                }
            }

            //Assert
            Assert.Equal(MultipleTestShotsNumber, validShotsCount);
        }

        /// <summary>
        /// Runs the ChooseNextMove method with different difficulty levels and only two hits on the grid. Should 
        /// return a grid position adjacent to the hits. Hard difficulty computer players should continue the row of 
        /// hits at one end.
        /// </summary>
        /// <param name="dto">The <see cref="ComputerPlayerDTO"/> containing the computer player data.</param>
        /// <param name="expectedShots">An array of integers representing expected shots that the computer player 
        /// could choose.</param>
        [Theory]
        [MemberData(nameof(GetTargetPhasePlayerDTOs))]
        public void ChooseNextMove_TwoHitsOnGrid_SelectsValidTargetPhaseShot(ComputerPlayerDTO dto, int[] expectedShots)
        {
            //Arrange
            dto.RandomShotPickerDTO = _randomShotPickerDTONewGame;
            var targetGrid = _testGrid.ToArray();
            targetGrid[24] = GridCellState.Hit;
            targetGrid[25] = GridCellState.Hit;
            dto.AvailablePositions.Remove(24);
            dto.AvailablePositions.Remove(25);

            dto.ProbabilityDensityMap[24] = 0;
            dto.ProbabilityDensityMap[25] = 0;

            dto.ProbabilityDensityMap[14] = 110;
            dto.ProbabilityDensityMap[15] = 110;
            dto.ProbabilityDensityMap[23] = 110;
            dto.ProbabilityDensityMap[26] = 110;
            dto.ProbabilityDensityMap[34] = 110;
            dto.ProbabilityDensityMap[35] = 110;

            var computerPlayer = new ComputerPlayer(dto);

            //Act
            var shotPosition = computerPlayer.ChooseNextMove(targetGrid, _remainingShipsAll, new SingleTurnReport(), out ShotType shotType);


            //Assert
            Assert.True(shotPosition >= 0);
            Assert.True(shotPosition < 100);
            Assert.True(shotPosition != 24 && shotPosition != 25);
            Assert.Contains(shotPosition, expectedShots);
        }

        /// <summary>
        /// Attempts to call the GetDTO from a <see cref="ComputerPlayer"/> instance with valid data. Should 
        /// create a valid <see cref="ComputerPlayerDTO"/> instance with the correct value properties.
        /// </summary>
        [Fact]
        public void GetDTO_ValidComputerPlayerInstance_CreatesDTOWithCorrectData()
        {
            //Arrange
            var computerPlayer = new ComputerPlayer(_computerPlayerDTOEasy);

            //Act
            var dto = computerPlayer.GetDTO();

            //Assert
            Assert.IsType<ComputerPlayerDTO>(dto);
            Assert.Equal(_computerPlayerDTOEasy.GameDifficulty, dto.GameDifficulty);
            Assert.Equal(_computerPlayerDTOEasy.ShipsCanTouch, dto.ShipsCanTouch);
            Assert.Equal(_computerPlayerDTOEasy.ProbabilityDensityMap, dto.ProbabilityDensityMap);
            Assert.Equal(_computerPlayerDTOEasy.Directions, dto.Directions);
            Assert.Equal(_computerPlayerDTOEasy.AvailablePositions, dto.AvailablePositions);
            Assert.Equal(_computerPlayerDTOEasy.AirstrikeAllowed, dto.AirstrikeAllowed);
            Assert.Equal(_computerPlayerDTOEasy.BombardmentAllowed, dto.BombardmentAllowed);
            Assert.Equal(_computerPlayerDTOEasy.AirstrikeHitCount, dto.AirstrikeHitCount);
            Assert.Equal(_computerPlayerDTOEasy.BombardmentHitCount, dto.BombardmentHitCount);
            Assert.Equal(_computerPlayerDTOEasy.MaximumShipSize, dto.MaximumShipSize);
            Assert.Equal(_computerPlayerDTOEasy.CompletedShots, dto.CompletedShots);
        }
        #endregion //Method Tests

        #region Helper Methods
        private static List<int> FindMultiShotPositions(int targetPosition, ShotType shotType)
        {
            return shotType switch
            {
                ShotType.AirstrikeUpRight => [targetPosition - 9, targetPosition - 18],
                ShotType.AirstrikeDownRight => [targetPosition + 11, targetPosition + 22],
                ShotType.Bombardment => [targetPosition - 1, targetPosition + 1, targetPosition - 10, targetPosition + 10],
                _ => []
            };
        }
        #endregion //Helper Methods

        #region MemberData
        public static IEnumerable<object[]> GetDTOConstructorData()
        {
            yield return new object[] { _computerPlayerDTOEasy, false, false };
            yield return new object[] { _computerPlayerDTOMedium, true, false };
            yield return new object[] { _computerPlayerDTOHard, false, true };
            yield return new object[] { _computerPlayerDTOAI, true, true };
        }
        public static IEnumerable<object[]> GetShotSelectionData()
        {
            yield return new object[] { _computerPlayerDTOEasy, new List<ShotType>() { ShotType.Single } };
            yield return new object[] { _computerPlayerDTOMedium, new List<ShotType>() { ShotType.Single, ShotType.AirstrikeUpRight, ShotType.AirstrikeDownRight } };
            yield return new object[] { _computerPlayerDTOHard, new List<ShotType>() { ShotType.Single, ShotType.Bombardment } };
        }
        public static IEnumerable<object[]> GetSingleShotPlayerDTO()
        {
            yield return new object[] { _computerPlayerDTOEasySingleShots };
            yield return new object[] { _computerPlayerDTOMediumSingleShots };
            yield return new object[] { _computerPlayerDTOHardSingleShots };
        }
        public static IEnumerable<object[]> GetPlayerDTO()
        {
            yield return new object[] { _computerPlayerDTOEasy };
            yield return new object[] { _computerPlayerDTOMedium };
            yield return new object[] { _computerPlayerDTOHard };
            yield return new object[] { _computerPlayerDTOEasySingleShots };
            yield return new object[] { _computerPlayerDTOMediumSingleShots };
            yield return new object[] { _computerPlayerDTOHardSingleShots };
        }
        public static IEnumerable<object[]> GetTargetPhasePlayerDTOs()
        {
            yield return new object[] { _computerPlayerDTOEasy, Enumerable.Range(0, 24).Union(Enumerable.Range(26, 74)).ToArray() };
            yield return new object[] { _computerPlayerDTOMedium, new int[] { 14, 15, 23, 26, 34, 35 } };
            yield return new object[] { _computerPlayerDTOHard, new int[] { 13, 16, 33, 36 } };
            yield return new object[] { _computerPlayerDTOEasySingleShots, new int[] { 14, 15, 23, 26, 34, 35 } };
            yield return new object[] { _computerPlayerDTOMediumSingleShots, new int[] { 14, 15, 23, 26, 34, 35 } };
            yield return new object[] { _computerPlayerDTOHardSingleShots, Enumerable.Range(0, 24).Union(Enumerable.Range(26, 74)).ToArray() };
        }
        #endregion //MemberData
    }
}
