using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battleships.MVVM.Factories;
using Battleships.MVVM.Enums;

namespace Battleships.MVVM.Services.Database
{
    public interface IGameRepository
    {
        public Task<bool> CheckForAutosaveFile();
        public Task<bool> CreateSaveGameRecord(string newGameData, string? gameName = null, SaveGameTable? tableName = null, int? saveSlot = null);
        public Task<string?> ReadSaveGameData(string? gameName = null, SaveGameTable? tableName = null);
        public Task<bool> UpdateSaveGameRecord(string newGameData, string? gameName = null, SaveGameTable? tableName = null, int? saveSlot = null);
        public Task<bool> DeleteSaveGameRecord(string? gameName = null, SaveGameTable? tableName = null);
        public Task<List<(string, DateTime, int)>> GenerateSaveGameList(bool includeAutosave = false);
    }

    /// <summary>
    /// A class that facilitates CRUD operations on an SQLite database which stores save game data.
    /// </summary>
    /// <param name="eventLogger">An instance of the ILogger interface used to log events in an orderly manner 
    /// with sufficient detail.</param>
    /// <param name="connectionString"></param>
    /// <param name="databaseFilePath"></param>
    public class GameRepository(ILoggerFactory loggerFactory, string connectionString) : IGameRepository
    {
        #region Fields
        private const string _eventSourceName = nameof(GameRepository);
        private readonly string _connectionString = connectionString 
            ?? throw new ArgumentNullException(nameof(connectionString), "Connection string cannot be null.");
        private readonly IEventLogger _eventLogger = loggerFactory.CreateLogger(_eventSourceName) 
            ?? throw new ArgumentNullException(nameof(loggerFactory), "Logger factory cannot be null.");
        #endregion //Fields

        #region CRUD operation values
        private const int RetryCount = 3;
        private const int TotalSaveGameSlots = 10;
        private readonly string _countRecordsCommand = @"SELECT COUNT(*) FROM {0};";
        private readonly string _createRecordCommand = @"INSERT OR REPLACE INTO {0} (Name, SaveTime, GameData, SaveSlot) VALUES (@gameName, @saveTime, @gameData, @saveSlot);";
        private readonly string _readGameDataCommand = @"SELECT GameData FROM {0} WHERE Name = @gameName LIMIT 1;";
        private readonly string _updateGameDataCommand = @"UPDATE {0} SET SaveTime = @saveTime, GameData = @gameData WHERE Name = @gameName;";
        private readonly string _deleteGameRecordCommand = @"DELETE FROM {0} WHERE Name = @gameName;";
        private const string ReadAutosaveTimeCommand = @"SELECT SaveTime FROM AutosaveGame WHERE Name = @gameName LIMIT 1;";
        private const string ReadSaveGameNamesCommand = @"SELECT Name, SaveTime, SaveSlot FROM SaveGames;";
        #endregion //CRUD operation values

        #region Database CRUD Methods

        /// <summary>
        /// Checks the Autosave table in the database for a record with the "Name" attribute set to "Autosave".
        /// </summary>
        /// <returns>A boolean value indicating whether the record exists.</returns>
        public async Task<bool> CheckForAutosaveFile()
        {
            var countQuery = string.Format(_countRecordsCommand, SaveGameTable.AutosaveGame);

            try
            {
                using var connection = new SQLiteConnection(_connectionString);
                await connection.OpenAsync();

                for (int i = 0; i < RetryCount; i++)
                {
                    try
                    {
                        using var command = new SQLiteCommand(countQuery, connection);

                        var result = await command.ExecuteScalarAsync();

                        if (Convert.ToInt32(result) > 0)
                            return true;
                    }
                    catch (Exception ex) when (i == RetryCount - 1)
                    {
                        _eventLogger.LogCritical($"Final attempt to check for autosave file failed.", ex);
                    }
                    catch (SQLiteException ex)
                    {
                        _eventLogger.LogWarning($"Database operation failed: {ex.Message}. Attempt {i + 1} of {RetryCount}");
                    }
                    catch (Exception)
                    {
                        _eventLogger.LogWarning($"Failed to check for autosave file. Attempt {i + 1} of {RetryCount}.", _eventSourceName);
                    }
                }
            }
            catch (SQLiteException ex)
            {
                _eventLogger.LogCritical("Database table \"AutosaveGame\" could not be accessed", ex, _eventSourceName, true);
            }

            return false;
        }

        /// <summary>
        /// Creates a record with "Name" <paramref name="gameName"/> in the <paramref name="tableName"> table 
        /// with the game data <paramref name="newGameData"/>. It uses the SQLite "INSERT OR REPLACE" 
        /// command to allow the Autosave game to be overwritten if a previous record has not been properly 
        /// deleted on completion of a game. Also allows overwriting of the record if the user chooses to
        /// save over a previous record. Client side validation will ensure that the user wants to overwrite 
        /// the previous record.
        /// Returns a boolean to confirm whether the update has been successful.
        /// </summary>
        /// <param name="newGameData">A string containing an encrypted Json of the new autosave game data.</param>
        /// <param name="gameName">Optional name for the game. If not provided, defaults to "Autosave".</param>
        /// <param name="tableName">Optional name for the table. If not provided, defaults to "AutosaveGame".</param>
        /// <paramref name="saveSlot"/>An integer value representing the requested save slot.
        /// <returns>A boolean indicating whether or not the record was successfully created.</returns>
        public async Task<bool> CreateSaveGameRecord(string newGameData, string? gameName = null, SaveGameTable? tableName = null, int? saveSlot = null)
        {
            if (gameName is not null && tableName is null)
                throw new InvalidOperationException("Only the Autosave game can be saved in the AutosaveGame table. Include the correct table name.");

            saveSlot ??= 0;
            
            tableName ??= SaveGameTable.AutosaveGame;
            if (tableName == SaveGameTable.AutosaveGame)
                gameName ??= "Autosave";

            int rowsAffected = 0;

            var countQuery = string.Format(_countRecordsCommand, tableName);
            var createRecordQuery = string.Format(_createRecordCommand, tableName);

            try
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(gameName, nameof(gameName));
                ArgumentException.ThrowIfNullOrWhiteSpace(newGameData, nameof(newGameData));

                using var connection = new SQLiteConnection(_connectionString);
                await connection.OpenAsync();

                for (int i = 0; i < RetryCount; i++)
                {
                    using var transaction = connection.BeginTransaction();
   
                    try
                    {
                        var spaceAvailable = (tableName == SaveGameTable.AutosaveGame) || await IsSaveSlotAvailable(connection, countQuery);

                        if (!spaceAvailable)
                            throw new InvalidOperationException("Save game limit reached: cannot create new record.");

                        rowsAffected = await CreateOrUpdateRecord(connection, createRecordQuery, gameName, newGameData, (int)saveSlot);
                        Debug.WriteLine($"Rows affected: {rowsAffected}");
                        if (rowsAffected > 0) //Record created successfully
                        {
                            transaction.Commit();
                            _eventLogger.LogInformation($"Save game record created successfully: {gameName}", _eventSourceName);
                            break;
                        }

                        transaction.Rollback();
                    }
                    catch (Exception ex) when (i == RetryCount - 1)
                    {
                        _eventLogger.LogCritical($"Final attempt to create database record in {tableName} failed.", ex);
                        transaction.Rollback();
                    }
                    catch (SQLiteException ex)
                    {
                        _eventLogger.LogWarning($"Database operation failed: {ex.Message}. Attempt {i + 1} of {RetryCount}");
                        transaction.Rollback();
                    }
                    catch (Exception)
                    {
                        _eventLogger.LogWarning($"Failed to create database record in {tableName}. Attempt {i + 1} of {RetryCount}.", _eventSourceName);
                        transaction.Rollback();
                    }
                }

            }
            catch (SQLiteException ex)
            {
                _eventLogger.LogCritical($"SQLite database could not be accessed properly: {ex.Message}", ex, _eventSourceName, true);
            }
            catch (ArgumentException ex)
            {
                _eventLogger.LogCritical($"Invalid game name or data - save game record could not be created: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _eventLogger.LogCritical($"Autosave data could not be written to database: {ex.Message}", ex);
            }

            return rowsAffected > 0;
        }

        /// <summary>
        /// Reads the "GameData" field from <paramref name="gameName"/> record in the <paramref name="tableName"> 
        /// table. Returns null if the record does not exist or if the data cannot be read.
        /// </summary>
        /// <returns>A string containing the encrypted Json from the "GameData" field. If no data can be read 
        /// from the table, returns null.</returns>
        /// <exception cref="SQLiteException">Thrown when the database cannot be accessed.</exception>
        /// <exception cref="FileNotFoundException">Thrown when there is no data to be read from the 
        /// database.</exception>
        public async Task<string?> ReadSaveGameData(string? gameName = null, SaveGameTable? tableName = null)
        {
            gameName ??= "Autosave";
            tableName ??= SaveGameTable.AutosaveGame;

            var readGameDataQuery = string.Format(_readGameDataCommand, tableName);

            try
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(gameName);
                
                using var connection = new SQLiteConnection(_connectionString);
                await connection.OpenAsync();

                for (int i = 0; i < RetryCount; i++)
                {
                    try
                    {
                        using var command = new SQLiteCommand(readGameDataQuery, connection);
                        command.Parameters.AddWithValue("@gameName", gameName);

                        using var reader = await command.ExecuteReaderAsync();

                        if (await reader.ReadAsync())
                        {
                            var gameData = reader["GameData"]?.ToString();

                            return gameData;
                        }
                    }
                    catch (Exception ex) when (i == RetryCount - 1)
                    {
                        _eventLogger.LogCritical($"Final attempt to read database record in {tableName} failed.", ex);
                    }
                    catch (SQLiteException ex)
                    {
                        _eventLogger.LogWarning($"Database operation failed: {ex.Message}. Attempt {i + 1} of {RetryCount}");
                    }
                    catch (Exception)
                    {
                        _eventLogger.LogWarning($"Failed to read database record in {tableName}. Attempt {i + 1} of {RetryCount}.");
                    }
                }

            }
            catch (SQLiteException ex)
            {
                _eventLogger.LogCritical($"SQLite database could not be read from correctly: {ex.Message}", ex, _eventSourceName, true);
            }
            catch (FileNotFoundException ex)
            {
                _eventLogger.LogCritical($"Save game data could not be found: {ex.Message}", ex);
            }
            catch (ArgumentException ex)
            {
                _eventLogger.LogCritical($"Invalid game name - save game data could not be read: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _eventLogger.LogCritical($"Save game data could not be loaded: {ex.Message}", ex);
            }

            return null;
        }

        /// <summary>
        /// Updates the "GameData" field in the <paramref name="gameName"/> record of the <paramref name="tableName"/> 
        /// table with the data stored in the encrypted Json <paramref name="newGameData"> parameter and the 
        /// current date and time. Returns a boolean to confirm whether the update has been successful.
        /// </summary>
        /// <param name="newGameData">A string containing an encrypted Json of the new autosave game data.</param>
        /// <param name="gameName">String representing the game name. If not provided, defaults to "Autosave".</param>
        /// <param name="tableName">The name of the table where the data will be stored. If not provided, 
        /// defaults to "AutosaveGame".</param>
        /// <param name="saveSlot">An integer value representing the requested save slot.</param>
        /// <returns>A boolean indicating whether or not the record was successfully updated.</returns>
        public async Task<bool> UpdateSaveGameRecord(string newGameData, string? gameName = null, SaveGameTable? tableName = null, int? saveSlot = null)
        {
            gameName ??= "Autosave";
            tableName ??= SaveGameTable.AutosaveGame;
            saveSlot ??= 0;

            int rowsAffected = 0;

            string updateGameDataQuery = string.Format(_updateGameDataCommand, tableName);

            try
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(gameName, nameof(gameName));
                ArgumentException.ThrowIfNullOrWhiteSpace(newGameData, nameof(newGameData));

                using var connection = new SQLiteConnection(_connectionString);
                await connection.OpenAsync();

                for (int i = 0; i < RetryCount; i++)
                {
                    try
                    {
                        rowsAffected = await CreateOrUpdateRecord(connection, updateGameDataQuery, gameName, newGameData, (int)saveSlot);

                        if (rowsAffected > 0) //Record updated successfully
                        {
                            _eventLogger.LogInformation($"Save game record updated successfully: {gameName}", _eventSourceName);
                            break;
                        }
                        else throw new InvalidOperationException("No record was updated.");
                    }
                    catch (Exception ex) when (i == RetryCount - 1)
                    {
                        _eventLogger.LogCritical($"Final attempt to update database record in {tableName} failed.", ex);
                    }
                    catch (SQLiteException ex)
                    {
                        _eventLogger.LogWarning($"Database operation failed: {ex.Message}. Attempt {i + 1} of {RetryCount}");
                    }
                    catch (InvalidOperationException ex)
                    {
                        _eventLogger.LogWarning($"Operation was not successful: {ex.Message}. Attempt {i + 1} of {RetryCount}");
                    }                   
                    catch (Exception)
                    {
                        _eventLogger.LogWarning($"Failed to update database record in {tableName}. Attempt {i + 1} of {RetryCount}.");
                    }
                }
            }
            catch (SQLiteException ex)
            {
                _eventLogger.LogCritical($"SQLite database could not be accessed properly: {ex.Message}", ex, _eventSourceName, true);
            }
            catch (ArgumentException ex)
            {
                _eventLogger.LogCritical($"Invalid game name - save game data could not be updated: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _eventLogger.LogCritical($"Save game data could not be updated: {ex.Message}", ex);
            }

            return rowsAffected > 0;
        }

        /// <summary>
        /// Deletes the chosen record with Name attribute <paramref name="gameName"/> from the <paramref name="tableName"/> 
        /// table in the database. If no argument are provided, it will delete the "Autosave" record from the 
        /// "AutosaveGame" table. This can be used at the end of a game to ensure that the "Autosave" record 
        /// is removed to avoid reloading a game that has been completed.
        /// </summary>
        /// <param name="gameName">A string representing the name of the game. If not provided, defaults 
        /// to "Autosave".</param>
        /// <param name="tableName">The name of the table where the data will be stored. If not provided, 
        /// defaults to "AutosaveGame".</param>
        /// <returns>A boolean value representing whether the record was successfully deleted.</returns>
        public async Task<bool> DeleteSaveGameRecord(string? gameName = null, SaveGameTable? tableName = null)
        {
            gameName ??= "Autosave";
            tableName ??= SaveGameTable.AutosaveGame;

            int rowsAffected = 0;

            var deleteGameRecordQuery = string.Format(_deleteGameRecordCommand, tableName);

            try
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(gameName, nameof(gameName));

                using var connection = new SQLiteConnection(_connectionString);
                await connection.OpenAsync();

                for (int i = 0; i < RetryCount; i++)
                {
                    try
                    {
                        using var command = new SQLiteCommand(deleteGameRecordQuery, connection);
                        command.Parameters.AddWithValue("@gameName", gameName);

                        rowsAffected = await command.ExecuteNonQueryAsync();

                        if (rowsAffected > 0) //Record deleted successfully
                        {
                            _eventLogger.LogInformation($"Save game record deleted successfully: {gameName}", _eventSourceName);
                            break;
                        }

                        throw new InvalidOperationException("No record was deleted.");
                    }
                    catch (Exception ex) when (i == RetryCount - 1)
                    {
                        _eventLogger.LogCritical($"Final attempt to delete database record in {tableName} failed.", ex);
                    }
                    catch (SQLiteException ex)
                    {
                        _eventLogger.LogWarning($"Database operation failed: {ex.Message}. Attempt {i + 1} of {RetryCount}");
                    }
                    catch (InvalidOperationException ex)
                    {
                        _eventLogger.LogWarning($"Operation was not successful: {ex.Message}. Attempt {i + 1} of {RetryCount}");
                    }
                    catch (Exception)
                    {
                        _eventLogger.LogWarning($"Failed to delete database record in {tableName}. Attempt {i + 1} of {RetryCount}.");
                    }
                }                    
            }
            catch (SQLiteException ex)
            {
                _eventLogger.LogCritical($"SQLite database could not be accessed properly: {ex.Message}", ex, _eventSourceName, true);
            }
            catch (ArgumentException ex)
            {
                _eventLogger.LogCritical($"Invalid game name - save game data could not be deleted: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _eventLogger.LogCritical($"Save game data could not be deleted: {ex.Message}", ex);
            }

            return rowsAffected > 0;
        }

        /// <summary>
        /// Retrieves the "Name" attributes stored in the database with their associated "SaveTime" 
        /// values as a list of Tuples. The "includeAutosave" parameter determines whether or not the 
        /// list includes the "Autosave" record from the "AutosaveGame" table, if it exists. If the 
        /// database tables are empty, an empty list is returned to allow the application to continue. If 
        /// an error still occurs on the final attempt or before accessing the database, a null value is 
        /// added to the list to inform the calling class.
        /// </summary>
        /// <param name="includeAutosave">Determines whether or not the "Autosave" game should be included.</param>
        /// <returns>A List of tuples containing the string value for each record's "Name" attribute 
        /// with the DateTime value for the associated "SaveTime" attribute and "SaveSlot" integer.</returns>
        public async Task<List<(string, DateTime, int)>> GenerateSaveGameList(bool includeAutosave = false)
        {
            var saveGameList = new List<(string, DateTime, int)>();

            try
            {
                using var connection = new SQLiteConnection(_connectionString);
                await connection.OpenAsync();

                for (int i = 0; i < RetryCount; i++)
                {
                    try
                    {
                        var transaction = connection.BeginTransaction();

                        if (includeAutosave)
                        {
                            (string saveName, DateTime saveTime, int saveSlot) autosaveTuple = await ReadAutosaveDateTime(connection);

                            if (autosaveTuple.saveName == "Autosave")
                                saveGameList.Add(autosaveTuple);
                        }

                        foreach (var saveGameTuple in await ReadSaveGamesNamesAndDateTimes(connection))
                            saveGameList.Add(saveGameTuple);

                        if (saveGameList.Count > 0)
                        {
                            transaction.Commit();
                            _eventLogger.LogInformation($"Save game list created successfully.", _eventSourceName);
                            break;
                        }
                        else throw new InvalidOperationException("No records were found.");
                    }
                    catch (Exception ex) when (i == RetryCount - 1)
                    {
                        _eventLogger.LogCritical($"Final attempt to read database record in {SaveGameTable.SaveGames} failed.", ex);
                    }
                    catch (SQLiteException ex)
                    {
                        _eventLogger.LogWarning($"Database operation failed: {ex.Message}. Attempt {i + 1} of {RetryCount}");
                    }
                    catch (InvalidOperationException ex)
                    {
                        _eventLogger.LogWarning($"Operation was not successful: {ex.Message}. Attempt {i + 1} of {RetryCount}");
                    }
                    catch (Exception)
                    {
                        _eventLogger.LogWarning($"Failed to read database record in {SaveGameTable.SaveGames}. Attempt {i + 1} of {RetryCount}.");
                    }
                }
            }
            catch (SQLiteException ex)
            {
                _eventLogger.LogCritical($"SQLite database could not be accessed properly: {ex.Message}", ex, _eventSourceName, true);
            }
            catch (Exception ex)
            {
                _eventLogger.LogCritical($"Save game names list could not be created: {ex.Message}", ex);
            }

            return saveGameList;
        }


        #endregion //Database CRUD Methods

        #region Helper Methods
        /// <summary>
        /// Checks if there is space available in the database table to create a new record. It enables the 
        /// user to set a limit on the number of records that can be created in the table. This method is only 
        /// able to retrieve a single piece of data. It is primarily using for "SELECT COUNT(*)" commands to 
        /// count how many records are in the table (or find the size of a subset of records that meet a 
        /// particular condition).
        /// </summary>
        /// <param name="connection">The SQLite connection that has already been created. The connection 
        /// must be open before the method can be called.</param>
        /// <param name="query">A string representing the query. It can only aim to retrieve a single 
        /// piece of data.</param>
        /// <returns>A boolean value indicating whether or not the total number of records in the table is less 
        /// than the TotalSaveGameSlots constant.</returns>
        /// <exception cref="ArgumentException">Thrown if an invalid <paramref name="query"/> is passed to avoid further 
        /// exceptions when the command is created or executed.</exception>
        /// <example>
        /// The following example shows how the method can be used to count the number of records in a table.
        /// <code> 
        /// var query = @"SELECT COUNT(*) FROM SaveGames;";
        /// using var connection = new SQLiteConnection(_connectionString);
        /// bool availableSpace = await IsSaveSlotAvailable(connection, query);
        /// 
        /// var statement = availableSpace ? "The table has space." : "There is no space in the table";
        /// Console.WriteLine(statement);
        /// </code>
        /// </example>
        private static async Task<bool> IsSaveSlotAvailable(SQLiteConnection connection, string query)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(query, nameof(query));
            if (!query.Contains("SELECT COUNT(*)"))
                throw new ArgumentException("Query must be a SELECT COUNT(*) statement.", nameof(query));

            using var countCommand = new SQLiteCommand(query, connection);

            var count = await countCommand.ExecuteScalarAsync();

            int tableSize = Convert.ToInt32(count);

            return tableSize < TotalSaveGameSlots;
        }

        /// <summary>
        /// Runs the SQLite command to create or update a record in the database using the provided 
        /// <paramref name="connection"/> parameter. The command can either use and "INSERT INTO" 
        /// or "UPDATE" command depending on whether the record already exists. It uses the <paramref name="gameName"/>, 
        /// <paramref name="newGameData"/> and the current date and time to create the record. It enables the 
        /// CRUD operation methods to carry out the similar commands in a structured manner.
        /// </summary>
        /// <param name="connection">The SQLite connection that has been opened. The connection must be 
        /// opened before the method can be called.</param>
        /// <param name="query">A string representing the command to be created. It must 
        /// contain "INSERT" or "UPDATE" and is used to changed data in the table.</param>
        /// <param name="gameName">The name of the game to be stored in the "Name" field.</param>
        /// <param name="newGameData">A string containing an encrypted Json to be stored in the 
        /// "GameData" field.</param>
        /// <param name="newSaveSlot">An integer value representing the number of the save slot to be used.
        /// </param>
        /// <returns>An integer representing the number of rows affected by the operation.</returns>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="query"/> parameter is not a valid 
        /// command to execute. Avoids further exceptions if the method tries to create or execute an 
        /// invalid command."</exception>
        /// <example>
        /// The following example shows how the method can be used to create a new record in a table.
        /// <code>
        /// var query = @"INSERT INTO SaveGames (Name, SaveTime, GameData) VALUES (@gameName, @saveTime, @gameData);";
        /// var connection = new SQLiteConnection(_connectionString);
        /// 
        /// </code>
        /// </example>
        internal static async Task<int> CreateOrUpdateRecord(SQLiteConnection connection, string query, string gameName, string newGameData, int newSaveSlot)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(gameName, nameof(gameName));
            if (!query.Contains("INSERT") && !query.Contains("UPDATE"))
                throw new ArgumentException("Query must be an INSERT or UPDATE statement.", nameof(query));

            using var command = new SQLiteCommand(query, connection);

            command.Parameters.AddWithValue("@gameName", gameName);
            command.Parameters.AddWithValue("@saveTime", DateTime.Now);
            command.Parameters.AddWithValue("@gameData", newGameData);
            command.Parameters.AddWithValue("@saveSlot", newSaveSlot);

            int rowsAffected = await command.ExecuteNonQueryAsync();

            return rowsAffected;
        }

        /// <summary>
        /// A helper method for accessing the "AutosaveGame" table in the database and retrieving the SaveTime 
        /// field for the "Autosave" record. This is used to help creates a list of save games as Tuples 
        /// containing their Name and SaveTime attributes to be used for saving and loading game views.
        /// </summary>
        /// <param name="connection">The SQLite connection that has been created. This connection must 
        /// be established before running the method.</param>
        /// <returns>A Tuple containing the "Autosave" Name attribute and its associated SaveTime and SaveSlot attributes.</returns>
        private static async Task<(string, DateTime, int)> ReadAutosaveDateTime(SQLiteConnection connection)
        {
            using var command = new SQLiteCommand(ReadAutosaveTimeCommand, connection);
            command.Parameters.AddWithValue("@gameName", "Autosave");

            using var reader = await command.ExecuteReaderAsync();

            if (reader.HasRows)
            {
                while (await reader.ReadAsync())
                {
                    var saveTime = reader.GetDateTime(0);
                    return ("Autosave", saveTime, 0);
                }
            }

            await reader.DisposeAsync();
            
            return (string.Empty, DateTime.Now, 0);
        }

        /// <summary>
        /// Reads the GameName, SaveTime and SaveSLot attributes for each record in the SaveGames table. Stores 
        /// each set of attributes as a Tuple and returns a list of these Tuples. This is used for generating 
        /// the save and load pages to enable users to see which save games have been created.
        /// </summary>
        /// <param name="connection">The SQLite connection that has been created. This connection must be 
        /// established before the method can be called.</param>
        /// <returns>A List of Tuples containing each Name attribute from the SaveGames tables with its 
        /// associated SaveTime and SaveSlot attributes.</returns>
        private static async Task<List<(string, DateTime, int)>> ReadSaveGamesNamesAndDateTimes(SQLiteConnection connection)
        {
            var saveGameList = new List<(string, DateTime, int)>();

            using var command = new SQLiteCommand(ReadSaveGameNamesCommand, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var name = reader.GetString(0);
                var saveTime = reader.GetDateTime(1);
                var saveSlot = reader.GetInt32(2);

                saveGameList.Add((name, saveTime, saveSlot));
            }

            await reader.DisposeAsync();

            return saveGameList;
        }
        #endregion //Helper Methods
    }
}

