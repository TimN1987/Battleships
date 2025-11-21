using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Battleships.MVVM.Factories;
using Battleships.MVVM.Services;
using Battleships.MVVM.Services.Database;
using Battleships.MVVM.View;
using Battleships.MVVM.ViewModel.Base;

namespace Battleships.MVVM.ViewModel
{
    public class ClassicRulesSetUpViewModel (IEventAggregator eventAggregator, IGameSetUpService gameSetUpService) : ViewModelBase
    {
        #region ToolTip Messages
        public static string FireUntilMissExplanation => "Players continue taking shots until they miss.";
        public static string BonusShotOnHitExplanation => "Players receive a bonus shot for each shot that hits a ship.";
        public static string HideSunkShipsExplanation => "Ships are not revealed when they are sunk.";
        #endregion //ToolTip Messages

        #region Fields
        private readonly IEventAggregator _eventAggregator = eventAggregator
            ?? throw new ArgumentNullException(nameof(eventAggregator));
        private readonly IGameSetUpService _gameSetUpService = gameSetUpService
            ?? throw new ArgumentNullException(nameof(gameSetUpService));

        private bool _fireUntilMiss = false;
        private bool _bonusShotOnHit = false;
        private bool _hideSunkShips = false;

        private ICommand? _setRulesCommand;
        #endregion //Fields

        #region Properties
        public bool FireUntilMiss
        {
            get => _fireUntilMiss;
            set => SetProperty(ref _fireUntilMiss, value);
        }
        public bool BonusShotOnHit
        {
            get => _bonusShotOnHit;
            set => SetProperty(ref _bonusShotOnHit, value);
        }
        public bool HideSunkShips
        {
            get => _hideSunkShips;
            set => SetProperty(ref _hideSunkShips, value);
        }
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
            _gameSetUpService.SetClassicRules(FireUntilMiss, BonusShotOnHit, HideSunkShips);
            _eventAggregator.GetEvent<NavigationEvent>().Publish(typeof(PlayerStartsView));
        }
    }
}
