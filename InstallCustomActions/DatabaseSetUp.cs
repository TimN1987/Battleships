using System;
using System.Configuration;
using System.Data.SQLite;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Tracing;
using System.IO;
using System.Runtime.Versioning;

namespace InstallCustomActions
{
    public static class DatabaseSetUp
    {
        private const string EventSourceName = "DatabaseSetUp";

        #region Database Setup Methods
        /// <summary>
        /// Ensures that a database exists at the correct file path and sets up a new SQLite Connection.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the database cannot be found or created.</exception>
        public static void InitializeDatabase()
        {
            string databaseFilePath = CreateFilePath();

            try
            {
                EventLog.WriteEntry(EventSourceName, $"Initializing database at path: {databaseFilePath}", EventLogEntryType.Information);

                bool databaseExists = EnsureDatabaseExists(databaseFilePath);

                if (databaseExists)
                {
                    EventLog.WriteEntry(EventSourceName, "Database exists. Ensuring tables are set up.", EventLogEntryType.Information);
                    EnsureSaveGameTablesExist(databaseFilePath);
                }

                EventLog.WriteEntry(EventSourceName, "Database initialized successfully.", EventLogEntryType.Information);
            }
            catch (InvalidOperationException ex)
            {
                EventLog.WriteEntry(EventSourceName, $"Database initialization failed: {ex.Message}", EventLogEntryType.Error);
                throw new InvalidOperationException("Database could not be initialized correctly.", ex);
            }
        }

        private static string CreateFilePath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Battleships", "savegames.db");
        }

        /// <summary>
        /// Ensures that a database exists at the correct file path. Creates a new database if one does not 
        /// exist already.
        /// </summary>
        /// <returns>True if database exists or is created. False if no database can be created.</returns>
        private static bool EnsureDatabaseExists(string databaseFilePath)
        {
            if (!File.Exists(databaseFilePath))
            {
                try
                {
                    string directory = Path.GetDirectoryName(databaseFilePath);

                    if (directory is null)
                        throw new ArgumentException("File path is null.");

                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                        EventLog.WriteEntry(EventSourceName, $"Creating directory for database at path: {directory}", EventLogEntryType.Information);
                    }

                    SQLiteConnection.CreateFile(databaseFilePath);
                    EventLog.WriteEntry(EventSourceName, $"Database file created at path: {databaseFilePath}", EventLogEntryType.Information);
                }
                catch (Exception ex)
                {
                    EventLog.WriteEntry(EventSourceName, $"Database could not be found or created: {ex.Message}", EventLogEntryType.Error);
                    throw new InvalidOperationException("Database could not be found or created.", ex);
                }
            }

            return true;
        }

        /// <summary>
        /// Ensures that the database contains the correct tables. If they do not exist, creates new tables 
        /// to hold save game data.
        /// </summary>
        private static void EnsureSaveGameTablesExist(string databaseFilePath)
        {
            string createAutosaveGameTable = @"
                CREATE TABLE IF NOT EXISTS AutosaveGame (
                    Name TEXT NOT NULL PRIMARY KEY,
                    SaveTime DATETIME NOT NULL,
                    GameData TEXT NOT NULL
                );";

            string createSaveGamesTable = @"
                CREATE TABLE IF NOT EXISTS SaveGames (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    SaveTime DATETIME NOT NULL,
                    GameData TEXT NOT NULL
                );";

            try
            {
                using (var connection = new SQLiteConnection($"Data Source={databaseFilePath}"))
                {
                    EventLog.WriteEntry(EventSourceName, $"Opening database connection to: {databaseFilePath}", EventLogEntryType.Information);
                    connection.Open();

                    EventLog.WriteEntry(EventSourceName, "Creating AutosaveGame table.", EventLogEntryType.Information);
                    using (var command = new SQLiteCommand(createAutosaveGameTable, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    EventLog.WriteEntry(EventSourceName, "Creating SaveGames table.", EventLogEntryType.Information);
                    using (var command = new SQLiteCommand(createSaveGamesTable, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    EventLog.WriteEntry(EventSourceName, "Database tables created successfully.", EventLogEntryType.Information);
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(EventSourceName, $"Failed to create save game tables: {ex.Message}", EventLogEntryType.Error);
                throw new InvalidOperationException("Save game tables could not be created in database.", ex);
            }
        }
        #endregion //Database Setup Methods
    }
}
