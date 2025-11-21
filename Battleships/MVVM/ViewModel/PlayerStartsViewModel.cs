using Battleships.MVVM.Services;
using Battleships.MVVM.Utilities;
using Battleships.MVVM.View;
using Battleships.MVVM.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleships.MVVM.ViewModel;

public class PlayerStartsViewModel(IEventAggregator eventAggregator, IGameSetUpService gameSetUpService) : ViewModelBase
{
    //Fields

    private readonly IEventAggregator _eventAggregator = eventAggregator;
    private readonly IGameSetUpService _gameSetUpService = gameSetUpService;

    // Command backing fields

    private RelayCommand? _playerStartsCommand;
    private RelayCommand? _computerStartsCommand;
    private RelayCommand? _randomStartsCommand;

    // Properties

    public static string PlayerStartsViewTitle => "Choose starting player";
    public static string PlayerStartsButtonText => "Player";
    public static string ComputerStartsButtonText => "Computer";
    public static string RandomStartsButtonText => "Random";

    // Commands

    public RelayCommand? PlayerStartsCommand => _playerStartsCommand 
        ??= new RelayCommand(param => SetStartingPlayer(false));

    public RelayCommand? ComputerStartsCommand => _computerStartsCommand 
        ??=new RelayCommand(param => SetStartingPlayer(false, false));

    public RelayCommand? RandomStartsCommand => _randomStartsCommand 
        ??= new RelayCommand(param => SetStartingPlayer());

    // Methods

    /// <summary>
    /// Updates the game setup service with the selected starting player and navigates to the 
    /// ship placement view to continue game set up.
    /// </summary>
    private void SetStartingPlayer(bool random = true, bool playerStarts = true)
    {
        if (random)
        {
            playerStarts = RandomProvider.Instance.Next(0, 2) == 0;
        }

        _gameSetUpService.SetPlayerStarts(playerStarts);
        _eventAggregator.GetEvent<NavigationEvent>().Publish(typeof(ShipPlacementView));
    }
}
