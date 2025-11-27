using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using Battleships.MVVM.Services;
using Battleships.MVVM.ViewModel.Base;
using Battleships.MVVM.Structs;
using System.Windows.Input;
using System.Windows.Controls;
using Battleships.MVVM.Model.DataTransferObjects;
using System.Diagnostics.CodeAnalysis;
using Battleships.MVVM.View;
using System.Windows.Documents;
using System.Windows;

namespace Battleships.MVVM.ViewModel;

public class LoadGameViewModel : ViewModelBase
{
    #region Fields
    private readonly IEventAggregator _eventAggregator;
    private readonly ISaveService _saveService;
    private SaveGame _selectedGame;
    private SaveGame[] _saveGames;
    private bool _gameInProgress;
    private ICommand? _loadGameCommand;
    private ICommand? _returnHomeCommand;
    private ICommand? _returnToGameCommand;
    #endregion //Fields

    #region Properties
    public SaveGame SelectedGame
    {
        get => _selectedGame;
        set => SetProperty(ref _selectedGame, value);
    }
    public SaveGame[] SaveGames
    {
        get => _saveGames;
        set => SetProperty(ref _saveGames, value);
    }
    public bool GameInProgress
    {
        get => _gameInProgress;
        set => SetProperty(ref _gameInProgress, value);
    }
    #endregion //Properties

    #region Commands
    public ICommand LoadGameCommand
    {
        get
        {
            _loadGameCommand ??= new RelayCommand(param => LoadGame(), param => CanLoadGame());
            return _loadGameCommand;
        }
    }
    public ICommand ReturnHomeCommand
    {
        get
        {
            _returnHomeCommand ??= new RelayCommand(param => ReturnHome());
            return _returnHomeCommand;
        }
    }
    public ICommand ReturnToGameCommand
    {
        get
        {
            _returnToGameCommand ??= new RelayCommand(param => ReturnToGame());
            return _returnToGameCommand;
        }
    }
    #endregion //Commands

    public LoadGameViewModel(IEventAggregator eventAggregator, ISaveService saveService)
    {
        _eventAggregator = eventAggregator 
            ?? throw new ArgumentNullException(nameof(eventAggregator));
        _saveService = saveService
            ?? throw new ArgumentNullException(nameof(saveService));

        _eventAggregator.GetEvent<UpdateGameStatusEvent>().Subscribe(param => UpdateGameStatus(param));
        _eventAggregator.GetEvent<RequestGameStatusEvent>().Publish();

        _selectedGame = new SaveGame(0);
        _saveGames = [.. Enumerable.Range(0, 11)
            .Select(number => new SaveGame(number))];

        SetUpSaveLists();
    }

    #region Methods
    private void SetUpSaveLists()
    {
        _ = PopulateSaveGamesList();
    }

    /// <summary>
    /// Retrieves the autosave and other save game files from the database and returns these as a list. This 
    /// is used to populate the save games list to be displayed to the user.
    /// </summary>
    private async Task PopulateSaveGamesList()
    {
        List<(string gameName, DateTime saveTime, int saveSlot)> saveGames =
            await _saveService.GetSaveGamesList(true);

        var updatedSaveGames = _saveGames.ToArray();

        foreach (var game in saveGames)
            updatedSaveGames[game.saveSlot] = new SaveGame(game.gameName, game.saveTime, game.saveSlot);

        SaveGames = updatedSaveGames.ToArray();

        if (_gameInProgress && _saveService.CurrentGame != null)
            SelectedGame = SaveGames
                .Where(game => game.SaveSlot == _saveService.CurrentSaveSlot)
                .FirstOrDefault();
    }

    /// <summary>
    /// Used when the Load Game button is clicked to load the <see cref="PlayGameViewModel"/> and send a 
    /// request to load the selected game.
    /// </summary>
    /// <remarks>The <see cref="PlayGameViewModel"/> is requested first as it may not be loaded yet. This 
    /// gives it time to subscribe to the loading events in its constructor.</remarks>
    private void LoadGame()
    {
        _eventAggregator.GetEvent<NavigationEvent>().Publish(typeof(PlayGameView));

        if (SelectedGame.GameName == "Autosave")
            _eventAggregator.GetEvent<LoadAutosaveEvent>().Publish();
        else
            _eventAggregator.GetEvent<LoadGameEvent>().Publish((SelectedGame.GameName, SelectedGame.SaveSlot));
    }

    /// <summary>
    /// Checks that a valid selection has been made before allowing the LoadGame method to be called.
    /// </summary>
    /// <returns>True if a valid selection has been made, false if not.</returns>
    private bool CanLoadGame()
    {
        return SelectedGame.GameName != "Empty";
    }

    private void ReturnHome()
    {
        _eventAggregator.GetEvent<NavigationEvent>().Publish(typeof(HomeView));
    }

    private void ReturnToGame()
    {
        _eventAggregator.GetEvent<NavigationEvent>().Publish(typeof(PlayGameView));
    }

    private void UpdateGameStatus(bool gameStatus)
    {
        GameInProgress = gameStatus;
    }
    #endregion //Methods
}
