using System.Windows.Input;
using Battleships.MVVM.Enums;
using Battleships.MVVM.Services;
using Battleships.MVVM.View;
using Battleships.MVVM.ViewModel.Base;

namespace Battleships.MVVM.ViewModel
{
    /// <summary>
    /// This class is responsible for handling the game type selection in the game setup process. It inherits 
    /// from ViewModelBase to implement the INotifyPropertyChanged interface.
    /// </summary>
    /// <param name="eventAggregator">The IEventAggregator Singleton injected by the ServiceProvider.</param>
    /// <param name="gameSetUpService">The IGameSetUpService Singleton injected by the ServiceProvider.</param>
    public class GameTypeViewModel(IEventAggregator eventAggregator, IGameSetUpService gameSetUpService) : ViewModelBase
    {
        #region Fields
        private readonly IEventAggregator _eventAggregator = eventAggregator
            ?? throw new ArgumentNullException(nameof(eventAggregator));
        private readonly IGameSetUpService _gameSetUpService = gameSetUpService
            ?? throw new ArgumentNullException(nameof(gameSetUpService));

        private ICommand? _selectGameTypeCommand;
        #endregion //Fields

        #region Commands
        public ICommand SelectGameTypeCommand
        {
            get
            {
                _selectGameTypeCommand ??= new RelayCommand(param =>
                {
                    if (param is GameType gameType)
                        SelectGameType(gameType);
                });
                return _selectGameTypeCommand;
            }
        }
        #endregion //Commands

        #region Methods
        /// <summary>
        /// Calls the SetGameType method from the <see cref="GameSetUpService"/> and navigates to 
        /// <see cref="DifficultyView"/> to continue setup.
        /// </summary>
        /// <param name="gameType"></param>
        internal void SelectGameType(GameType gameType)
        {
            _gameSetUpService.SetGameType(gameType);
            _eventAggregator.GetEvent<NavigationEvent>().Publish(typeof(DifficultyView));
        }
        #endregion //Methods
    }
}
