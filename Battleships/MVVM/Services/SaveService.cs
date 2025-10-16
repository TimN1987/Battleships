using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battleships.MVVM.Factories;
using Battleships.MVVM.Services.Database;
using Battleships.MVVM.Model;
using Prism.Events;
using Battleships.MVVM.View;
using Battleships.MVVM.Enums;
using System.Diagnostics;
using Battleships.MVVM.Model.DataTransferObjects;

namespace Battleships.MVVM.Services
{
    /// <summary>
    /// Defines a contract for saving and loading game data. It provides methods to save the game, load the 
    /// game and generate a list of save games. The methods are asynchronous and return a Task to allow for 
    /// greater flexibility and responsiveness in the application.
    /// </summary>
    public interface ISaveService
    {
        public string? CurrentGameName { get; set; }
        public int? CurrentSaveSlot { get; set; }
        public Game? CurrentGame { get; set; }
        public Task<bool> SaveGame(bool autosaveGame = true, bool newSaveGame = false);
        public Task<bool> LoadGame(bool autosaveGame = true);
        public Task<List<(string, DateTime, int)>> GetSaveGamesList(bool includeAutosave = false);
    }

    /// <summary>
    /// A class that implements the <see cref="ISaveService"/> interface and provides functionality for 
    /// saving and loading game data. It uses the <see cref="IEventAggregator"/> to publish events to the 
    /// main view model and the <see cref="IEventLogger"/> to log events. It also uses the 
    /// <see cref="IGameRepository"/> to interact with the database and the <see cref="IEncryptionService"/> 
    /// to encrypt and decrypt game data. It works as an access point for the data storage within the application. 
    /// The class is stateless and does not modify shared data, making it thread-safe. It can be used 
    /// concurrently across multiple threads.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="eventAggregator"/>, 
    /// <paramref name="gameRepository"/> or <paramref name="encryptionService"/> parameters are null.</exception>
    /// <exception cref="NullReferenceException">Thrown if the loggerFactory parameter is null when it calls 
    /// the CreateLogger method.</exception>
    public class SaveService(IEventAggregator eventAggregator, ILoggerFactory loggerFactory, IGameRepository gameRepository, IEncryptionService encryptionService) : ISaveService
    {
        #region Fields
        private readonly IEventAggregator _eventAggregator = eventAggregator
                ?? throw new ArgumentNullException(nameof(eventAggregator), "Event aggregator cannot be null.");
        private readonly IEventLogger _eventLogger = loggerFactory.CreateLogger(nameof(SaveService))
                ?? throw new ArgumentNullException(nameof(_eventLogger), "Logger factory returned null.");
        private readonly IGameRepository _gameRepository = gameRepository
                ?? throw new ArgumentNullException(nameof(gameRepository), "Game repository cannot be null.");
        private readonly IEncryptionService _encryptionService = encryptionService
                ?? throw new ArgumentNullException(nameof(encryptionService), "Encryption service cannot be null.");

        private const int SaveStatusMessageDisplayTime = 2000;
        #endregion //Fields

        #region Properties
        public string? CurrentGameName { get; set; } //Set when a new game is first saved
        public int? CurrentSaveSlot { get; set; }
        public Game? CurrentGame { get; set; }
        public bool AutosaveGameExists { get; set; }
        #endregion //Properties

        #region Save Methods
        /// <summary>
        /// Saves the game to the database. If <paramref name="autosaveGame"/> is true, it will save the 
        /// autosave game to the database. If <paramref name="autosaveGame"/> is false, it will save the 
        /// current game with the name <see cref="CurrentGameName"/> to the database. The calling class will 
        /// need to update the <see cref="CurrentGame"/> property before calling this method to ensure that 
        /// the correct game is saved. If the <see cref="CurrentGameName"> property is null, the SaveGameView 
        /// will be loaded to enable the user to choose a game name. A null return indicates that the game 
        /// could not be saved. The method uses the <see cref="IEncryptionService"/> to encrypt the game data 
        /// and the <see cref="IGameRepository"/> to save the game data to the database. It also uses the 
        /// <see cref="IEventAggregator"/> to publish events to the main view model for display on the menu bar.
        /// </summary>
        /// <param name="autosaveGame">A boolean value indicating whether or not the autosave game is being 
        /// saved. </param>
        /// <returns>A boolean value indicating whether or not the game was successfully saved.</returns>
        /// <remarks>The CurrentGame property must be updated before calling this method.</remarks>
        public async Task<bool> SaveGame(bool autosaveGame = true, bool newSaveGame = false)
        {
            if (CurrentGameName is null && !autosaveGame)
            {
                _eventAggregator.GetEvent<NavigationEvent>().Publish(typeof(SaveGameView));
                await UpdateSaveStatus("No save game found. Create a new save file...");
                return true;
            }

            await UpdateSaveStatus("Saving game...");
            
            if (CurrentGame is null)
            {
                _eventLogger.LogWarning("No game data exists to save.");

                await UpdateSaveStatus("Save failed");
                return false;
            }

            try
            {
                var encryptedGame = await _encryptionService.EncryptGameData(CurrentGame)
                        ?? throw new InvalidOperationException($"Game data could not be encrypted. Save not possible.");

                if (autosaveGame)
                {
                    if (AutosaveGameExists)
                    {
                        if (await _gameRepository.UpdateSaveGameRecord(encryptedGame))
                        {
                            await UpdateSaveStatus("Game saved");
                            return true;
                        }
                        else
                        {
                            _eventLogger.LogWarning("Game could not be autosaved.");
                            await UpdateSaveStatus("Save failed");
                            return false;
                        }
                    }
                    else
                    {
                        AutosaveGameExists = await _gameRepository.CreateSaveGameRecord(encryptedGame);

                        if (AutosaveGameExists)
                        {
                            await UpdateSaveStatus("Game saved");
                            return true;
                        }
                        else
                        {
                            _eventLogger.LogWarning("Game could not be autosaved.");

                            await UpdateSaveStatus("Save failed");
                            return false;
                        }
                    }
                }
                else
                {
                    if (newSaveGame)
                    {
                        if (await _gameRepository.CreateSaveGameRecord(encryptedGame, CurrentGameName, SaveGameTable.SaveGames, CurrentSaveSlot))
                        {
                            await UpdateSaveStatus($"Game saved as {CurrentGameName}");
                            return true;
                        }
                        else
                        {
                            _eventLogger.LogWarning("Game could not be saved.");
                            await UpdateSaveStatus("Save failed");
                            return false;
                        }
                    }
                    else
                    {
                        if (await _gameRepository.UpdateSaveGameRecord(encryptedGame, CurrentGameName, SaveGameTable.SaveGames, CurrentSaveSlot))
                        {
                            await UpdateSaveStatus("Game saved");
                            return true;
                        }
                        else
                        {
                            _eventLogger.LogWarning("Game could not be saved.");
                            await UpdateSaveStatus("Save failed");
                            return false;
                        }
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                _eventLogger.LogCritical($"Game data could not be encrypted. Save not possible: {ex.Message}", ex);
                await UpdateSaveStatus("Save failed");
                return false;
            }

            catch (Exception ex)
            {

                _eventLogger.LogCritical($"Error saving game: {ex.Message}", ex);
                await UpdateSaveStatus("Save failed");
                return false;
            }
            finally
            {
                ClearCurrentGame();
            }
        }
        #endregion //Save Methods

        #region Load Methods
        /// <summary>
        /// Loads the game from the database. If <paramref name="autosaveGame"/> is true, it will load the 
        /// Autosave game from the database. If <paramref name="autosaveGame"/> is false, it will load the 
        /// data with Name attribute CurrentGameName from the database. The calling class will need to 
        /// update the CurrentName property before calling this method to ensure that the correct game is 
        /// loaded. A null return indicates that the game could not be loaded.
        /// </summary>
        /// <param name="autosaveGame">A boolean value indicating whether or not the autosave game should 
        /// be loaded. False indicates that the CurrentGameName should be used to load data.</param>
        /// <returns>A Game object containing the data for requested game. A null return indicates that no 
        /// game data could be loaded.</returns>
        public async Task<bool> LoadGame(bool autosaveGame = true)
        {
            await UpdateSaveStatus("Game loading...");
            
            var gameName = (autosaveGame) ? null : CurrentGameName;
            SaveGameTable? saveGameTable = (autosaveGame) ? null : SaveGameTable.SaveGames;

            try
            {
                var encryptedGame = await _gameRepository.ReadSaveGameData(gameName, saveGameTable)
                    ?? throw new InvalidOperationException($"No save game found. Load not possible.");

                ArgumentException.ThrowIfNullOrWhiteSpace(encryptedGame, nameof(encryptedGame));

                var gameDTO = await _encryptionService.DecryptGameData(encryptedGame)
                    ?? throw new InvalidOperationException($"Game data could not be decrypted. Load not possible.");

                await UpdateSaveStatus("Game loaded");
                _eventAggregator.GetEvent<GameLoadedEvent>().Publish(gameDTO);
                return true;
            }
            catch (InvalidOperationException ex)
            {
                _eventLogger.LogWarning($"No save game found. Load not possible: {ex.Message}");
                await UpdateSaveStatus("Load failed");
                return false;
            }
            catch (ArgumentException ex)
            {
                _eventLogger.LogCritical($"Game data could not be decrypted. Load not possible: {ex.Message}", ex);
                await UpdateSaveStatus("Load failed");
                return false;
            }
            catch (Exception ex)
            {
                _eventLogger.LogCritical($"Error loading game: {ex.Message}", ex);
                await UpdateSaveStatus("Load failed");
                return false;
            }
        }

        /// <summary>
        /// Generates a list of save games from the database. The list is a list of tuples containing the name 
        /// of the save game and the date it was saved.
        /// </summary>
        /// <param name="includeAutosave">A boolean value indicating whether or not the autosave game 
        /// should be included in the list.</param>
        /// <returns>A list of tuples containing the name of each save game with its associated save time.</returns>
        public async Task<List<(string, DateTime, int)>> GetSaveGamesList(bool includeAutosave = false)
        {
            return await _gameRepository.GenerateSaveGameList(includeAutosave);
        }
        #endregion //Load Methods

        #region Save Status Helper
        private async Task UpdateSaveStatus(string saveStatusMessage)
        {
            try
            {
                _eventAggregator.GetEvent<SaveStatusEvent>().Publish(saveStatusMessage);

                await Task.Delay(SaveStatusMessageDisplayTime);
                
                _eventAggregator.GetEvent<SaveStatusEvent>().Publish(string.Empty);
            }
            catch (Exception ex)
            {
                _eventLogger.LogCritical($"Error updating save status: {ex.Message}", ex);
            }
        }
        #endregion //Save Status Helper
        /// <summary>
        /// Cleans up the game data after the game has been saved. This method is called to ensure that no 
        /// game data is stored in the class for longer than it is needed.
        /// </summary>
        public void ClearCurrentGame()
        {
            CurrentGame = null;
        }

        /// <summary>
        /// Cleans up the game data after a game has been completed. It ensures that no game data is stored 
        /// in the class for longer than it is needed. It also resets the <see cref="_autosaveCreated"/> and 
        /// <see cref="_currentGameName"/> properties ready for a new game.
        /// </summary>
        public void EndGameCleanUp()
        {
            CurrentGame = null;
            CurrentGameName = null;
            AutosaveGameExists = false;
        }
    }
}
