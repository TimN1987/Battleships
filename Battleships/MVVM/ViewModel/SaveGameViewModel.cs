using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Battleships.MVVM.Services;
using Battleships.MVVM.Structs;
using Battleships.MVVM.View;
using Battleships.MVVM.ViewModel.Base;
using Prism.Events;

namespace Battleships.MVVM.ViewModel;

public partial class SaveGameViewModel : ViewModelBase
{
    #region Fields
    private readonly IEventAggregator _eventAggregator;
    private readonly ISaveService _saveService;
    private SaveGame _selectedGame;
    private SaveGame[] _saveGames;
    private string _saveName;
    private ICommand? _saveGameCommand;
    private ICommand? _returnToGameCommand;
    #endregion //Fields

    public SaveGame SelectedGame
    {
        get => _selectedGame;
        set
        {
            SetProperty(ref _selectedGame, value);
            if (value.GameName != "Empty")
                SaveName = value.GameName;
        }
    }
    public SaveGame[] SaveGames
    {
        get => _saveGames;
        set => SetProperty(ref _saveGames, value);
    }
    public string SaveName
    {
        get => _saveName;
        set
        {
            if (ValidateSaveName(value))
                SetProperty(ref _saveName, value);
        }
    }

    public ICommand SaveGameCommand
    {
        get
        {
            _saveGameCommand ??= new RelayCommand(param => SaveGame(), param => CanSaveGame());
            return _saveGameCommand;
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

    public SaveGameViewModel(IEventAggregator eventAggregator, ISaveService saveService)
    {
        _eventAggregator = eventAggregator
            ?? throw new ArgumentNullException(nameof(eventAggregator));
        _saveService = saveService
            ?? throw new ArgumentNullException(nameof(saveService));

        _selectedGame = new SaveGame(0);
        _saveGames = [.. Enumerable.Range(1, 10)
            .Select(number => new SaveGame(number))];
        _saveName = string.Empty;

        SetUpSaveList();
    }

    #region Methods
    private void SetUpSaveList()
    {
        _ = PopulateSaveGamesList();
    }

    private static bool ValidateSaveName(string name)
    {
        return SaveNameRegex().IsMatch(name);
    }

    /// <summary>
    /// Retrieves the autosave and other save game files from the database and returns these as a list. This 
    /// is used to populate the save games list to be displayed to the user.
    /// </summary>
    private async Task PopulateSaveGamesList()
    {
        List<(string gameName, DateTime saveTime, int saveSlot)> saveGames =
            await _saveService.GetSaveGamesList(false);

        var updatedSaveGames = _saveGames.ToArray();

        foreach (var game in saveGames)
            updatedSaveGames[game.saveSlot - 1] = new SaveGame(game.gameName, game.saveTime, game.saveSlot);

        SaveGames = updatedSaveGames.ToArray();

        if (_saveService.CurrentGame != null)
            SelectedGame = SaveGames
                .Where(game => game.SaveSlot == _saveService.CurrentSaveSlot)
                .FirstOrDefault();
    }

    private void SaveGame()
    {
        if (SelectedGame.GameName == "Autosave")
            _eventAggregator.GetEvent<AutosaveEvent>().Publish();
        else
            _eventAggregator.GetEvent<SaveAsEvent>().Publish((SaveName, SelectedGame.SaveSlot));

        _eventAggregator.GetEvent<NavigationEvent>().Publish(typeof(PlayGameView));
    }

    /// <summary>
    /// Checks that a valid selection has been made before allowing the LoadGame method to be called.
    /// </summary>
    /// <returns>True if a valid selection has been made, false if not.</returns>
    private bool CanSaveGame()
    {
        if (string.IsNullOrWhiteSpace(SaveName))
            return false;
        if (SaveName.Length < 3)
            return false;
        if (SaveName == "Empty")
            return false;
        return Regex.IsMatch(SaveName, @"^[A-Za-z0-9]{3,20}$");
    }

    private void ReturnToGame()
    {
        _eventAggregator.GetEvent<NavigationEvent>().Publish(typeof(PlayGameView));
    }

    [GeneratedRegex(@"^[A-Za-z0-9]{0,20}$")]
    private static partial Regex SaveNameRegex();
    #endregion //Methods
}
