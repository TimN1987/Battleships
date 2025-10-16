using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battleships.MVVM.Enums;
using Battleships.MVVM.Factories;
using Battleships.MVVM.Model;
using Battleships.MVVM.Model.DataTransferObjects;
using Battleships.MVVM.Services;
using Battleships.MVVM.Structs;
using Moq;

namespace BattleshipsTests.Tests.Services
{
    public class EncryptionServiceTests
    {
        #region Fields
        private readonly Mock<IEventLogger> _eventLogger;
        private readonly Mock<ILoggerFactory> _loggerFactory;
        private readonly GameSetUpInformation _gameSetupInformation;
        #endregion //Fields

        public EncryptionServiceTests()
        {
            _loggerFactory = new Mock<ILoggerFactory>();
            _eventLogger = new Mock<IEventLogger>();

            _loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(_eventLogger.Object);

            _gameSetupInformation = new GameSetUpInformation
            {
                Type = GameType.Classic,
                Difficulty = GameDifficulty.Easy,
                PlayerStarts = true,
                ShipsCanTouch = true,
                AirstrikeAllowed = true,
                BombardmentAllowed = true,
                FireUntilMiss = true,
                BonusShotIfSunk = true,
                HideSunkShips = true,
                SalvoShotType = SalvoShots.EqualsLargestUndamagedShip,
                ShipPositions = new Dictionary<ShipType, (int, bool)>
                {
                    {ShipType.Battleship, (0, true) },
                    {ShipType.Carrier, (20, true) },
                    {ShipType.Cruiser, (40, true) },
                    {ShipType.Destroyer, (60, true) },
                    {ShipType.Submarine, (80, true) }
                }

            };
        }

        #region Constructor Tests
        /// <summary>
        /// Tests the constructor of the EncryptionService class with a null ILoggerFactory parameter. Throws a 
        /// NullReferenceException when the logger factory CreateLogger method is called.
        /// </summary>
        [Fact]
        public void Constructor_NullLoggerFactoryParameter_ThrowsNullReferenceException()
        {
            // Arrange
            ILoggerFactory loggerFactory = null!;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => new EncryptionService(loggerFactory));
        }

        /// <summary>
        /// Tests the constructor of the EncryptionService class with a valid ILoggerFactory parameter. 
        /// Checks that it creates a non-null instance.
        /// </summary>
        [Fact]
        public void Constructor_ValidLoggerFactoryParameter_InstanceCreatedSuccessfully()
        {
            //Arrange & Act
            var encryptionService = new EncryptionService(_loggerFactory.Object);

            //Assert
            Assert.NotNull(encryptionService);
        }
        #endregion //Constructor Tests

        #region Encryption Tests
        /// <summary>
        /// Tests the EncryptGameData method of the EncryptionService class with a valid ClassicGame parameter.
        /// Carries out a series of checks to ensure that the encrypted string is not null and could contain 
        /// a valid initialization vector at the start. Also, decrypts the encrypted string and checks that 
        /// the original game data is returned.
        /// </summary>
        [Fact]
        public async Task EncryptGameData_ValidGameParameter_ReturnsDecryptableStringWithCorrectByteArrayLength()
        {
            //Arrange
            var encryptionService = new EncryptionService(_loggerFactory.Object);
            var validGame = new ClassicGame(_loggerFactory.Object, _gameSetupInformation);

            //Act
            var encryptedString = await encryptionService.EncryptGameData(validGame);

            var rawBytes = Convert.FromBase64String(encryptedString!);

            var decryptedGame = await encryptionService.DecryptGameData(encryptedString!);

            //Assert
            Assert.NotNull(encryptedString);
            Assert.True(rawBytes.Length > 16);
            Assert.False(rawBytes.Take(16).All(bytes => bytes == 0));
            Assert.NotNull(decryptedGame);
            Assert.IsType<GameDTO>(decryptedGame);
        }

        [Fact]
        public async Task EncryptGameData_NullGameParameter_LogCriticalReturnNull()
        {
            //Arrange
            var encryptionService = new EncryptionService(_loggerFactory.Object);

            //Act
            var encryptedString = await encryptionService.EncryptGameData(null!);

            //Assert
            Assert.Null(encryptedString);
            _eventLogger.Verify(logger =>
                logger.LogCritical(It.Is<string>(msg => msg.Contains("Game data input is null")), It.IsAny<ArgumentException>(), null, false));
        }
        #endregion //Encryption Tests

        #region Decryption Tests
        /// <summary>
        /// Tests the DecryptGameData method of the EncryptionService class with a valid encrypted string. 
        /// Uses the EncryptGameData method to create a valid encrypted string and then decrypts it to 
        /// ensure that a non-null ClassicGame object is returned that matches the original game data.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task DecryptGameData_ValidEncryptedString_ReturnsDecryptedGame()
        {
            //Arrange
            var encryptionService = new EncryptionService(_loggerFactory.Object);
            var validGame = new ClassicGame(_loggerFactory.Object, _gameSetupInformation);
            var encryptedString = await encryptionService.EncryptGameData(validGame);

            //Act
            var decryptedGame = await encryptionService.DecryptGameData(encryptedString!);
            
            //Assert
            Assert.NotNull(decryptedGame);
            Assert.IsType<GameDTO>(decryptedGame);
        }

        /// <summary>
        /// Tests the DecryptGameData method of the EncryptionService class with a null encrypted string. 
        /// Checks that the returned Game object is null and that exception logging is performed.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task DecryptGameData_NullEncryptedString_LogCriticalReturnNull()
        {
            //Arrange
            var encryptionService = new EncryptionService(_loggerFactory.Object);

            //Act
            var decryptedGame = await encryptionService.DecryptGameData(null!);

            //Assert
            Assert.Null(decryptedGame);
            _eventLogger.Verify(logger =>
                logger.LogCritical(It.Is<string>(msg => msg.Contains("Game data input is null")), It.IsAny<ArgumentException>(), null, false));
        }

        /// <summary>
        /// Tests the DecryptGameData method of the EncryptionService class with an invalid encrypted string. 
        /// Checks that the returned Game object is null and that exception logging is performed.
        /// </summary>
        [Fact]
        public async Task DecryptGameData_InvalidEncryptedString_LogCriticalReturnNull()
        {
            //Arrange
            var encryptionService = new EncryptionService(_loggerFactory.Object);
            var invalidEncryptedString = "InvalidEncryptedString";

            //Act
            var decryptedGame = await encryptionService.DecryptGameData(invalidEncryptedString);

            //Assert
            Assert.Null(decryptedGame);
            _eventLogger.Verify(logger =>
                logger.LogCritical(It.Is<string>(msg => msg.Contains("Could not successfully decrypt")), It.IsAny<InvalidOperationException>(), null, false));
        }
        #endregion //Decryption Tests

        #region Json Tests
        /// <summary>
        /// Tests the ConvertGameToJson method of the EncryptionService class with a valid ClassicGame parameter. 
        /// Checks that the returned JSON string is not null and has a length greater than 0.
        /// </summary>
        [Fact]
        public void ConvertGameToJson_ValidGameInput_ReturnsValidString()
        {
            //Arrange
            var encryptionService = new EncryptionService(_loggerFactory.Object);
            var validGame = new ClassicGame(_loggerFactory.Object, _gameSetupInformation);

            //Act
            var jsonString = encryptionService.ConvertGameToJson(validGame);

            //Assert
            Assert.NotNull(jsonString);
            Assert.True(jsonString.Length > 0);
        }

        /// <summary>
        /// Tests the DeserializeGameData method of the EncryptionService class with a valid JSON string. Checks 
        /// that the created Json is valid and that it converts back correctly.
        /// </summary>
        [Fact]
        public void DeserializeGameData_ValidJsonString_ReturnsGameObject()
        {
            //Arrange
            var encryptionService = new EncryptionService(_loggerFactory.Object);
            var validGame = new ClassicGame(_loggerFactory.Object, _gameSetupInformation);
            var jsonString = encryptionService.ConvertGameToJson(validGame);

            //Act
            var gameObject = encryptionService.DeserializeGameData(jsonString!);

            //Assert
            Assert.NotNull(jsonString);
            Assert.True(jsonString.Length > 0);
            Assert.Contains("Grid", jsonString);
            Assert.Contains("Ships", jsonString);

            Assert.NotNull(gameObject);
            Assert.IsType<GameDTO>(gameObject);
        }
        #endregion //Json Tests

        #region AES Tests
        /// <summary>
        /// Tests the RetrieveKey method of the EncryptionService class. Checks that the returned key is 
        /// non-null and the correct length.
        /// </summary>
        [Fact]
        public async Task RetrieveKey_MethodCalled_ReturnsNonNullKey()
        {
            // Arrange
            var encryptionService = new EncryptionService(_loggerFactory.Object);

            // Act
            var key = await encryptionService.RetrieveKey();

            // Assert
            Assert.NotNull(key);
            Assert.True(key.Length == 32);
        }

        /// <summary>
        /// Tests the EncryptJsonData method of the EncryptionService class with a valid JSON string. Checks 
        /// that the returned encrypted string is not null and has a length greater than 0.
        /// </summary>
        [Fact]
        public async Task EncryptJsonData_ValidJsonString_ReturnsEncryptedString()
        {
            // Arrange
            var encryptionService = new EncryptionService(_loggerFactory.Object);
            var validGame = new ClassicGame(_loggerFactory.Object, _gameSetupInformation);
            var validJsonString = encryptionService.ConvertGameToJson(validGame);

            // Act
            var encryptedString = await encryptionService.EncryptJsonData(validJsonString!);
            var rawBytes = Convert.FromBase64String(encryptedString!);

            // Assert
            Assert.NotNull(encryptedString);
            Assert.True(encryptedString.Length > 0);
            Assert.True(rawBytes.Length > 16);
            Assert.False(rawBytes.Take(16).All(bytes => bytes == 0));
        }

        /// <summary>
        /// Tests the EncryptJsonData method of the EncryptionService class with a non-null string. 
        /// Checks that the string is unchanged after encryption and decryption.
        /// </summary>
        [Fact]
        public async Task EncryptJsonData_NonNullString_DecryptsCorrectly()
        {
            //Arrange\
            var encryptionService = new EncryptionService(_loggerFactory.Object);
            var inputString = "This is a string.";

            //Act
            var encryptedString = await encryptionService.EncryptJsonData(inputString);
            var decryptedString = await encryptionService.DecryptJsonData(encryptedString!);

            // Assert
            Assert.NotNull(encryptedString);
            Assert.True(encryptedString.Length > 0);

            Assert.NotNull(decryptedString);
            Assert.True(decryptedString?.Length > 0);
            Assert.Equal(inputString, decryptedString);
        }

        /// <summary>
        /// Tests the EncryptJsonData method of the EncryptionService class with a null JSON string. Checks 
        /// that the returned encrypted string is not null and that it contains the IV.
        /// </summary>
        [Fact]
        public async Task EncryptJsonData_TestJsonString_ReturnsEncryptedString()
        {
            //Arrange
            var encryptionService = new EncryptionService(_loggerFactory.Object);
            var inputString = "{\"name\": \"Test Game\", \"score\": 100}";


            //Act
            var encryptedString = await encryptionService.EncryptJsonData(inputString);
            var rawBytes = Convert.FromBase64String(encryptedString!);

            // Assert
            Assert.NotNull(encryptedString);
            Assert.True(encryptedString.Length > 0);
            Assert.True(rawBytes.Length > 16);
            Assert.False(rawBytes.Take(16).All(bytes => bytes == 0));
        }

        /// <summary>
        /// Tests the EncryptJsonData method of the EncryptionService class with a valid JSON string. Checks 
        /// that it can be decrypted correctly and that the original JSON string is returned.
        /// </summary>
        [Fact]
        public async Task EncryptJsonData_TestJsonString_DecryptsCorrectly()
        {
            //Arrange\
            var encryptionService = new EncryptionService(_loggerFactory.Object);
            var inputString = "{\"name\": \"Test Game\", \"score\": 100}";


            //Act
            var encryptedString = await encryptionService.EncryptJsonData(inputString);
            var rawBytes = Convert.FromBase64String(encryptedString!);

            var decryptedString = await encryptionService.DecryptJsonData(encryptedString!);

            // Assert
            Assert.NotNull(encryptedString);
            Assert.NotNull(rawBytes);
            Assert.True(encryptedString.Length > 0);
            Assert.True(rawBytes.Length > 16);
            Assert.False(rawBytes.Take(16).All(bytes => bytes == 0));

            Assert.NotNull(decryptedString);
            Assert.True(decryptedString?.Length > 0);
            Assert.Contains("Test Game", decryptedString);
            Assert.Equal(inputString, decryptedString);
        }

        /// <summary>
        /// Tests the DecryptJsonData method of the EncryptionService class with a valid encrypted string. 
        /// Checks that the returned JSON string is not null and is of type string.
        /// </summary>
        [Fact]
        public async Task DecryptJsonData_ValidEncryptedString_ReturnsDecryptedJsonString()
        {
            // Arrange
            var encryptionService = new EncryptionService(_loggerFactory.Object);
            var validGame = new ClassicGame(_loggerFactory.Object, _gameSetupInformation);
            var encryptedString = await encryptionService.EncryptGameData(validGame);

            // Act
            var decryptedJsonString = await encryptionService.DecryptJsonData(encryptedString!);

            // Assert
            Assert.NotNull(decryptedJsonString);
        }
        #endregion //AES Tests

        
    }
}
