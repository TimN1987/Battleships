using System.Data.SQLite;
using Battleships.MVVM.Enums;
using Battleships.MVVM.Factories;
using Battleships.MVVM.Services;
using Battleships.MVVM.Services.Database;
using Moq;

namespace BattleshipsTests.Tests.Database
{
    public class GameRepositoryTests
    {
        #region Fields
        private const string Autosave = "Autosave";
        private const string TestName = "TestGame";
        private const string TestData = "TestData";
        private const string WhiteSpace = "   ";
        private const string UpdateData = "UpdateData";
        private const string InvalidName = "InvalidName";

        private readonly Mock<ILoggerFactory> _loggerFactory;
        private readonly Mock<IEventLogger> _eventLogger;
        private readonly DatabaseInitializer _databaseInitializer;
        private readonly GameRepository _gameRepository;

        private readonly string _testDirectoryPath;
        private readonly string _testDatabasePath;
        private readonly string _connectionString;
        #endregion //Fields

        public GameRepositoryTests()
        {
            _loggerFactory = new Mock<ILoggerFactory>();
            _eventLogger = new Mock<IEventLogger>();

            _loggerFactory
                .Setup(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(_eventLogger.Object);

            _testDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BattleshipsTests");
            _testDatabasePath = Path.Combine(_testDirectoryPath, "savegames.db");

            if (!Directory.Exists(_testDirectoryPath))
                Directory.CreateDirectory(_testDirectoryPath);

            _databaseInitializer = new DatabaseInitializer(_loggerFactory.Object, _testDatabasePath);
            Task.Run(async () =>
                await _databaseInitializer.InitializeDatabaseWithRetries());

            if (!File.Exists(_testDatabasePath))
                throw new InvalidOperationException("Database does not exist.");

            _connectionString = $"Data Source={_testDatabasePath}";

            _gameRepository = new GameRepository(_loggerFactory.Object, _connectionString);
        }

        #region Constructor Tests
        /// <summary>
        /// Passes a null <see cref="ILoggerFactory"/> instance to the constructor. An <see cref="ArgumentNullException"/> 
        /// should be thrown.
        /// </summary>
        [Fact]
        public void Constructor_NullLoggerFactory_ThrowsArgumentNullException()
        {
            //Arrange & Act
            var exception = Assert.Throws<NullReferenceException>(() =>
                new GameRepository(null!, _connectionString));

            //Assert
            Assert.Contains("Object reference not set to an instance of an object", exception.Message);
        }

        /// <summary>
        /// Passes a null connection string to the constructor. An <see cref="ArgumentNullException"/> should be thrown.
        /// </summary>
        [Fact]
        public void Constructor_NullConnectionString_ThrowsArgumentNullException()
        {
            //Arrange & Act
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new GameRepository(_loggerFactory.Object, null!));

            //Assert
            Assert.Contains("Connection string cannot be null", exception.Message);
        }

        /// <summary>
        /// Passes valid parameters to the constructor. A non-null object of type <see cref="GameRepository"/> should 
        /// be instantiated.
        /// </summary>
        [Fact]
        public void Constructor_ValidParameters_InstanceCreatedSuccessfully()
        {
            //Arrange & Act
            var gameRepository = new GameRepository(_loggerFactory.Object, _connectionString);

            //Assert
            Assert.NotNull(gameRepository);
            Assert.IsType<GameRepository>(gameRepository);
        }

        /// <summary>
        /// Passes an invalid connection string to the constructor. A non-null object of type <see cref="GameRepository"/> 
        /// should be instantiated.
        /// </summary>
        [Fact]
        public void Constructor_InvalidConnectionString_InstanceCreatedSuccessfully()
        {
            //Arrange & Act
            var gameRepository = new GameRepository(_loggerFactory.Object, "Invalid string");

            //Assert
            Assert.NotNull(gameRepository);
            Assert.IsType<GameRepository>(gameRepository);
        }
        #endregion //Constructor Tests

        #region Autosave Tests
        /// <summary>
        /// Ensures that the test database contains an autosave file and uses the CheckForAutosaveFile to see 
        /// if it exists. Return value of true expected.
        /// </summary>
        [Fact]
        public async Task CheckForAutosaveFile_AutosaveFileExists_ReturnsTrue()
        {
            //Arrange
            ClearTable(SaveGameTable.AutosaveGame);

            var commandString = @"INSERT OR REPLACE INTO AutosaveGame (Name, SaveTime, GameData) VALUES (@gameName, @saveTime, @gameData);";
            var gameName = "Autosave";
            var saveTime = DateTime.Now;
            var gameData = "Test data";

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            using var command = new SQLiteCommand(commandString, connection);
            command.Parameters.AddWithValue("@gameName", gameName);
            command.Parameters.AddWithValue("@saveTime", saveTime);
            command.Parameters.AddWithValue("@gameData", gameData);

            command.ExecuteNonQuery();
            connection.Close();

            //Act
            var isAutosave = await _gameRepository.CheckForAutosaveFile();

            //Assert
            Assert.True(isAutosave);
            Assert.True(CountTableEntries(SaveGameTable.AutosaveGame) == 1);
        }

        /// <summary>
        /// Ensures that the test database contains no autosave files and uses the CheckForAutosaveFile to see 
        /// if an autosave exists. Return value of false expected.
        /// </summary>
        [Fact]
        public async Task CheckForAutosaveFile_AutosaveFileDoesNotExist_ReturnsFalse()
        {
            //Arrange
            ClearTable(SaveGameTable.AutosaveGame);

            //Act
            var isAutosave = await _gameRepository.CheckForAutosaveFile();

            //Assert
            Assert.False(isAutosave);
            Assert.True(CountTableEntries(SaveGameTable.AutosaveGame) == 0);
        }
        #endregion //Autosave Tests

        #region Create Tests
        /// <summary>
        /// Attempts to create a record containing null data in the AutosaveGame table. Should throw an 
        /// <see cref="ArgumentException"/>.
        /// </summary>
        [Fact]
        public async Task CreateSaveGameRecord_NullGameDataAutosave_ThrowArgumentException()
        {
            // Arrange & Act
            await _gameRepository.CreateSaveGameRecord(null!);

            // Assert
            _eventLogger.Verify(message =>
                message.LogCritical(It.IsAny<string>(), It.IsAny<ArgumentException>(), null, false));
        }

        /// <summary>
        /// Attempts to create a record containing null data in the SaveGames table. Should throw an 
        /// <see cref="ArgumentException"/>.
        /// </summary>
        [Fact]
        public async Task CreateSaveGameRecord_NullGameDataSaveGame_ThrowArgumentException()
        {
            // Arrange & Act
            await _gameRepository.CreateSaveGameRecord(null!, TestName, SaveGameTable.SaveGames);

            // Assert
            _eventLogger.Verify(message =>
                message.LogCritical(It.IsAny<string>(), It.IsAny<ArgumentException>(), null, false));
        }

        /// <summary>
        /// Attempts to create an Autosave record with whitespace game data. Should log a critical message.
        /// </summary>
        [Fact]
        public async Task CreateSaveGameRecord_WhiteSpaceGameDataAutosave_ThrowsArgumentException()
        {
            //Arrange & Act
            await _gameRepository.CreateSaveGameRecord(WhiteSpace);

            //Assert
            _eventLogger.Verify(message =>
                message.LogCritical(It.IsAny<string>(), It.IsAny<ArgumentException>(), null, false));
        }

        /// <summary>
        /// Attempts to create a SaveGames record with whitespace game data. Should log a critical message.
        /// </summary>
        [Fact]
        public async Task CreateSaveGameRecord_WhiteSpaceGameDataSaveGames_LogsCriticalMessage()
        {
            //Arrange & Act
            await _gameRepository.CreateSaveGameRecord(WhiteSpace, TestName, SaveGameTable.SaveGames);

            //Assert
            _eventLogger.Verify(message =>
                message.LogCritical(It.IsAny<string>(), It.IsAny<ArgumentException>(), null, false));
        }

        /// <summary>
        /// Attempts to create a record with a null game name. Should create an autosave record with Name set 
        /// to "Autosave".
        /// </summary>
        [Fact]
        public async Task CreateSaveGameRecord_NullGameNameAutosave_CreatesRecordSuccessfully()
        {
            //Arrange
            ClearTable(SaveGameTable.AutosaveGame);

            //Act
            await _gameRepository.CreateSaveGameRecord(TestData, null!);

            //Assert
            Assert.True(CountTableEntries(SaveGameTable.AutosaveGame) == 1);
            Assert.True(CheckForGameName(Autosave, SaveGameTable.AutosaveGame));
        }

        /// <summary>
        /// Attempts to create a record with a null game name in the SaveGames table. Should log a critical 
        /// <see cref="EventLogger"/> message.
        /// </summary>
        [Fact]
        public async Task CreateSaveGameRecord_NullGameNameSaveGame_LogsCriticalMessage()
        {
            //Arrange
            ClearTable(SaveGameTable.SaveGames);

            //Act
            await _gameRepository.CreateSaveGameRecord(TestData, null!, SaveGameTable.SaveGames);

            //Assert
            _eventLogger.Verify(message =>
                message.LogCritical(It.IsAny<string>(), It.IsAny<ArgumentException>(), null, false));
        }

        /// <summary>
        /// Attempts to create a record with valid game name and data but no table name. Should throw an <see 
        /// cref="InvalidOperationException"/>.
        /// </summary>
        [Fact]
        public async Task CreateSaveGameRecord_ValidSaveGameNoTableName_ThrowsInvalidOperationException()
        {
            //Arrange & Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _gameRepository.CreateSaveGameRecord(WhiteSpace, TestName));

            //Assert
            Assert.Contains("Only the Autosave game can be saved in the AutosaveGame table", exception.Message);
        }

        /// <summary>
        /// Attempts to create a record with valid game data. Should create an autosave record with Name set 
        /// to "Autosave".
        /// </summary>
        [Fact]
        public async Task CreateSaveGameRecord_ValidDataAutosave_CreatesRecordSuccessfully()
        {
            //Arrange
            ClearTable(SaveGameTable.AutosaveGame);

            //Act
            await _gameRepository.CreateSaveGameRecord(TestData);

            //Assert
            Assert.True(CountTableEntries(SaveGameTable.AutosaveGame) == 1);
            Assert.True(CheckForGameName(Autosave, SaveGameTable.AutosaveGame));
        }

        /// <summary>
        /// Attempts to create a record with valid game data and a valid game name. Should create an SaveGames 
        /// record with Name set to <<see cref="TestName"/>>.
        /// </summary>
        [Fact]
        public async Task CreateSaveGameRecord_ValidGameDataAndNameSaveGames_CreatesRecordSuccessfully()
        {
            //Arrange
            ClearTable(SaveGameTable.SaveGames);

            //Act
            await _gameRepository.CreateSaveGameRecord(TestData, TestName, SaveGameTable.SaveGames);

            //Assert
            Assert.True(CountTableEntries(SaveGameTable.SaveGames) == 1);
            Assert.True(CheckForGameName(TestName, SaveGameTable.SaveGames));
        }
        #endregion //Create Tests

        #region Read Tests
        /// <summary>
        /// Attempts to read data from the SaveGames table with a null game name. Should try to read data with 
        /// Name "Autosave" from the SaveGames table and return null.
        /// </summary>
        [Fact]
        public async Task ReadSaveGameData_GameNameNullSaveGames_ReturnsNull()
        {
            //Arrange
            ClearTable(SaveGameTable.SaveGames);
            await _gameRepository.CreateSaveGameRecord(TestData, TestName, SaveGameTable.SaveGames);

            //Act
            var data = await _gameRepository.ReadSaveGameData(null!, SaveGameTable.SaveGames);

            //Assert
            Assert.Null(data);
        }

        /// <summary>
        /// Attempts to read data from the SaveGames table with a whitespace game name. Logs a critical <see 
        /// cref="EventLogger"/> message and returns null.
        /// </summary>
        [Fact]
        public async Task ReadSaveGameData_GameNameWhitespaceSaveGames_ReturnsNullLogsCriticalMessage()
        {
            //Arrange
            ClearTable(SaveGameTable.SaveGames);
            await _gameRepository.CreateSaveGameRecord(TestData, TestName, SaveGameTable.SaveGames);

            //Act
            var data = await _gameRepository.ReadSaveGameData(WhiteSpace, SaveGameTable.SaveGames);

            //Assert
            Assert.Null(data);
            _eventLogger.Verify(message =>
                message.LogCritical(It.IsAny<string>(), It.IsAny<ArgumentException>(), null, false));
        }

        /// <summary>
        /// Attempts to read data from the AutosaveGame table with a white space game name. Should log a critical 
        /// <see cref="EventLogger"/> message and return null.
        /// </summary>
        [Fact]
        public async Task ReadSaveGameData_GameNameWhitespaceAutosaveGame_ReturnsNullLogsCriticalMessage()
        {
            //Arrange
            ClearTable(SaveGameTable.AutosaveGame);
            await _gameRepository.CreateSaveGameRecord(TestData);

            //Act
            var data = await _gameRepository.ReadSaveGameData(WhiteSpace, SaveGameTable.AutosaveGame);

            //Assert
            Assert.Null(data);
            _eventLogger.Verify(message =>
                message.LogCritical(It.IsAny<string>(), It.IsAny<ArgumentException>(), null, false));
        }

        /// <summary>
        /// Attempts to read data from the AutosaveGame table. Should return the test data.
        /// </summary>
        [Fact]
        public async Task ReadSaveGameData_ValidGameNameAutosave_ReturnsTestData()
        {
            //Arrange
            ClearTable(SaveGameTable.AutosaveGame);
            await _gameRepository.CreateSaveGameRecord(TestData);


            //Act
            var data = await _gameRepository.ReadSaveGameData();

            //Assert
            Assert.True(CountTableEntries(SaveGameTable.AutosaveGame) == 1);
            Assert.NotNull(data);
            Assert.Equal(TestData, data);
        }

        /// <summary>
        /// Attempts to read data from the SaveGames table. Should return the test data.
        /// </summary>
        [Fact]
        public async Task ReadSaveGameData_ValidGameNameSaveGames_ReturnsTestData()
        {
            //Arrange
            ClearTable(SaveGameTable.SaveGames);
            await _gameRepository.CreateSaveGameRecord(TestData, TestName, SaveGameTable.SaveGames);


            //Act
            var data = await _gameRepository.ReadSaveGameData(TestName, SaveGameTable.SaveGames);

            //Assert
            Assert.True(CountTableEntries(SaveGameTable.SaveGames) == 1);
            Assert.NotNull(data);
            Assert.Equal(TestData, data);
        }
        #endregion //Read Tests

        #region Update Tests
        /// <summary>
        /// Attempt to update the Autosave table with null value data. Should return false and log a critical 
        /// <see cref="EventLogger"/> message.
        /// </summary>
        [Fact]
        public async Task UpdateSaveGameRecord_NullGameDataAutosave_ReturnsFalseLogsCriticalMessage()
        {
            //Arrange
            ClearTable(SaveGameTable.AutosaveGame);
            await _gameRepository.CreateSaveGameRecord(TestData);

            //Act
            var isUpdated = await _gameRepository.UpdateSaveGameRecord(null!);

            //Assert
            Assert.False(isUpdated);
            _eventLogger.Verify(message =>
                message.LogCritical(It.IsAny<string>(), It.IsAny<ArgumentException>(), null, false));
        }

        /// <summary>
        /// Attempt to update the SaveGames table with null value data. Should return false and log a critical 
        /// <see cref="EventLogger"/> message.
        /// </summary>
        [Fact]
        public async Task UpdateSaveGameRecord_NullGameDataSaveGames_ReturnsFalseLogsCriticalMessage()
        {
            //Arrange
            ClearTable(SaveGameTable.SaveGames);
            await _gameRepository.CreateSaveGameRecord(TestData, TestName, SaveGameTable.SaveGames);

            //Act
            var isUpdated = await _gameRepository.UpdateSaveGameRecord(null!, TestName, SaveGameTable.SaveGames);

            //Assert
            Assert.False(isUpdated);
            _eventLogger.Verify(message =>
                message.LogCritical(It.IsAny<string>(), It.IsAny<ArgumentException>(), null, false));
        }

        /// <summary>
        /// Attempt to update the Autosave table with whitespace. Should return false and log a critical 
        /// <see cref="EventLogger"/> message.
        /// </summary>
        [Fact]
        public async Task UpdateSaveGameRecord_WhitespaceGameDataAutosave_ReturnsFalseLogsCriticalMessage()
        {
            //Arrange
            ClearTable(SaveGameTable.AutosaveGame);
            await _gameRepository.CreateSaveGameRecord(TestData);

            //Act
            var isUpdated = await _gameRepository.UpdateSaveGameRecord(WhiteSpace);

            //Assert
            Assert.False(isUpdated);
            _eventLogger.Verify(message =>
                message.LogCritical(It.IsAny<string>(), It.IsAny<ArgumentException>(), null, false));
        }

        /// Attempt to update the SaveGames table with whitespace. Should return false and log a critical 
        /// <see cref="EventLogger"/> message.
        /// </summary>
        [Fact]
        public async Task UpdateSaveGameRecord_WhitespaceGameDataSaveGames_ReturnsFalseLogsCriticalMessage()
        {
            //Arrange
            ClearTable(SaveGameTable.SaveGames);
            await _gameRepository.CreateSaveGameRecord(TestData, TestName, SaveGameTable.SaveGames);

            //Act
            var isUpdated = await _gameRepository.UpdateSaveGameRecord(WhiteSpace, TestName, SaveGameTable.SaveGames);

            //Assert
            Assert.False(isUpdated);
            _eventLogger.Verify(message =>
                message.LogCritical(It.IsAny<string>(), It.IsAny<ArgumentException>(), null, false));
        }

        /// Attempt update with a valid game name and valid data but without setting the <see cref="SaveGameTable"/>. 
        /// Should return false and log two warnings and one critical <see cref="EventLogger"/> message.
        /// </summary>
        [Fact]
        public async Task UpdateSaveGameRecord_ValidDataAndNameNoTableName_ReturnsFalseLogsTwoWarningsOneCritical()
        {
            //Arrange
            ClearTable(SaveGameTable.AutosaveGame);
            await _gameRepository.CreateSaveGameRecord(TestData);

            //Act
            var isUpdated = await _gameRepository.UpdateSaveGameRecord(UpdateData, TestName);

            //Assert
            Assert.False(isUpdated);
            _eventLogger.Verify(message =>
                message.LogWarning(It.IsAny<string>(), null), Times.Exactly(2));
            _eventLogger.Verify(message =>
                message.LogCritical(It.IsAny<string>(), It.IsAny<InvalidOperationException>(), null, false), Times.Once);
        }

        /// Attempt update with a valid game data to the Save Games table but without setting the game name. 
        /// Should return false and log two warnings and one critical <see cref="EventLogger"/> message.
        /// </summary>
        [Fact]
        public async Task UpdateSaveGameRecord_ValidDataNullNameSaveGames_ReturnsFalseLogsTwoWarningsOneCritical()
        {
            //Arrange
            ClearTable(SaveGameTable.SaveGames);
            await _gameRepository.CreateSaveGameRecord(TestData);

            //Act
            var isUpdated = await _gameRepository.UpdateSaveGameRecord(UpdateData, null!, SaveGameTable.SaveGames);

            //Assert
            Assert.False(isUpdated);
            _eventLogger.Verify(message =>
                message.LogWarning(It.IsAny<string>(), null), Times.Exactly(2));
            _eventLogger.Verify(message =>
                message.LogCritical(It.IsAny<string>(), It.IsAny<InvalidOperationException>(), null, false), Times.Once);
        }

        /// <summary>
        /// Updates the AutosaveGame table with valid data. Data read from AutosaveGame table should match 
        /// the update data and update should return true.
        /// </summary>
        [Fact]
        public async Task UpdateSaveGameRecord_ValidDataAutosave_ReturnsTrueStoredDataEqualsUpdateData()
        {
            //Arrange
            ClearTable(SaveGameTable.AutosaveGame);
            await _gameRepository.CreateSaveGameRecord(TestData);

            //Act
            var isUpdated = await _gameRepository.UpdateSaveGameRecord(UpdateData);
            var newData = await _gameRepository.ReadSaveGameData();

            //Assert
            Assert.True(isUpdated);
            Assert.Equal(UpdateData, newData);
            Assert.True(CountTableEntries(SaveGameTable.AutosaveGame) == 1);
        }

        /// <summary>
        /// Updates the test record of the SaveGames table with valid data. Data read from SaveGames table 
        /// should match the update data and update should return true.
        /// </summary>
        [Fact]
        public async Task UpdateSaveGameRecord_ValidDataAndNameSaveGames_ReturnsTrueStoredDataEqualsUpdateData()
        {
            //Arrange
            ClearTable(SaveGameTable.SaveGames);
            await _gameRepository.CreateSaveGameRecord(TestData, TestName, SaveGameTable.SaveGames);

            //Act
            var isUpdated = await _gameRepository.UpdateSaveGameRecord(UpdateData, TestName, SaveGameTable.SaveGames);
            var newData = await _gameRepository.ReadSaveGameData(TestName, SaveGameTable.SaveGames);

            //Assert
            Assert.True(isUpdated);
            Assert.Equal(UpdateData, newData);
            Assert.True(CountTableEntries(SaveGameTable.SaveGames) == 1);
        }
        #endregion //Update Tests

        #region Delete Tests
        /// <summary>
        /// Attempts to delete a game with null name from SaveGames. This should try three times, logging 
        /// two warnings and one critical <see cref="EventLogger"/> message then return false.
        /// </summary>
        [Fact]
        public async Task DeleteSaveGameRecord_NullNameSaveGames_ReturnsFalseLogsTwoWarningsOneCritical()
        {
            //Arrange
            ClearTable(SaveGameTable.SaveGames);
            await _gameRepository.CreateSaveGameRecord(TestData, TestName, SaveGameTable.SaveGames);

            //Act
            var isDeleted = await _gameRepository.DeleteSaveGameRecord(null!, SaveGameTable.SaveGames);

            //Assert
            Assert.False(isDeleted);
            _eventLogger.Verify(message =>
                message.LogWarning(It.IsAny<string>(), null), Times.Exactly(2));
            _eventLogger.Verify(message =>
                message.LogCritical(It.IsAny<string>(), It.IsAny<InvalidOperationException>(), null, false), Times.Once);
            Assert.True(CountTableEntries(SaveGameTable.SaveGames) == 1);
        }

        /// <summary>
        /// Attempts to delete a game with an invalid name from SaveGames. This should try three times, logging 
        /// two warnings and one critical <see cref="EventLogger"/> message then return false.
        /// </summary>
        [Fact]
        public async Task DeleteSaveGameRecord_InvalidNameSaveGames_ReturnsFalseLogsTwoWarningsOneCritical()
        {
            //Arrange
            ClearTable(SaveGameTable.SaveGames);
            await _gameRepository.CreateSaveGameRecord(TestData, TestName, SaveGameTable.SaveGames);

            //Act
            var isDeleted = await _gameRepository.DeleteSaveGameRecord(InvalidName, SaveGameTable.SaveGames);

            //Assert
            Assert.False(isDeleted);
            _eventLogger.Verify(message =>
                message.LogWarning(It.IsAny<string>(), null), Times.Exactly(2));
            _eventLogger.Verify(message =>
                message.LogCritical(It.IsAny<string>(), It.IsAny<InvalidOperationException>(), null, false), Times.Once);
            Assert.True(CountTableEntries(SaveGameTable.SaveGames) == 1);
        }

        /// <summary>
        /// Attempts to delete a game with an invalid name from Autosave Game. This should try three times, 
        /// logging two warnings and one critical <see cref="EventLogger"/> message then return false.
        /// </summary>
        [Fact]
        public async Task DeleteSaveGameRecord_InvalidNameAutosave_ReturnsFalseLogsTwoWarningsOneCritical()
        {
            //Arrange
            ClearTable(SaveGameTable.AutosaveGame);
            await _gameRepository.CreateSaveGameRecord(TestData);

            //Act
            var isDeleted = await _gameRepository.DeleteSaveGameRecord(InvalidName);

            //Assert
            Assert.False(isDeleted);
            _eventLogger.Verify(message =>
                message.LogWarning(It.IsAny<string>(), null), Times.Exactly(2));
            _eventLogger.Verify(message =>
                message.LogCritical(It.IsAny<string>(), It.IsAny<InvalidOperationException>(), null, false), Times.Once);
            Assert.True(CountTableEntries(SaveGameTable.AutosaveGame) == 1);
        }

        /// <summary>
        /// Attempts to delete a game with a whitespace name from Autosave Games. This should log a critical 
        /// <see cref="EventLogger"/> message then return false.
        /// </summary>
        [Fact]
        public async Task DeleteSaveGameRecord_WhiteSpaceNameAutosave_ReturnsFalseLogsCriticalMessage()
        {
            //Arrange
            ClearTable(SaveGameTable.AutosaveGame);
            await _gameRepository.CreateSaveGameRecord(TestData);

            //Act
            var isDeleted = await _gameRepository.DeleteSaveGameRecord(WhiteSpace);

            //Assert
            Assert.False(isDeleted);
            _eventLogger.Verify(message =>
                message.LogCritical(It.IsAny<string>(), It.IsAny<ArgumentException>(), null, false), Times.Once);
            Assert.True(CountTableEntries(SaveGameTable.AutosaveGame) == 1);
        }

        /// <summary>
        /// Attempts to delete a game with a whitespace name from Save Games. This should log a critical 
        /// <see cref="EventLogger"/> message then return false.
        /// </summary>
        [Fact]
        public async Task DeleteSaveGameRecord_WhiteSpaceNameSaveGames_ReturnsFalseLogsCriticalMessage()
        {
            //Arrange
            ClearTable(SaveGameTable.SaveGames);
            await _gameRepository.CreateSaveGameRecord(TestData, TestName, SaveGameTable.SaveGames);

            //Act
            var isDeleted = await _gameRepository.DeleteSaveGameRecord(WhiteSpace, SaveGameTable.SaveGames);

            //Assert
            Assert.False(isDeleted);
            _eventLogger.Verify(message =>
                message.LogCritical(It.IsAny<string>(), It.IsAny<ArgumentException>(), null, false), Times.Once);
            Assert.True(CountTableEntries(SaveGameTable.SaveGames) == 1);
        }

        /// <summary>
        /// Attempts to delete a save game record from an empty AutosaveGame table. Should log two warning 
        /// and one critical <see cref="EventLogger"/> messages then return false.
        /// </summary>
        [Fact]
        public async Task DeleteSaveGameRecord_EmptyDatabaseTableAutosave_ReturnsFalseLogsTwoWarningsOneCritical()
        {
            //Arrange
            ClearTable(SaveGameTable.AutosaveGame);

            //Act
            var isDeleted = await _gameRepository.DeleteSaveGameRecord();

            //Assert
            Assert.False(isDeleted);
            _eventLogger.Verify(message =>
                message.LogWarning(It.IsAny<string>(), null), Times.Exactly(2));
            _eventLogger.Verify(message =>
                message.LogCritical(It.IsAny<string>(), It.IsAny<InvalidOperationException>(), null, false), Times.Once);
            Assert.True(CountTableEntries(SaveGameTable.AutosaveGame) == 0);
        }

        /// <summary>
        /// Attempts to delete a save game record from an empty SaveGames table. Should log two warning 
        /// and one critical <see cref="EventLogger"/> messages then return false.
        /// </summary>
        [Fact]
        public async Task DeleteSaveGameRecord_EmptyDatabaseTableSaveGames_ReturnsFalseLogsTwoWarningsOneCritical()
        {
            //Arrange
            ClearTable(SaveGameTable.SaveGames);

            //Act
            var isDeleted = await _gameRepository.DeleteSaveGameRecord(TestName, SaveGameTable.SaveGames);

            //Assert
            Assert.False(isDeleted);
            _eventLogger.Verify(message =>
                message.LogWarning(It.IsAny<string>(), null), Times.Exactly(2));
            _eventLogger.Verify(message =>
                message.LogCritical(It.IsAny<string>(), It.IsAny<InvalidOperationException>(), null, false), Times.Once);
            Assert.True(CountTableEntries(SaveGameTable.SaveGames) == 0);
        }

        /// <summary>
        /// Attempts to delete a save game record from a valid AutosaveGame table. Should return true and 
        /// attempts to read the data should be null.
        /// </summary>
        [Fact]
        public async Task DeleteSaveGameRecord_ValidEntryAutosave_ReturnsTrueTableEmpty()
        {
            //Arrange
            ClearTable(SaveGameTable.AutosaveGame);
            await _gameRepository.CreateSaveGameRecord(TestData);

            //Act
            var isDeleted = await _gameRepository.DeleteSaveGameRecord();
            var storedData = await _gameRepository.ReadSaveGameData();

            //Assert
            Assert.True(isDeleted);
            Assert.Null(storedData);
            Assert.True(CountTableEntries(SaveGameTable.AutosaveGame) == 0);
        }

        /// <summary>
        /// Attempts to delete a save game record from a valid SaveGames table. Should return true and 
        /// attempts to read the data should be null.
        /// </summary>
        [Fact]
        public async Task DeleteSaveGameRecord_ValidEntrySaveGames_ReturnsTrueTableEmpty()
        {
            //Arrange
            ClearTable(SaveGameTable.SaveGames);
            await _gameRepository.CreateSaveGameRecord(TestData, TestName, SaveGameTable.SaveGames);

            //Act
            var isDeleted = await _gameRepository.DeleteSaveGameRecord(TestName, SaveGameTable.SaveGames);
            var storedData = await _gameRepository.ReadSaveGameData(TestName, SaveGameTable.SaveGames);

            //Assert
            Assert.True(isDeleted);
            Assert.Null(storedData);
            Assert.True(CountTableEntries(SaveGameTable.SaveGames) == 0);
        }
        #endregion //Delete Tests

        #region Async Tests
        [Fact]
        public async Task CreateSaveGameRecord_RunMultipleTasks_TenEntryLimitReached()
        {
            //Arrange
            ClearTable(SaveGameTable.SaveGames);
            var tasks = new List<Task>();

            for (int i = 0; i < 50; i++)
                tasks.Add(_gameRepository.CreateSaveGameRecord($"GameData_{i}", $"GameName_{i}", SaveGameTable.SaveGames));

            //Act
            await Task.WhenAll(tasks);
            var sampleData = await _gameRepository.ReadSaveGameData("GameName_2", SaveGameTable.SaveGames);

            //Assert
            Assert.True(CountTableEntries(SaveGameTable.SaveGames) == 10);
            Assert.Equal("GameData_2", sampleData);
        }
        #endregion //Async Tests

        #region Test Helper Methods
        /// <summary>
        /// Checks how many entries are stored in a given database table. Used to validate results from <see 
        /// cref="GameRepository" methods.
        /// </summary>
        /// <param name="table">The <see cref="SaveGameTable"/> name to be checked.</param>
        /// <returns>
        /// An integer value representing the number of entries in the database <paramref name="table"/>.
        /// </returns>
        private int CountTableEntries(SaveGameTable table)
        {
            var commandString = @"SELECT COUNT(*) FROM {0};";
            commandString = string.Format(commandString, table);

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            using var command = new SQLiteCommand(commandString, connection);

            return Convert.ToInt32(command.ExecuteScalar());
        }

        /// <summary>
        /// Deletes all data from a given table to ensure that it is clear before running tests. Used to avoid 
        /// unexpected entries in the table.
        /// </summary>
        /// <param name="table">The <see cref="SaveGameTable"/> name to be cleared.</param>
        private void ClearTable(SaveGameTable table)
        {
            var commandString = @"DELETE FROM {0};";
            commandString = string.Format(commandString, table);

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            using var command = new SQLiteCommand(commandString, connection);

            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Checks if the given <see cref="SaveGameTable"/> has exactly one entry with the given name <paramref 
        /// name="gameName"/>. Used to check if a record has been successfully created.
        /// </summary>
        /// <param name="gameName">A string representing the expected Name attribute.</param>
        /// <param name="table">The <see cref="SaveGameTable"/> to be checked.</param>
        /// <returns>A boolean value indicating whether exactly one record with Name <paramref name="gameName"/> 
        /// exists.</returns>
        /// <remarks>This method returns false if multiple records with the same name exist.</remarks>
        private bool CheckForGameName(string gameName, SaveGameTable table)
        {
            var commandString = @"SELECT COUNT(*) FROM {0} WHERE Name = @gameName;";
            commandString = string.Format(commandString, table);

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            using var command = new SQLiteCommand(commandString, connection);
            command.Parameters.AddWithValue("@gameName", gameName);

            return Convert.ToInt32(command.ExecuteScalar()) == 1;
        }
        #endregion //Test Helper Methods
    }
}
