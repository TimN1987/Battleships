using System.Windows.Controls;
using System.Windows.Input;
using Battleships.MVVM.Factories;
using Battleships.MVVM.Services;
using Battleships.MVVM.Services.Database;
using Battleships.MVVM.View;
using Battleships.MVVM.ViewModel.Base;

namespace Battleships.MVVM.ViewModel
{
    public class HomeViewModel : ViewModelBase
    {
        #region Fields
        private readonly IEventAggregator _eventAggregator;
        private readonly IGameRepository _gameRepository;
        private readonly IEventLogger _eventLogger;

        private bool _autosaveFileExists;

        private ICommand _newGameCommand;
        private ICommand _continueGameCommand;
        private ICommand _loadGameCommand;
        #endregion //Fields

        #region Properties
        public bool AutosaveFileExists
        {
            get => _autosaveFileExists;
            set => SetProperty(ref _autosaveFileExists, value);
        }
        #endregion //Properties

        #region Commands
        public ICommand NewGameCommand
        {
            get
            {
                _newGameCommand ??= new RelayCommand(param => NavigateTo<GameTypeView>());
                return _newGameCommand;
            }
        }

        public ICommand ContinueGameCommand
        {
            get
            {
                _continueGameCommand ??= new RelayCommand(param => LoadAutosaveGame(), param => CanLoadAutosaveGame());
                return _continueGameCommand;
            }
        }

        public ICommand LoadGameCommand
        {
            get
            {
                _loadGameCommand ??= new RelayCommand(param => NavigateTo<LoadGameView>());
                return _loadGameCommand;
            }
        }
        #endregion //Commands

        public HomeViewModel(IEventAggregator eventAggregator, IGameRepository gameRepository, ILoggerFactory loggerFactory)
        {
            _eventAggregator = eventAggregator;
            _gameRepository = gameRepository;
            _eventLogger = loggerFactory.CreateLogger(nameof(HomeViewModel));

            _newGameCommand = new RelayCommand(param => NavigateTo<GameTypeView>());
            _continueGameCommand = new RelayCommand(param => LoadAutosaveGame(), param => CanLoadAutosaveGame());
            _loadGameCommand = new RelayCommand(param => NavigateTo<LoadGameView>());

            try
            {
                Task.Run(async () => await CheckForAutosave());
            }
            catch (Exception ex)
            {
                _eventLogger.LogCritical($"Error checking for autosave file: {ex.Message}", ex);
            }
        }

        #region Methods
        /// <summary>
        /// Checks if an autosave file exists using the game repository. Updates the booelan _autosaveFileExists 
        /// which determines whether or not the Continue Game button is enabled (allowing the user to continue 
        /// the autosave game).
        /// </summary>
        private async Task CheckForAutosave()
        {
            AutosaveFileExists = await _gameRepository.CheckForAutosaveFile();
        }

        /// <summary>
        /// Publishes a navigation event using the event aggregator to navigate to the requested view. Either 
        /// to begin setting up a new game or to load a saved game.
        /// </summary>
        private void NavigateTo<TView>() where TView : UserControl
        {
            _eventAggregator.GetEvent<NavigationEvent>().Publish(typeof(TView));
        }

        /// <summary>
        /// Publishes a navigation event using the event aggregator to navigate to the PlayGameView and an 
        /// AutosaveEvent to indicate that the autosave game should be loaded by the PlayGameView.
        /// </summary>
        private void LoadAutosaveGame()
        {
            _eventAggregator.GetEvent<NavigationEvent>().Publish(typeof(PlayGameView));
            _eventAggregator.GetEvent<LoadAutosaveEvent>().Publish();
        }

        /// <summary>
        /// Allows the <see cref="LoadAutosaveGame"> method to be called by the ContinueGameCommand only if the 
        /// autosave file exists. This is used as a fallback to ensure that the command can only be executed 
        /// if there is a game to load.
        /// </summary>
        /// <returns></returns>
        private bool CanLoadAutosaveGame()
        {
            return AutosaveFileExists;
        }
        #endregion //Methods

    }
}
