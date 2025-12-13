using Battleships.MVVM.Enums;
using Battleships.MVVM.Model;
using Battleships.MVVM.Model.DataTransferObjects;
using Battleships.MVVM.Utilities;

namespace BattleshipsTests.Tests.Models
{
    public class RandomShotPickerTests
    {
        #region Fields
        private const int MaximumSpacingSize = 5;
        private const int OneShotAvailablePosition = 1;
        private static readonly RandomShotPickerDTO _randomShotPickerDTO = new()
        {
            AvailableShots = [0, 1, 2, 10, 11, 12, 20, 21, 22, 30, 31, 32, 99],
            AvailableDiagonalSpacingTwoShots = [0, 11, 20, 22, 31, 99],
            AvailableDiagonalSpacingThreeShots = [0, 12, 21, 30, 99],
            AvailableDiagonalSpacingFourShots = [0, 22, 31],
            AvailableDiagonalSpacingFiveShots = [0, 32]
        };
        private static readonly RandomShotPickerDTO _noShotsDTO = new()
        {
            AvailableShots = [],
            AvailableDiagonalSpacingTwoShots = [],
            AvailableDiagonalSpacingThreeShots = [],
            AvailableDiagonalSpacingFourShots = [],
            AvailableDiagonalSpacingFiveShots = []
        };
        private static readonly RandomShotPickerDTO _oneShotAvailableDTO = new()
        {
            AvailableShots = [1],
            AvailableDiagonalSpacingTwoShots = [],
            AvailableDiagonalSpacingThreeShots = [],
            AvailableDiagonalSpacingFourShots = [],
            AvailableDiagonalSpacingFiveShots = []
        };
        #endregion //Fields

        #region Constructor Tests
        /// <summary>
        /// Calls the <see cref="RandomShotPicker"/> constructor with a null parameter. Should throw a <see 
        /// cref="NullReferenceException"/>.
        /// </summary>
        [Fact]
        public void Constructor_NullParameter_ThrowsNullReferenceException()
        {
            //Arrange & Act & Assert
            Assert.Throws<NullReferenceException>(() => new RandomShotPicker(null!));
        }

        /// <summary>
        /// Calls the parameterless constructor for <see cref="RandomShotPicker"/>. Checks that it creates an 
        /// object of type <see cref="RandomShotPicker"/>.
        /// </summary>
        [Fact]
        public void ParameterlessConstructor_NoParameter_CreatesInstanceSuccessfully()
        {
            //Arrange & Act
            var randomShotPicker = new RandomShotPicker();

            //Assert
            Assert.IsType<RandomShotPicker>(randomShotPicker);
        }

        /// <summary>
        /// Calls the DTO constructor for <see cref="RandomShotPicker"/>. Checks that it creates an object of 
        /// type <see cref="RandomShotPicker"/>.
        /// </summary>
        [Fact]
        public void DTOConstructor_ValidDTO_CreatesInstanceSuccessfully()
        {
            //Arrange & Act
            var randomShotPicker = new RandomShotPicker(_randomShotPickerDTO);

            //Assert
            Assert.IsType<RandomShotPicker>(randomShotPicker);
        }
        #endregion //Constructor Tests

        #region Method Tests
        /// <summary>
        /// Calls the GenerateRandomShot method when there are shots available with space to play any type of 
        /// shot. Checks that the returned shot position was available and checks that the shot type is the 
        /// same type as expected.
        /// </summary>
        /// <param name="shotType">The type of shot to be tested.</param>
        /// <param name="spacing">The spacing between diagonal lines to start with.</param>
        /// <param name="expectedShotType">The shot type that should fit into the available spaces.</param>
        [Theory]
        [InlineData(ShotType.Single, 5, ShotType.Single)]
        [InlineData(ShotType.Single, 4, ShotType.Single)]
        [InlineData(ShotType.Single, 3, ShotType.Single)]
        [InlineData(ShotType.Single, 2, ShotType.Single)]
        [InlineData(ShotType.Single, 0, ShotType.Single)]
        [InlineData(ShotType.Single, null, ShotType.Single)]
        [InlineData(ShotType.AirstrikeUpRight, 5, ShotType.AirstrikeUpRight)]
        [InlineData(ShotType.AirstrikeDownRight, 5, ShotType.AirstrikeDownRight)]
        [InlineData(ShotType.Bombardment, 5, ShotType.Bombardment)]
        public void GenerateRandomShot_ShotsAvailableWithSpaceForAllShotTypes_ReturnsValidShotCorrectType
            (ShotType shotType, int? spacing, ShotType expectedShotType)
        {
            //Arrange
            var randomShotPicker = new RandomShotPicker(_randomShotPickerDTO);

            //Act
            var shotPosition = randomShotPicker.GenerateRandomShot(ref shotType, spacing);

            //Assert
            Assert.Contains(shotPosition, _randomShotPickerDTO.AvailableShots);
            Assert.Equal(expectedShotType, shotType);
        }

        /// <summary>
        /// Calls the GenerateRandomShot method when there is only one shot available. Checks that the returned 
        /// shot positions equals the one available position and that the shot type is a single shot.
        /// </summary>
        /// <param name="shotType">The type of shot to be tested.</param>
        /// <param name="spacing">The spacing between diagonal lines to start with.</param>
        [Theory]
        [InlineData(ShotType.AirstrikeUpRight, 5)]
        [InlineData(ShotType.AirstrikeDownRight, 5)]
        [InlineData(ShotType.Bombardment, 5)]
        [InlineData(ShotType.Single, 5)]
        public void GenerateRandomShot_OnlyOneShotAvailable_ReturnsExpectedShotWithSingleShotType
            (ShotType shotType, int? spacing)
        {
            //Arrange
            var randomShotPicker = new RandomShotPicker(_oneShotAvailableDTO);

            //Act
            var shotPosition = randomShotPicker.GenerateRandomShot(ref shotType, spacing);

            //Assert
            Assert.Equal(OneShotAvailablePosition, shotPosition);
            Assert.Equal(ShotType.Single, shotType);
        }

        /// <summary>
        /// Creates a RandomShotPicker with no available shots. GenerateRandomShot should return -1 to indicate 
        /// that it could not create a random shot.
        /// </summary>
        [Fact]
        public void GenerateRandomShot_NoShotsAvailable_ReturnsMinusOne()
        {
            //Arrange
            var randomShotPicker = new RandomShotPicker(_noShotsDTO);
            var shotType = ShotType.Single;

            //Act
            var shotPosition = randomShotPicker.GenerateRandomShot(ref shotType, MaximumSpacingSize);

            //Assert
            Assert.Equal(-1, shotPosition);
        }

        /// <summary>
        /// Calls the GenerateRandomShot method fifty times with random shot types. Counts errors for any overlaps 
        /// or duplicated positions. Checks that errors total zero.
        /// </summary>
        [Fact]
        public void GenerateRandomShot_CompleteMultipleShots_NoRepeatsOrOverlap()
        {
            //Arrange
            var randomShotPicker = new RandomShotPicker();
            var availablePositions = Enumerable.Range(0, 100).ToList();
            var shotType = ShotType.Single;
            var errors = 0;

            //Act
            for (int i = 0; i < 50; i++)
            {
                var shotChoiceNumber = RandomProvider.Instance.Next(20);

                shotType = shotChoiceNumber switch
                {
                    0 => ShotType.Bombardment,
                    1 => ShotType.AirstrikeUpRight,
                    2 => ShotType.AirstrikeDownRight,
                    _ => ShotType.Single
                };

                var shotPosition = randomShotPicker.GenerateRandomShot(ref shotType, 5);

                if (!availablePositions.Remove(shotPosition))
                    errors++;

                var multiShotPositions = FindMultiShotPositions(shotPosition, shotType);

                foreach (var position in multiShotPositions)
                {
                    if (!availablePositions.Remove(position))
                        errors++;
                }

            }

            //Assert
            Assert.True(errors == 0);
            Assert.True(availablePositions.Count <= 50);
        }

        /// <summary>
        /// Calls the GetDTO method from a valid <see cref="RandomShotPicker"/> instance. Should create a DTO 
        /// with all the correct data inside.
        /// </summary>
        [Fact]
        public void GetDTO_ValidRandomShotPicker_CreatesInstanceSuccessfully()
        {
            //Arrange
            var randomShotPicker = new RandomShotPicker(_randomShotPickerDTO);

            //Act
            var dto = randomShotPicker.GetDTO();

            //Assert
            Assert.IsType<RandomShotPickerDTO>(dto);
            Assert.Equal(13, dto.AvailableShots.Count);
            Assert.Equal(6, dto.AvailableDiagonalSpacingTwoShots.Count);
            Assert.Equal(5, dto.AvailableDiagonalSpacingThreeShots.Count);
            Assert.Equal(3, dto.AvailableDiagonalSpacingFourShots.Count);
            Assert.Equal(2, dto.AvailableDiagonalSpacingFiveShots.Count);

            Assert.Equal([0, 32], dto.AvailableDiagonalSpacingFiveShots);
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
    }
}
