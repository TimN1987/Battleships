using Battleships.MVVM.Enums;
using Battleships.MVVM.Factories;
using Battleships.MVVM.Model;
using Battleships.MVVM.Services;
using Battleships.MVVM.Services.Database;
using Battleships.MVVM.Structs;
using Battleships.MVVM.View;
using Moq;

namespace BattleshipsTests.Tests.Services
{
    public class SaveServiceTests
    {
        #region Fields
        private readonly Mock<IEventAggregator> _eventAggregator;
        private readonly Mock<ILoggerFactory> _loggerFactory;
        private readonly Mock<IEventLogger> _eventLogger;
        private readonly Mock<IGameRepository> _gameRepository;
        private readonly Mock<IEncryptionService> _encryptionService;
        private readonly GameSetUpInformation _gameSetupInformation;
        #endregion //Fields

        public SaveServiceTests()
        {
            _eventAggregator = new Mock<IEventAggregator>();
            _loggerFactory = new Mock<ILoggerFactory>();
            _eventLogger = new Mock<IEventLogger>();
            _gameRepository = new Mock<IGameRepository>();
            _encryptionService = new Mock<IEncryptionService>();

            _loggerFactory
                .Setup(x => x.CreateLogger(It.IsAny<string>()))
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
                    { ShipType.Battleship, (0, true) },
                    { ShipType.Carrier, (20, true) },
                    { ShipType.Cruiser, (40, true) },
                    { ShipType.Destroyer, (60, true) },
                    { ShipType.Submarine, (80, true) }
                }
            };
        }

        #region Constructor Tests
        /// <summary>
        /// Tests the constructor of the SaveService class a null EventAggregator parameter. Checks that an 
        /// ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void Constructor_NullEventAggregator_ThrowsArgumentNullException()
        {
            //Arrange & Act
            var saveService = Assert.Throws<ArgumentNullException>(() =>
                new SaveService(null!, _loggerFactory.Object, _gameRepository.Object, _encryptionService.Object));

            //Assert
            Assert.Contains("cannot be null", saveService.Message);
        }

        /// <summary>
        /// Tests the constructor of the SaveService class a null LoggerFactory parameter. Checks that a 
        /// NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void Constructor_NullLoggerFactory_ThrowsNullReferenceException()
        {
            //Arrange, Act & Assert
            var saveService = Assert.Throws<NullReferenceException>(() =>
                new SaveService(_eventAggregator.Object, null!, _gameRepository.Object, _encryptionService.Object));
        }

        /// <summary>
        /// Tests the constructor of the SaveService class a null GameRepository parameter. Checks that an 
        /// ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void Constructor_NullGameRepository_ThrowsArgumentNullException()
        {
            //Arrange & Act
            var saveService = Assert.Throws<ArgumentNullException>(() =>
                new SaveService(_eventAggregator.Object, _loggerFactory.Object, null!, _encryptionService.Object));

            //Assert
            Assert.Contains("cannot be null", saveService.Message);
        }

        /// <summary>
        /// Tests the constructor of the SaveService class a null EncryptionService parameter. Checks that an 
        /// ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void Constructor_NullEncryptionService_ThrowsArgumentNullException()
        {
            //Arrange & Act
            var saveService = Assert.Throws<ArgumentNullException>(() =>
                new SaveService(_eventAggregator.Object, _loggerFactory.Object, _gameRepository.Object, null!));

            //Assert
            Assert.Contains("cannot be null", saveService.Message);
        }

        /// <summary>
        /// Tests the constructor of the SaveService class with all parameters valid. Checks that an instance 
        /// is created.
        /// </summary>
        [Fact]
        public void Constructor_AllParametersValid_CreatesInstance()
        {
            //Arrange & Acts
            var saveService = new SaveService(_eventAggregator.Object, _loggerFactory.Object, _gameRepository.Object, _encryptionService.Object);
            //Assert
            Assert.NotNull(saveService);
        }
        #endregion //Constructor Tests

        #region SaveGame Method Tests
        /// <summary>
        /// Tests the SaveGame method of the SaveService class when save is called for a save game without 
        /// a valid <see cref="CurrentGameName"/>. Checks that the correct EventAggregator messages are sent.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SaveGame_CurrentGameNameNull_PublishesNavigationEventAndTwoSaveStatusEvents()
        {
            //Arrange
            var saveService = new SaveService(_eventAggregator.Object, _loggerFactory.Object, _gameRepository.Object, _encryptionService.Object);
            saveService.CurrentGameName = null;

            var saveStatusEventMock = new Mock<SaveStatusEvent>();
            var navigationEventMock = new Mock<NavigationEvent>();

            _eventAggregator
                .Setup(x => x.GetEvent<SaveStatusEvent>())
                .Returns(saveStatusEventMock.Object);

            _eventAggregator
                .Setup(x => x.GetEvent<NavigationEvent>())
                .Returns(navigationEventMock.Object);

            //Act
            await saveService.SaveGame(false);

            //Assert
            _eventAggregator.Verify(x => x.GetEvent<NavigationEvent>().Publish(typeof(SaveGameView)), Times.Once);
            _eventAggregator.Verify(x => x.GetEvent<SaveStatusEvent>().Publish(It.IsAny<string>()), Times.Exactly(2));
        }

        /// <summary>
        /// Tests the SaveGame method of the SaveService class when save is called for a save game with 
        /// <cref see="CurrentGameName"/> set to null. Checks that the correct EventAggregator messages are 
        /// sent and the warning message is logged.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task SaveGame_CurrentGameNameNotNullCurrentGameNull_PublishesFourSaveStatusMessages(bool autosaveGame)
        {
            //Arrange
            var saveService = new SaveService(_eventAggregator.Object, _loggerFactory.Object, _gameRepository.Object, _encryptionService.Object);
            saveService.CurrentGameName = "TestGame";
            saveService.CurrentGame = null;

            var saveStatusEventMock = new Mock<SaveStatusEvent>();

            _eventAggregator
                .Setup(x => x.GetEvent<SaveStatusEvent>())
                .Returns(saveStatusEventMock.Object);

            //Act
            await saveService.SaveGame(autosaveGame);

            //Assert
            _eventAggregator.Verify(x => x.GetEvent<SaveStatusEvent>().Publish(It.IsAny<string>()), Times.Exactly(4));
            _eventAggregator.Verify(x => x.GetEvent<SaveStatusEvent>().Publish("Saving game..."), Times.Once);
            _eventAggregator.Verify(x => x.GetEvent<SaveStatusEvent>().Publish("Save failed"), Times.Once);
            _eventLogger.Verify(logger =>
                logger.LogWarning(It.Is<string>(msg => msg.Contains("No game data exists to save.")), null));
        }

        /// <summary>
        /// Tests the SaveGame method of the SaveService class when save is called for an autosave game. Mocks 
        /// the GameRepository to return true (this is tested separately). Tests with AutosaveGameExists set to 
        /// true and false. Both tests should return true and publish the correct events.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task SaveGame_SaveRecordStoresAutosaveCorrectly_ReturnsTruePublishesSaveStatusEvent(bool autosaveExists)
        {
            //Arrange
            var saveService = new SaveService(_eventAggregator.Object, _loggerFactory.Object, _gameRepository.Object, _encryptionService.Object);
            saveService.CurrentGame = new ClassicGame(_loggerFactory.Object, _gameSetupInformation);
            saveService.AutosaveGameExists = autosaveExists;

            var saveStatusEventMock = new Mock<SaveStatusEvent>();

            _eventAggregator
                .Setup(x => x.GetEvent<SaveStatusEvent>())
                .Returns(saveStatusEventMock.Object);

            _encryptionService
                .Setup(x => x.EncryptGameData(It.IsAny<Game>()))
                .ReturnsAsync("EncryptedData");

            _gameRepository
                .Setup(x => x.CreateSaveGameRecord(It.IsAny<string>(), null, null))
                .ReturnsAsync(true);

            _gameRepository
                .Setup(x => x.UpdateSaveGameRecord(It.IsAny<string>(), null, null))
                .ReturnsAsync(true);

            //Act
            var result = await saveService.SaveGame();

            //Assert
            Assert.True(result);
            _eventAggregator.Verify(x => x.GetEvent<SaveStatusEvent>().Publish(It.IsAny<string>()), Times.Exactly(4));
            _eventAggregator.Verify(x => x.GetEvent<SaveStatusEvent>().Publish("Saving game..."), Times.Once);
            _eventAggregator.Verify(x => x.GetEvent<SaveStatusEvent>().Publish("Game saved"), Times.Once);
        }

        /// <summary>
        /// Tests the SaveGame method of the SaveService class when save is called for a named game. Mocks the 
        /// GameRepository to return true (this is tested separately).
        /// </summary>
        [Fact]
        public async Task SaveGame_SaveRecordStoresNamedGameCorrectly_ReturnsTruePublishesSaveStatusEvent()
        {
            //Arrange
            var saveService = new SaveService(_eventAggregator.Object, _loggerFactory.Object, _gameRepository.Object, _encryptionService.Object);
            saveService.CurrentGame = new ClassicGame(_loggerFactory.Object, _gameSetupInformation);
            saveService.CurrentGameName = "TestGame";

            var saveStatusEventMock = new Mock<SaveStatusEvent>();

            _eventAggregator
                .Setup(x => x.GetEvent<SaveStatusEvent>())
                .Returns(saveStatusEventMock.Object);

            _encryptionService
                .Setup(x => x.EncryptGameData(It.IsAny<Game>()))
                .ReturnsAsync("EncryptedData");

            _gameRepository
                .Setup(x => x.UpdateSaveGameRecord(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SaveGameTable>()))
                .ReturnsAsync(true);

            //Act
            var result = await saveService.SaveGame(false);

            //Assert
            Assert.True(result);
            _eventAggregator.Verify(x => x.GetEvent<SaveStatusEvent>().Publish(It.IsAny<string>()), Times.Exactly(4));
            _eventAggregator.Verify(x => x.GetEvent<SaveStatusEvent>().Publish("Saving game..."), Times.Once);
            _eventAggregator.Verify(x => x.GetEvent<SaveStatusEvent>().Publish("Game saved"), Times.Once);
        }

        /// <summary>
        /// Tests the SaveGame method of the SaveService class when save is called for an autosave game. Mocks 
        /// GameRepository to return false (this is tested separately). Tests with AutosaveGameExists set to 
        /// true and false. Returns false and publishes the correct events.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task SaveGame_SaveRecordFailsForAutosaveGame_ReturnsFalsePublishesSaveStatusEvent(bool autosaveExists)
        {
            //Arrange
            var saveService = new SaveService(_eventAggregator.Object, _loggerFactory.Object, _gameRepository.Object, _encryptionService.Object);
            saveService.CurrentGame = new ClassicGame(_loggerFactory.Object, _gameSetupInformation);
            saveService.AutosaveGameExists = autosaveExists;

            var saveStatusEventMock = new Mock<SaveStatusEvent>();

            _eventAggregator
                .Setup(x => x.GetEvent<SaveStatusEvent>())
                .Returns(saveStatusEventMock.Object);

            _encryptionService
                .Setup(x => x.EncryptGameData(It.IsAny<Game>()))
                .ReturnsAsync("EncryptedData");

            _gameRepository
                .Setup(x => x.CreateSaveGameRecord(It.IsAny<string>(), null, null))
                .ReturnsAsync(false);

            _gameRepository
                .Setup(x => x.UpdateSaveGameRecord(It.IsAny<string>(), null, null))
                .ReturnsAsync(false);

            //Act
            var result = await saveService.SaveGame();

            //Assert
            Assert.False(result);
            _eventAggregator.Verify(x => x.GetEvent<SaveStatusEvent>().Publish(It.IsAny<string>()), Times.Exactly(4));
            _eventAggregator.Verify(x => x.GetEvent<SaveStatusEvent>().Publish("Saving game..."), Times.Once);
            _eventAggregator.Verify(x => x.GetEvent<SaveStatusEvent>().Publish("Save failed"), Times.Once);
            _eventLogger.Verify(logger =>
                logger.LogWarning(It.Is<string>(msg => msg.Contains("Game could not be autosaved.")), null));
        }

        /// <summary>
        /// Tests the SaveGame method of the SaveService class when save is called for a named game. Mocks the 
        /// GameRepository to return false (this is tested separately). Returns false and publishes the 
        /// correct SaveStatus events.
        /// </summary>
        [Fact]
        public async Task SaveGame_SaveRecordFailsForNamedGame_ReturnsFalsePublishesSaveStatusEvent()
        {
            //Arrange
            var saveService = new SaveService(_eventAggregator.Object, _loggerFactory.Object, _gameRepository.Object, _encryptionService.Object);
            saveService.CurrentGame = new ClassicGame(_loggerFactory.Object, _gameSetupInformation);
            saveService.CurrentGameName = "TestGame";

            var saveStatusEventMock = new Mock<SaveStatusEvent>();

            _eventAggregator
                .Setup(x => x.GetEvent<SaveStatusEvent>())
                .Returns(saveStatusEventMock.Object);

            _encryptionService
                .Setup(x => x.EncryptGameData(It.IsAny<Game>()))
                .ReturnsAsync("EncryptedData");

            _gameRepository
                .Setup(x => x.CreateSaveGameRecord(It.IsAny<string>(), It.IsAny<string>(), SaveGameTable.SaveGames))
                .ReturnsAsync(false);

            _gameRepository
                .Setup(x => x.UpdateSaveGameRecord(It.IsAny<string>(), It.IsAny<string>(), SaveGameTable.SaveGames))
                .ReturnsAsync(false);

            //Act
            var result = await saveService.SaveGame(false);

            //Assert
            Assert.False(result);
            _eventAggregator.Verify(x => x.GetEvent<SaveStatusEvent>().Publish(It.IsAny<string>()), Times.Exactly(4));
            _eventAggregator.Verify(x => x.GetEvent<SaveStatusEvent>().Publish("Saving game..."), Times.Once);
            _eventAggregator.Verify(x => x.GetEvent<SaveStatusEvent>().Publish("Save failed"), Times.Once);
            _eventLogger.Verify(logger =>
                logger.LogWarning(It.Is<string>(msg => msg.Contains("Game could not be saved.")), null));
        }

        /// <summary>
        /// Tests the SaveGame method of the SaveService class when the Game encryption fails and returns null. 
        /// An InvalidOperationException is thrown and caught. It returns false and logs a critical event. 
        /// SaveStatusEvent messages are published to update the user.
        /// </summary>
        [Fact]
        public async Task SaveGame_EncryptDataFailsReturnsNull_ReturnsFalsePublishesSaveStatusEventLogsCritical()
        {
            //Arrange
            var saveService = new SaveService(_eventAggregator.Object, _loggerFactory.Object, _gameRepository.Object, _encryptionService.Object);
            saveService.CurrentGame = new ClassicGame(_loggerFactory.Object, _gameSetupInformation);
            saveService.CurrentGameName = "TestGame";

            var saveStatusEventMock = new Mock<SaveStatusEvent>();

            _eventAggregator
                .Setup(x => x.GetEvent<SaveStatusEvent>())
                .Returns(saveStatusEventMock.Object);

            _encryptionService
                .Setup(x => x.EncryptGameData(It.IsAny<Game>()))
                .ReturnsAsync(() => null);

            //Act
            var result = await saveService.SaveGame();

            //Assert
            Assert.False(result);
            _eventAggregator.Verify(x => x.GetEvent<SaveStatusEvent>().Publish(It.IsAny<string>()), Times.Exactly(4));
            _eventAggregator.Verify(x => x.GetEvent<SaveStatusEvent>().Publish("Saving game..."), Times.Once);
            _eventAggregator.Verify(x => x.GetEvent<SaveStatusEvent>().Publish("Save failed"), Times.Once);
            _eventLogger.Verify(logger =>
                logger.LogCritical(It.Is<string>(msg => msg.Contains("Game data could not be encrypted")), It.IsAny<InvalidOperationException>(), null, false));
        }
        #endregion //SaveGame Method Tests
    }
}
