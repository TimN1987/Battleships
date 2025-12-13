using Battleships.MVVM.Factories;
using Battleships.MVVM.Services;
using Battleships.MVVM.Services.Database;
using Moq;

namespace BattleshipsTests.Tests.Database
{
    public class DatabaseInitializerTests : IDisposable
    {
        #region Fields
        private readonly Mock<ILoggerFactory> _loggerFactory;
        private readonly Mock<IEventLogger> _eventLogger;
        private readonly string _testDirectoryPath;
        private readonly string _testDatabasePath;
        #endregion //Fields

        public DatabaseInitializerTests()
        {

            _loggerFactory = new Mock<ILoggerFactory>();
            _eventLogger = new Mock<IEventLogger>();

            _loggerFactory
                .Setup(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(_eventLogger.Object);

            _testDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BattleshipsTests");
            _testDatabasePath = Path.Combine(_testDirectoryPath, "TestDatabase.db");

            if (!Directory.Exists(_testDirectoryPath))
                Directory.CreateDirectory(_testDirectoryPath);
        }

        /// <summary>
        /// Passes a null ILoggerFactory argument to the constructor. An ArgumentNullException 
        /// should be thrown.
        /// </summary>
        #region Constructor tests
        [Fact]
        public void Constructor_NullLoggerFactory_ThrowsArgumentNullException()
        {
            //Arrange & Act
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new DatabaseInitializer(null!, _testDatabasePath));

            //Assert
            Assert.Contains("loggerFactory", exception.Message);
        }

        /// <summary>
        /// Passes a null database path argument to the constructor. An ArgumentNullException 
        /// should be thrown.
        /// </summary>
        [Fact]
        public void Constructor_NullDatabasePath_ThrowsArgumentNullException()
        {
            //Arrange & Act
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new DatabaseInitializer(_loggerFactory.Object, null!));

            //Assert
            Assert.Contains("databaseFilePath", exception.Message);
        }

        /// <summary>
        /// Passes two valid arguments to the constructor. An object should be successfully 
        /// created by the Logger Factory.
        /// </summary>
        [Fact]
        public void Constructor_ValidArguments_CreatesInstanceSuccessfully()
        {
            //Arrange & Act
            var initializer = new DatabaseInitializer(_loggerFactory.Object, _testDatabasePath);

            //Assert
            Assert.NotNull(initializer);
        }
        #endregion //Constructor tests

        #region Database Creation tests
        /// <summary>
        /// Ensures that the database does not exists. The EnsureDatabaseExists method should create a 
        /// new database and return true.
        /// </summary>
        [Fact]
        public void EnsureDatabaseExists_DatabaseDoesNotExist_ReturnsTrue()
        {
            //Arrange
            CleanupTestDatabase();
            var initializer = new DatabaseInitializer(_loggerFactory.Object, _testDatabasePath);

            //Act
            var result = initializer.EnsureDatabaseExists();

            //Assert
            Assert.True(result);
        }

        /// <summary>
        /// Creates a database in the test database path directory. The EnsureDatabaseExists method 
        /// should confirm this file exists and return true.
        /// </summary>
        [Fact]
        public void EnsureDatabaseExists_DatabaseExists_ReturnsTrue()
        {
            //Arrange
            File.Create(_testDatabasePath).Dispose();
            var initializer = new DatabaseInitializer(_loggerFactory.Object, _testDatabasePath);

            //Act
            var result = initializer.EnsureDatabaseExists();

            //Assert
            Assert.True(result);
        }
        #endregion //Database Creation tests

        #region Database Initialization tests
        /// <summary>
        /// Mocks the InitializeDatabase method to succeed on the first attempt. The event logger should record 
        /// an information log highlighting that the database is correctly initialized.
        /// </summary>
        [Fact]
        public async Task InitializeDatabaseWithRetries_SucceedsOnFirstAttempt_LogsSuccessMessage()
        {
            // Arrange
            var initializer = new Mock<DatabaseInitializer>(_loggerFactory.Object, _testDatabasePath);

            // Act
            await initializer.Object.InitializeDatabaseWithRetries();

            // Assert
            _eventLogger.Verify(logger =>
                logger.LogInformation(It.Is<string>(msg => msg.Contains("Database successfully initialized")), null));
        }

        /// <summary>
        /// Mocks the InitializeDatabase method to succeed on the third attempt after two failures. The event 
        /// logger should record two warning logs highlighting that the initialization failed and one information 
        /// log highlighting that the database is correctly initialized.
        /// </summary>
        [Fact]
        public async Task InitializeDatabaseWithRetries_FirstTwoAttemptsFail_DatebaseCreated()
        {
            //Arrange
            CleanupTestDatabase();
            var initializer = new Mock<IDatabaseInitializer>();

            var result = initializer.SetupSequence(i => i.InitializeDatabase())
                                            .Throws(new Exception("First attempt failed."))
                                            .Throws(new Exception("Second attempt failed."))
                                            .ReturnsAsync(true);

            //Act
            await initializer.Object.InitializeDatabaseWithRetries();

            //Assert
            Assert.True(!File.Exists(_testDatabasePath));
        }

        /// <summary>
        /// Runs the InitializeDatabaseWithRetries method with no database stored in the correct directory. 
        /// It should create a new database with the correct tables before logging a success message.
        /// </summary>
        [Fact]
        public async Task InitializeDatabaseWithRetries_DatabaseDoesNotExist_LogsSuccessMessage()
        {
            //Arrange
            CleanupTestDatabase();
            var initializer = new Mock<DatabaseInitializer>(_loggerFactory.Object, _testDatabasePath);

            //Act
            await initializer.Object.InitializeDatabaseWithRetries();

            //Assert
            _eventLogger.Verify(logger => logger.LogInformation(It.Is<string>(msg => msg.Contains("Save games database is correctly initialized")), null));
        }

        #endregion //Database Initialization tests

        #region Cleanup
        /// <summary>
        /// Ensures that all data is cleared to avoid affecting future testing.
        /// </summary>
        public void Dispose()
        {
            CleanupTestDatabase();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Deletes the test database if it exists, ensuring that future database creation tests 
        /// can be run properly.
        /// </summary>
        private void CleanupTestDatabase()
        {
            if (File.Exists(_testDatabasePath))
            {
                File.Delete(_testDatabasePath);
                Assert.False(File.Exists(_testDatabasePath), "Test database was not properly deleted.");
            }
        }
        #endregion //Cleanup
    }
}