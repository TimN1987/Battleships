using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using Battleships.MVVM.Factories;

namespace Battleships.MVVM.Services.Database
{
    /// <summary>
    /// Defines a contract for initializing databases. This interface is used for creating a database with 
    /// an appropriate file path and creating the necessary tables in the database.
    /// </summary>
    public interface IDatabaseInitializer
    {
        Task<bool> InitializeDatabase();
        Task InitializeDatabaseWithRetries();
        event Action? DatabaseInitialized;
    }
    
    /// <summary>
    /// A class that initializes an SQLite Database and creates Tables to store save game data.
    /// </summary>
    public class DatabaseInitializer : IDatabaseInitializer
    {
        #region Fields
        private readonly string _connectionString;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IEventLogger _eventLogger;

        private const int RetryCount = 3;

        private const string CreateAutosaveGameTable = @"
                    CREATE TABLE IF NOT EXISTS AutosaveGame (
                        Name TEXT NOT NULL PRIMARY KEY,
                        SaveTime DATETIME NOT NULL,
                        GameData TEXT NOT NULL,
                        SaveSlot INTEGER NOT NULL UNIQUE
                    );";

        private const string CreateSaveGamesTable = @"
                    CREATE TABLE IF NOT EXISTS SaveGames (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        SaveTime DATETIME NOT NULL,
                        GameData TEXT NOT NULL,
                        SaveSlot INTEGER NOT NULL UNIQUE
                    );";
        #endregion //Fields

        public event Action? DatabaseInitialized;

        public DatabaseInitializer(ILoggerFactory loggerFactory, string connectionString)
        {
            _connectionString = connectionString 
                ?? throw new ArgumentNullException(nameof(connectionString), "Database file path cannot be null.");
            _loggerFactory = loggerFactory 
                ?? throw new ArgumentNullException(nameof(loggerFactory), "Logger factory cannot be null.");
            _eventLogger = _loggerFactory.CreateLogger(nameof(DatabaseInitializer)) ?? throw new ArgumentNullException(nameof(loggerFactory), "Event logger cannot be null.");
        }

        #region Database Initialization Methods
        /// <summary>
        /// Runs the InitializeDatabase method up to three times until the database is successfully 
        /// initialized. On completion, an <see cref="EventAggregator"/> <see cref="DatabaseInitializedEvent"/> 
        /// is published to inform the application that initialization is complete.
        /// </summary>
        /// <exception cref="SQLiteException">Thrown if the SQLite connection cannot be correctly 
        /// established after three attempts.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the database cannot be correctly 
        /// initialized after three attempts.</exception>
        /// <example>
        /// The following example shows how to initialize the database on startup.
        /// <code>
        /// private async Task InitializeDatabaseOnStartUp()
        /// {
        ///     var databaseInitializer = new DatabaseInitializer(loggerFactory, databaseFilePath);
        ///     await databaseInitializer.InitializeDatabaseWithRetries();     
        /// }
        /// </code>
        /// </example>
        public async Task InitializeDatabaseWithRetries()
        {
            bool databaseExists = false;

            try
            {
                for (int i = 0; i < RetryCount; i++)
                {
                    try
                    {
                        databaseExists = await InitializeDatabase();
                        _eventLogger.LogInformation($"Database successfully initialized on attempt {i + 1} of {RetryCount}.");
                        break;
                    }
                    catch (Exception)
                    {
                        if (i == 2) throw;

                        if (databaseExists)
                            _eventLogger.LogWarning($"Database exists, but save game table creation failed on attempt {i + 1}. {2 - i} tries remaining.");
                        else
                            _eventLogger.LogWarning($"Database initialization failed on attempt {i + 1}. {2 - i} retries left.");

                       await Task.Delay(500);
                    }
                }
            }
            catch (SQLiteException ex)
            {
                _eventLogger.LogCritical("SQLite connection could not be properly established after three attempts.", ex);
            }
            catch (InvalidOperationException ex)
            {
                if (databaseExists)
                    _eventLogger.LogCritical("Database exists, but save game tables could not be initialized correctly after three attempts.", ex);
                else
                    _eventLogger.LogCritical("Database could not be initialized after three attempts.", ex);
            }
            finally
            {
                DatabaseInitialized?.Invoke();
            }
        }

        /// <summary>
        /// Ensures that a database exists at the correct file path and ensures that the database contains 
        /// the correct tables for CRUD operations on save game data.
        /// </summary>
        /// <exception cref="SQLiteException">Thrown if the SQLite connection cannot be established correctly.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the database cannot be initialized 
        /// correctly.</exception>
        public async Task<bool> InitializeDatabase()
        {
            bool databaseExists;

            databaseExists = EnsureDatabaseExists();

            if (databaseExists)
                await EnsureSaveGameTablesExist();

            _eventLogger.LogInformation("Database successfully initialized.");
            return databaseExists;
        }

        /// <summary>
        /// Ensures that a database exists at the correct file path. Creates a new database if one does not 
        /// exist already.
        /// </summary>
        /// <returns>True if the database exists.</returns>
        public bool EnsureDatabaseExists()
        {
            try
            {
                if (!DatabaseFileExists())
                    CreateDatabaseFile();
            }
            catch (ArgumentNullException ex)
            {
                _eventLogger.LogCritical($"Database file path is invalid: {_connectionString}", ex);
                throw new ArgumentException($"Ensure that the database file path is a valid file path: {_connectionString}", ex);
            }

            _eventLogger.LogInformation("Save games database is correctly initialized.");
            return true;
        }

        /// <summary>
        /// Ensures that the database contains the correct tables. If they do not exist, creates new tables 
        /// to hold save game data.
        /// </summary>
        /// <exception cref="SQLiteException">Thrown if the SQLite connection is not established correctly.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the tables cannot be created.</exception>
        private async Task EnsureSaveGameTablesExist()
        {
            try
            {
                using var connection = new SQLiteConnection(_connectionString);
                await connection.OpenAsync();
                using var transaction = connection.BeginTransaction();
                try
                {
                    using var createAutosaveTableCommand = new SQLiteCommand(CreateAutosaveGameTable, connection, transaction);
                    await createAutosaveTableCommand.ExecuteNonQueryAsync();

                    using var createSaveGamesTableCommand = new SQLiteCommand(CreateSaveGamesTable, connection, transaction);
                    await createSaveGamesTableCommand.ExecuteNonQueryAsync();

                    await transaction.CommitAsync();
                }
                catch (SQLiteException ex)
                {
                    await transaction.RollbackAsync();
                    _eventLogger.LogWarning("Transaction rolled back due to SQLite exception: " + ex.Message);
                    throw;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Save game tables could not be created in database. {ex.Message}");
            }

            _eventLogger.LogInformation("Save game tables are correctly initialized.");
        }
        #endregion //Database Initialization Methods

        #region Helper Methods

        /// <summary>
        /// Retrieves the directory name from the database file path and returns it if it is not null.
        /// </summary>
        /// <returns>A non-null path to the database directory.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the directory path is null.</exception>
        private string GetValidDatabaseFolderPath()
        {
            string? directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Battleships");

            return directory ?? throw new ArgumentNullException(nameof(directory), "Directory path cannot be null.");
        }

        /// <summary>
        /// Checks if the database file exists at the correct file path.
        /// </summary>
        /// <returns>A boolean value indicating whether or not the database file exists.</returns>
        private bool DatabaseFileExists()
        {
            var directory = GetValidDatabaseFolderPath();
            var filePath = Path.Combine(directory, "savegames.sqlite");

            return File.Exists(filePath);
        }

        /// <summary>
        /// Checks that appropriate directory exists to store the database and creates the database file 
        /// in it.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the database directory cannot be found 
        /// or created.</exception>
        /// <exception cref="ArgumentNullException">Thrown if the generated directory path is null.</exception>
        /// <exception cref="SQLiteException">Thrown if the SQLite database cannot be created in the directory.</exception>
        private void CreateDatabaseFile()
        {
            Debug.WriteLine("Creating database file");
            string directory = GetValidDatabaseFolderPath();
            Debug.WriteLine($"Valid path retrieved: {directory}");
            EnsureDatabaseFolderExists(directory);
        }

        /// <summary>
        /// Checks if a directory exists at the desired path. If not, it creates one.
        /// </summary>
        /// <param name="directory">The folder path for the directory where the database will be stored.</param>
        /// <exception cref="InvalidOperationException">Thrown if the database directory cannot be found 
        /// or created.</exception>
        private static void EnsureDatabaseFolderExists(string directory)
        {
            Debug.WriteLine("Ensuring that database folder exists");
            
            try
            {
                ArgumentNullException.ThrowIfNullOrWhiteSpace(directory);
                Debug.WriteLine("Checking directory exists.");
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                Debug.WriteLine("Ensured directory exists");
            }
            catch (ArgumentNullException)
            {
                throw new ArgumentNullException(nameof(directory), "Directory path cannot be null.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Database directory could not be found or created.", ex);
            }
        }
        #endregion //Helper Methods
    }
}
