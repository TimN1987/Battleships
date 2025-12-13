using Battleships.MVVM.Enums;
using Battleships.MVVM.Model;
using Battleships.MVVM.Model.DataTransferObjects;

namespace BattleshipsTests.Tests.Models
{
    public class ShipTests
    {
        #region Fields
        private const int InvalidPosition = 105;

        private static readonly ShipDTO _battleshipDTO = new()
        {
            ShipType = ShipType.Battleship,
            Size = 4,
            Damage = [false, false, false, false],
            Positions = [7, 17, 27, 37],
            IsHorizontal = false
        };
        private static readonly ShipDTO _carrierDTO = new()
        {
            ShipType = ShipType.Carrier,
            Size = 5,
            Damage = [true, true, false, false, false],
            Positions = [10, 11, 12, 13, 14],
            IsHorizontal = true
        };
        private static readonly ShipDTO _cruiserDTO = new()
        {
            ShipType = ShipType.Cruiser,
            Size = 3,
            Damage = [true, true, true],
            Positions = [97, 98, 99],
            IsHorizontal = true
        };
        private static readonly ShipDTO _destroyerDTO = new()
        {
            ShipType = ShipType.Destroyer,
            Size = 2,
            Damage = [false, false],
            Positions = [5, 15],
            IsHorizontal = false
        };
        private static readonly ShipDTO _submarineDTO = new()
        {
            ShipType = ShipType.Submarine,
            Size = 3,
            Damage = [false, false, false],
            Positions = [90, 91, 92],
            IsHorizontal = true
        };
        #endregion //Fields

        #region Constructor Tests
        /// <summary>
        /// Creates instances of the different <see cref="Ship"/> subclasses. Checks that they are the correct 
        /// type and contain the correct data.
        /// </summary>
        /// <param name="startPosition">An integer representing the start position for the ship.</param>
        /// <param name="isHorizontal">A boolean value indicating whether the ship is aligned horizontally.</param>
        [Theory]
        [InlineData(3, true)]
        [InlineData(26, false)]
        [InlineData(90, true)]
        public void MainConstructor_ValidParameters_SuccessfullyCreatesInstance(int startPosition, bool isHorizontal)
        {
            //Arrange & Act
            Ship battleship = new Battleship((startPosition, isHorizontal));
            Ship carrier = new Carrier((startPosition, isHorizontal));
            Ship cruiser = new Cruiser((startPosition, isHorizontal));
            Ship destroyer = new Destroyer((startPosition, isHorizontal));
            Ship submarine = new Submarine((startPosition, isHorizontal));

            //Assert
            Assert.IsType<Battleship>(battleship);
            Assert.IsType<Carrier>(carrier);
            Assert.IsType<Cruiser>(cruiser);
            Assert.IsType<Destroyer>(destroyer);
            Assert.IsType<Submarine>(submarine);

            Assert.Equal(ShipType.Battleship, battleship.ShipType);
            Assert.Equal(ShipType.Carrier, carrier.ShipType);
            Assert.Equal(ShipType.Cruiser, cruiser.ShipType);
            Assert.Equal(ShipType.Destroyer, destroyer.ShipType);
            Assert.Equal(ShipType.Submarine, submarine.ShipType);

            Assert.True(battleship.Size == battleship.Positions.Length);
            Assert.True(carrier.Size == carrier.Positions.Length);
            Assert.True(cruiser.Size == cruiser.Positions.Length);
            Assert.True(destroyer.Size == destroyer.Positions.Length);
            Assert.True(submarine.Size == submarine.Positions.Length);

            Assert.False(battleship.IsSunk);
            Assert.False(carrier.IsSunk);
            Assert.False(cruiser.IsSunk);
            Assert.False(destroyer.IsSunk);
            Assert.False(submarine.IsSunk);

            Assert.True(battleship.IsUndamaged);
            Assert.True(carrier.IsUndamaged);
            Assert.True(cruiser.IsUndamaged);
            Assert.True(destroyer.IsUndamaged);
            Assert.True(submarine.IsUndamaged);
        }

        /// <summary>
        /// Creates instances of the different <see cref="Ship"/> subclasses. Checks that they correctly create 
        /// the Position arrays.
        /// </summary>
        /// <param name="startPosition">An integer representing the start position for the ship.</param>
        /// <param name="isHorizontal">A boolean value indicating whether the ship is aligned horizontally.</param>
        [Theory]
        [InlineData(3, true, new int[] { 3, 4 })]
        [InlineData(26, false, new int[] { 26, 36 })]
        [InlineData(90, true, new int[] { 90, 91 })]
        public void MainConstructor_ValidStartPosition_SuccessfullyCreatesPositionsArray(int startPosition, bool isHorizontal, int[] expectedPositions)
        {
            //Arrange & Act
            Ship battleship = new Battleship((startPosition, isHorizontal));
            Ship carrier = new Carrier((startPosition, isHorizontal));
            Ship cruiser = new Cruiser((startPosition, isHorizontal));
            Ship destroyer = new Destroyer((startPosition, isHorizontal));
            Ship submarine = new Submarine((startPosition, isHorizontal));

            //Assert
            Assert.Equal(expectedPositions, battleship.Positions.Take(2).ToArray());
            Assert.Equal(expectedPositions, carrier.Positions.Take(2).ToArray());
            Assert.Equal(expectedPositions, cruiser.Positions.Take(2).ToArray());
            Assert.Equal(expectedPositions, destroyer.Positions);
            Assert.Equal(expectedPositions, submarine.Positions.Take(2).ToArray());

            Assert.Equal(4, battleship.Positions.Length);
            Assert.Equal(5, carrier.Positions.Length);
            Assert.Equal(3, cruiser.Positions.Length);
            Assert.Equal(2, destroyer.Positions.Length);
            Assert.Equal(3, submarine.Positions.Length);
        }

        /// <summary>
        /// Creates instances of the different <see cref="Ship"/> subclasses. Checks that they correctly create 
        /// the Damage arrays. The array length should equal the ship size and all values should be false.
        /// <param name="startPosition">An integer representing the start position for the ship.</param>
        /// <param name="isHorizontal">A boolean value indicating whether the ship is aligned horizontally.</param>
        [Theory]
        [InlineData(3, true)]
        [InlineData(26, false)]
        [InlineData(90, true)]
        public void MainConstructor_ValidStartPosition_SuccessfullyCreatesDamageArray(int startPosition, bool isHorizontal)
        {
            //Arrange & Act
            Ship battleship = new Battleship((startPosition, isHorizontal));
            Ship carrier = new Carrier((startPosition, isHorizontal));
            Ship cruiser = new Cruiser((startPosition, isHorizontal));
            Ship destroyer = new Destroyer((startPosition, isHorizontal));
            Ship submarine = new Submarine((startPosition, isHorizontal));

            //Assert
            Assert.True(battleship.Damage.All(damage => damage == false));
            Assert.True(carrier.Damage.All(damage => damage == false));
            Assert.True(cruiser.Damage.All(damage => damage == false));
            Assert.True(destroyer.Damage.All(damage => damage == false));
            Assert.True(submarine.Damage.All(damage => damage == false));

            Assert.Equal(4, battleship.Damage.Length);
            Assert.Equal(5, carrier.Damage.Length);
            Assert.Equal(3, cruiser.Damage.Length);
            Assert.Equal(2, destroyer.Damage.Length);
            Assert.Equal(3, submarine.Damage.Length);
        }

        /// <summary>
        /// Checks that the main constructor throws a <see cref="NullReferenceException"/> if a null position 
        /// parameter is passed.
        /// </summary>
        [Fact]
        public void Constructor_NullInput_ThrowsNullReferenceException()
        {
            //Arrange & Act & Assert
            Assert.Throws<NullReferenceException>(() => new Battleship(null!));
        }

        /// <summary>
        /// Creates instances of each <see cref="Ship"/> type using the DTO constructor. Checks that the arrays 
        /// are correctly created and that the data matches the stored ship state.
        /// </summary>
        /// <remarks>Each ship type covers different cases, for example including sunk or undamaged ships.</remarks>
        [Fact]
        public void DTOConstructor_ValidDTOParameter_CreatesInstanceWithCorrectData()
        {
            //Arrange & Act
            var battleship = new Battleship(_battleshipDTO);
            var carrier = new Carrier(_carrierDTO);
            var cruiser = new Cruiser(_cruiserDTO);
            var destroyer = new Destroyer(_destroyerDTO);
            var submarine = new Submarine(_submarineDTO);

            //Assert
            Assert.IsType<Battleship>(battleship);
            Assert.IsType<Carrier>(carrier);
            Assert.IsType<Cruiser>(cruiser);
            Assert.IsType<Destroyer>(destroyer);
            Assert.IsType<Submarine>(submarine);

            Assert.Equal(4, battleship.Size);
            Assert.Equal(5, carrier.Size);
            Assert.Equal(3, cruiser.Size);
            Assert.Equal(2, destroyer.Size);
            Assert.Equal(3, submarine.Size);

            Assert.Equal(4, battleship.Positions.Length);
            Assert.Equal(5, carrier.Positions.Length);
            Assert.Equal(3, cruiser.Positions.Length);
            Assert.Equal(2, destroyer.Positions.Length);
            Assert.Equal(3, submarine.Positions.Length);

            Assert.Equal(4, battleship.Damage.Length);
            Assert.Equal(5, carrier.Damage.Length);
            Assert.Equal(3, cruiser.Damage.Length);
            Assert.Equal(2, destroyer.Damage.Length);
            Assert.Equal(3, submarine.Damage.Length);

            Assert.True(battleship.IsUndamaged);
            Assert.False(battleship.IsSunk);

            Assert.False(carrier.IsUndamaged);
            Assert.False(carrier.IsSunk);

            Assert.False(cruiser.IsUndamaged);
            Assert.True(cruiser.IsSunk);

            Assert.True(destroyer.IsUndamaged);
            Assert.False(destroyer.IsSunk);

            Assert.True(submarine.IsUndamaged);
            Assert.False(submarine.IsSunk);
        }
        #endregion //Constructor Tests

        #region Method Tests
        /// <summary>
        /// Passes an invalid position to the CheckForHit method. Should return false.
        /// </summary>
        [Fact]
        public void CheckForHit_InvalidPosition_ReturnsFalse()
        {
            //Arrange
            var ship = new Battleship(_battleshipDTO);

            //Act
            var result = ship.CheckForHit(InvalidPosition);

            //Assert
            Assert.False(result);
        }

        /// <summary>
        /// Checks the outcome of the CheckForHit method with a series of valid positions. Should return true if 
        /// they are a new hit. Returns false if they are missing.
        /// </summary>
        /// <param name="position">An inteer value representing the position to be tested.</param>
        /// <param name="expectedResult">A boolean value indicating the expected outcome.</param>
        [Theory]
        [InlineData(7, true)]
        [InlineData(17, true)]
        [InlineData(27, true)]
        [InlineData(37, true)]
        [InlineData(0, false)]
        [InlineData(47, false)]
        [InlineData(99, false)]
        [InlineData(9, false)]
        public void CheckForHit_ValidPositions_ReturnsTrueIfHit(int position, bool expectedResult)
        {
            //Arrange
            var ship = new Battleship(_battleshipDTO);

            //Act
            var result = ship.CheckForHit(position);

            //Assert
            Assert.Equal(expectedResult, result);

            if (expectedResult)
                Assert.False(ship.IsUndamaged);
            else
                Assert.True(ship.IsUndamaged);
        }

        /// <summary>
        /// Checks the different ships with a range of hits including all their unhit positions. At the end, the 
        /// ship should not be undamaged and should be sunk.
        /// </summary>
        /// <param name="ship">The <see cref="Ship"/> instance to be tested.</param>
        /// <param name="positions">An array of positions to be passed to the CheckForHit method.</param>
        [Theory]
        [MemberData(nameof(GetShipAndPositionsHitTest))]
        public void CheckForHit_MultipleHits_SinksShip(Ship ship, int[] positions)
        {
            //Arrange & Act
            foreach (var position in positions)
                ship.CheckForHit(position);

            //Assert
            Assert.False(ship.IsUndamaged);
            Assert.True(ship.IsSunk);
        }

        /// Checks the different ships with a range of misses avoiding all their ship positions. The ship should 
        /// have the same IsUndamaged and IsSunk values at the start and end of the test.
        /// </summary>
        /// <param name="ship">The <see cref="Ship"/> instance to be tested.</param>
        /// <param name="positions">An array of positions to be passed to the CheckForHit method.</param>
        [Theory]
        [MemberData(nameof(GetShipAndPositionsMissTest))]
        public void CheckForHit_MultipleMisses_IsUndamagedAndIsSunkUnchanged(Ship ship, int[] positions)
        {
            //Arrange
            var startUndamaged = ship.IsUndamaged;
            var startSunk = ship.IsSunk;

            //Act
            foreach (var position in positions)
                ship.CheckForHit(position);

            var endUndamaged = ship.IsUndamaged;
            var endSunk = ship.IsSunk;

            //Assert
            Assert.Equal(startUndamaged, endUndamaged);
            Assert.Equal(startSunk, endSunk);
        }

        /// <summary>
        /// Calls the GetDTO method on different ships. Checks that the data has been correctly stored.
        /// </summary>
        [Fact]
        public void GetDTO_ValidShipInstances_ReturnsDTOWithCorrectData()
        {
            //Arrange
            var battleship = new Battleship(_battleshipDTO);
            var carrier = new Carrier(_carrierDTO);
            var cruiser = new Cruiser(_cruiserDTO);
            var destroyer = new Destroyer(_destroyerDTO);
            var submarine = new Submarine(_submarineDTO);

            //Act
            var battleshipDTO = battleship.GetDTO();
            var carrierDTO = carrier.GetDTO();
            var cruiserDTO = cruiser.GetDTO();
            var destroyerDTO = destroyer.GetDTO();
            var submarineDTO = submarine.GetDTO();

            //Assert
            Assert.IsType<ShipDTO>(battleshipDTO);
            Assert.IsType<ShipDTO>(carrierDTO);
            Assert.IsType<ShipDTO>(cruiserDTO);
            Assert.IsType<ShipDTO>(destroyerDTO);
            Assert.IsType<ShipDTO>(submarineDTO);

            Assert.Equal(4, battleshipDTO.Size);
            Assert.Equal(5, carrierDTO.Size);
            Assert.Equal(3, cruiserDTO.Size);
            Assert.Equal(2, destroyerDTO.Size);
            Assert.Equal(3, submarineDTO.Size);

            Assert.Equal([false, false, false, false], battleshipDTO.Damage);
            Assert.Equal([true, true, false, false, false], carrierDTO.Damage);
            Assert.Equal([true, true, true], cruiserDTO.Damage);
            Assert.Equal([false, false], destroyerDTO.Damage);
            Assert.Equal([false, false, false], submarineDTO.Damage);
        }
        #endregion //Method Tests

        #region MemberData
        public static IEnumerable<object[]> GetShipAndPositionsHitTest()
        {
            yield return new object[] { new Battleship(_battleshipDTO), new int[] { 30, 99, 27, 17, 14, 37, 7 } };
            yield return new object[] { new Carrier(_carrierDTO), new int[] { 99, 0, 17, 12, 13, 1, 14 } };
            yield return new object[] { new Cruiser(_cruiserDTO), Array.Empty<int>() };
            yield return new object[] { new Destroyer(_destroyerDTO), new int[] { 0, 17, 15, 99, 42, 5 } };
            yield return new object[] { new Submarine(_submarineDTO), new int[] { 99, 98, 97, 96, 95, 94, 93, 92, 91, 90 } };
        }

        public static IEnumerable<object[]> GetShipAndPositionsMissTest()
        {
            yield return new object[] { new Battleship(_battleshipDTO), new int[] { 30, 99, 28, 19, 14, 31, 11 } };
            yield return new object[] { new Carrier(_carrierDTO), new int[] { 99, 0, 17, 18, 19, 1, 4 } };
            yield return new object[] { new Cruiser(_cruiserDTO), new int[] { 0, 1, 20, 35, 92 } };
            yield return new object[] { new Destroyer(_destroyerDTO), new int[] { 0, 17, 18, 99, 42, 6 } };
            yield return new object[] { new Submarine(_submarineDTO), new int[] { 99, 98, 97, 96, 95, 94 } };
        }
        #endregion //MemberData
    }
}
