using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Markup;
using Battleships.MVVM.Enums;
using Battleships.MVVM.Services;
using Battleships.MVVM.View;
using Battleships.MVVM.ViewModel.Base;

namespace Battleships.MVVM.ViewModel
{
    public class SalvoRulesSetUpViewModel(IEventAggregator eventAggregator, IGameSetUpService gameSetUpService) : ViewModelBase
    {
        #region Tooltip Messages
        public static string ShotsFixedExplanation => "Five shots every turn.";
        public static string ShotsEqualUnsunkExplanation => "The number of shots each turn equals the number of unsunk player ships remaining.";
        public static string ShotsEqualUndamagedExplanation => "The number of shots each turn equals the number of undamaged player ships remaining. It cannot go below one.";
        public static string ShotsEqualLargestUnsunkExplanation => "The number of shots each turn equals the size of the largest unsunk player ship.";
        public static string ShotsEqualLargestUndamagedExplanation => "The number of shots each turn equals the size of the largest undamaged player ship. It cannot go below one.";
        #endregion //Tooltip Messages

        #region Fields
        private readonly IEventAggregator _eventAggregator = eventAggregator 
            ?? throw new ArgumentNullException(nameof(eventAggregator));
        private readonly IGameSetUpService _gameSetUpService = gameSetUpService 
            ?? throw new ArgumentNullException(nameof(gameSetUpService));

        private bool _shotsFixed = true;
        private bool _shotsEqualUnsunkShips = false;
        private bool _shotsEqualUndamagedShips = false;
        private bool _shotsEqualLargestUnsunkShip = false;
        private bool _shotsEqualLargestUndamagedShip = false;

        private Uri _helpPageImage = new(@"pack://application:,,,/MVVM/Resources/Images/SalvoRulesView/salvoruleshelp.jpg", UriKind.Absolute);

        private SalvoShots _selectedSalvoShotType = SalvoShots.Fixed;

        private ICommand? _setRulesCommand;
        #endregion //Fields

        #region Properties
        

        public bool ShotsFixed
        {
            get => _shotsFixed;
            set
            {
                SetProperty(ref _shotsFixed, value);

                if (value == true)
                {
                    _selectedSalvoShotType = SalvoShots.Fixed;
                    
                    ShotsEqualUnsunkShips = false;
                    ShotsEqualUndamagedShips = false;
                    ShotsEqualLargestUnsunkShip = false;
                    ShotsEqualLargestUndamagedShip = false;
                }
            }
        }
        public bool ShotsEqualUnsunkShips
        {
            get => _shotsEqualUnsunkShips;
            set
            {
                SetProperty(ref _shotsEqualUnsunkShips, value);

                if (value == true)
                {
                    _selectedSalvoShotType = SalvoShots.EqualsUnsunkShips;
                    
                    ShotsFixed = false;
                    ShotsEqualUndamagedShips = false;
                    ShotsEqualLargestUnsunkShip = false;
                    ShotsEqualLargestUndamagedShip = false;
                }
            }
        }
        public bool ShotsEqualUndamagedShips
        {
            get => _shotsEqualUndamagedShips;
            set
            {
                SetProperty(ref _shotsEqualUndamagedShips, value);

                if (value == true)
                {
                    _selectedSalvoShotType = SalvoShots.EqualsUndamagedShips;
                    
                    ShotsFixed = false;
                    ShotsEqualUnsunkShips = false;
                    ShotsEqualLargestUnsunkShip = false;
                    ShotsEqualLargestUndamagedShip = false;
                }
            }
        }
        public bool ShotsEqualLargestUnsunkShip
        {
            get => _shotsEqualLargestUnsunkShip;
            set
            {
                SetProperty(ref _shotsEqualLargestUnsunkShip, value);

                if (value == true)
                {
                    _selectedSalvoShotType = SalvoShots.EqualsLargestUnsunkShip;
                    
                    ShotsFixed = false;
                    ShotsEqualUnsunkShips = false;
                    ShotsEqualUndamagedShips = false;
                    ShotsEqualLargestUndamagedShip = false;
                }
            }
        }
        public bool ShotsEqualLargestUndamagedShip
        {
            get => _shotsEqualLargestUndamagedShip;

            set
            {
                SetProperty(ref _shotsEqualLargestUndamagedShip, value);

                if (value == true)
                {
                    _selectedSalvoShotType = SalvoShots.EqualsLargestUndamagedShip;
                    
                    ShotsFixed = false;
                    ShotsEqualUnsunkShips = false;
                    ShotsEqualUndamagedShips = false;
                    ShotsEqualLargestUnsunkShip = false;
                }
            }
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
            _gameSetUpService.SetSalvoRules(_selectedSalvoShotType);
            _eventAggregator.GetEvent<NavigationEvent>().Publish(typeof(PlayerStartsView));
        }
    }
}
