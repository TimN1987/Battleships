using System.Windows.Input;
using Battleships.MVVM.Enums;
using Battleships.MVVM.Services;
using Battleships.MVVM.View;
using Battleships.MVVM.ViewModel.Base;

namespace Battleships.MVVM.ViewModel
{
    public class DifficultyViewModel(IEventAggregator eventAggregator, IGameSetUpService gameSetUpService) : ViewModelBase
    {
        private readonly IEventAggregator _eventAggregator = eventAggregator
            ?? throw new ArgumentNullException(nameof(eventAggregator));
        private readonly IGameSetUpService _gameSetUpService = gameSetUpService
            ?? throw new ArgumentNullException(nameof(gameSetUpService));

        private ICommand? _selectDifficultyCommand;

        public ICommand SelectDifficultyCommand
        {
            get
            {
                _selectDifficultyCommand ??= new RelayCommand(param =>
                {
                    if (param is GameDifficulty difficulty)
                        SelectDifficulty(difficulty);
                });
                return _selectDifficultyCommand;
            }
        }

        private void SelectDifficulty(GameDifficulty difficulty)
        {
            _gameSetUpService.SetDifficulty(difficulty);
            _eventAggregator.GetEvent<NavigationEvent>().Publish(typeof(GeneralRulesSetUpView));
        }
    }
}