using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Battleships.MVVM.Enums;
using Battleships.MVVM.Services;
using Battleships.MVVM.View;
using Battleships.MVVM.ViewModel.Base;

namespace Battleships.MVVM.ViewModel
{
    public class GeneralRulesSetUpViewModel(IEventAggregator eventAggregator, IGameSetUpService gameSetUpService) : ViewModelBase
    {
        #region ToolTip Messages
        public static string AirstrikeExplanation => "Allows a three shot airstrike. Activated after five hits.";
        public static string BombardmentExplanation => "Allows a five shot bombardment. Activated after seven hits.";
        public static string ShipsCanTouchExplanation => "Allows ships to touch horizontally or vetically on the game board.";
        #endregion //ToolTip Messages

        #region Fields
        private readonly IEventAggregator _eventAggregator = eventAggregator
            ?? throw new ArgumentNullException(nameof(eventAggregator));
        private readonly IGameSetUpService _gameSetUpService = gameSetUpService
            ?? throw new ArgumentNullException(nameof(gameSetUpService));

        private bool _airstrikeAllowed = false;
        private bool _bombardmentAllowed = false;
        private bool _shipsCanTouch = false;

        private Uri _helpPageImage = new(@"pack://application:,,,/MVVM/Resources/Images/GeneralRulesView/generalruleshelp.jpg", UriKind.Absolute);

        private ICommand? _setRulesCommand;
        #endregion //Fields

        #region Properties
        public bool AirstrikeAllowed
        {
            get => _airstrikeAllowed;
            set => SetProperty(ref _airstrikeAllowed, value);
        }
        public bool BombardmentAllowed
        {
            get => _bombardmentAllowed;
            set => SetProperty(ref _bombardmentAllowed, value);
        }
        public bool ShipsCanTouch
        {
            get => _shipsCanTouch;
            set => SetProperty(ref _shipsCanTouch, value);
        }
        public Uri HelpPageImage => _helpPageImage;
        #endregion //Properties

        public ICommand SetRulesCommand
        {
            get
            {
                _setRulesCommand ??= new RelayCommand(param => SetRules());
                return _setRulesCommand;
            }
        }

        public void SetRules()
        {
            _gameSetUpService.SetGeneralRules(_airstrikeAllowed, _bombardmentAllowed, _shipsCanTouch);

            var view = (_gameSetUpService.GetGameType() == GameType.Classic)
                ? typeof(ClassicRulesSetUpView)
                : typeof(SalvoRulesSetUpView);

            _eventAggregator.GetEvent<NavigationEvent>().Publish(view);
        }
    }
}
