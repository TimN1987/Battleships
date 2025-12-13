using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Battleships.MVVM.Enums;
using Battleships.MVVM.Factories;
using Battleships.MVVM.Services;
using Battleships.MVVM.View;
using Battleships.MVVM.ViewModel.Base;

namespace Battleships.MVVM.ViewModel;

public class MainViewModel : ViewModelBase
{
    #region Fields
    private readonly IViewFactory _viewFactory;
    private readonly IEventAggregator _eventAggregator;
    private readonly ISoundService _soundService;
    private readonly Dictionary<ThemeNames, Uri> _themePaths;

    private UserControl _currentView;

    private bool _gameInProgress;
    private bool _moveInProgress;
    private Visibility _returnToGameAvailable;
    private Visibility _returnHomeAvailable;
    private bool _isFullScreen;
    private bool _volumeMuted;

    private double _backgroundVolume;
    private double _foregroundVolume;

    private Uri? _backgroundMusic;
    private Uri? _foregroundSound;
    private Uri? _foregroundSpeech;

    private Uri _muteMenuImage;
    private Uri _fullScreenMenuImage;
    private Uri _loadIcon;
    private Uri _saveIcon;
    private Uri _saveAsIcon;
    private Uri _returnIcon;
    private Uri _homeIcon;

    private string _saveStatusMessage;
    private ThemeNames _currentTheme;

    private string _muteAutomationPropertyName;
    private string _fullScreenAutomationPropertyName;

    private ICommand? _muteCommand;
    private ICommand? _fullScreenCommand;
    private ICommand? _saveCommand;
    private ICommand? _saveAsCommand;
    private ICommand? _loadCommand;
    private ICommand? _returnToGameCommand;
    private ICommand? _returnHomeCommand;
    private ICommand? _changeThemeCommand;
    private ICommand? _closeApplicationCommand;
    #endregion //Fields

    #region Theme Resources
    private static readonly ThemeNames[] DarkBackgroundThemes =
    [
        ThemeNames.Classic,
        ThemeNames.Dark,
        ThemeNames.Neon,
    ];

    private static readonly ThemeNames[] LightBackgroundThemes =
    [
        ThemeNames.Light,
        ThemeNames.Neutral,
    ];

    private static readonly Uri _classicThemeResourceDictionary = new(@"pack://application:,,,/MVVM/Styles/ClassicTheme.xaml", UriKind.Absolute);
    private static readonly Uri _darkThemeResourceDictionary = new(@"pack://application:,,,/MVVM/Styles/DarkTheme.xaml", UriKind.Absolute);
    private static readonly Uri _lightThemeResourceDictionary = new(@"pack://application:,,,/MVVM/Styles/LightTheme.xaml", UriKind.Absolute);
    private static readonly Uri _neonThemeResourceDictionary = new(@"pack://application:,,,/MVVM/Styles/NeonTheme.xaml", UriKind.Absolute);
    private static readonly Uri _neutralThemeResourceDictionary = new(@"pack://application:,,,/MVVM/Styles/NeutralTheme.xaml", UriKind.Absolute);

    public static Uri ClassicThemeIcon => new(@"pack://application:,,,/MVVM/Resources/Images/MainWindow/Menu/classicicon.png", UriKind.Absolute);
    public static Uri DarkThemeIcon => new(@"pack://application:,,,/MVVM/Resources/Images/MainWindow/Menu/darkicon.png", UriKind.Absolute);
    public static Uri LightThemeIcon => new(@"pack://application:,,,/MVVM/Resources/Images/MainWindow/Menu/lighticon.png", UriKind.Absolute);
    public static Uri NeonThemeIcon => new(@"pack://application:,,,/MVVM/Resources/Images/MainWindow/Menu/neonicon.png", UriKind.Absolute);
    public static Uri NeutralThemeIcon => new(@"pack://application:,,,/MVVM/Resources/Images/MainWindow/Menu/neutralicon.png", UriKind.Absolute);
    #endregion //Theme Resources

    #region Images Uris
    private static readonly Uri _fullScreenMinimisedWhite = new(@"pack://application:,,,/MVVM/Resources/Images/MainWindow/Menu/minimisewhite.png");
    private static readonly Uri FullScreenMaximisedWhite = new(@"pack://application:,,,/MVVM/Resources/Images/MainWindow/Menu/maximisewhite.png");
    private static readonly Uri FullScreenMinimisedBlack = new(@"pack://application:,,,/MVVM/Resources/Images/MainWindow/Menu/minimiseblack.png");
    private static readonly Uri FullScreenMaximisedBlack = new(@"pack://application:,,,/MVVM/Resources/Images/MainWindow/Menu/maximiseblack.png");

    private static readonly Uri UnMuteIconWhite = new(@"pack://application:,,,/MVVM/Resources/Images/MainWindow/Menu/unmutewhite.png");
    private static readonly Uri MuteIconWhite = new(@"pack://application:,,,/MVVM/Resources/Images/MainWindow/Menu/mutewhite.png");
    private static readonly Uri UnMuteIconBlack = new(@"pack://application:,,,/MVVM/Resources/Images/MainWindow/Menu/unmuteblack.png");
    private static readonly Uri MuteIconBlack = new(@"pack://application:,,,/MVVM/Resources/Images/MainWindow/Menu/muteblack.png");

    private static readonly Uri LoadIconWhite = new(@"pack://application:,,,/MVVM/Resources/Images/MainWindow/Menu/loadwhite.png");
    private static readonly Uri LoadIconBlack = new(@"pack://application:,,,/MVVM/Resources/Images/MainWindow/Menu/loadicon.png");
    private static readonly Uri SaveIconWhite = new(@"pack://application:,,,/MVVM/Resources/Images/MainWindow/Menu/savewhite.png");
    private static readonly Uri SaveIconBlack = new(@"pack://application:,,,/MVVM/Resources/Images/MainWindow/Menu/saveicon.png");
    private static readonly Uri SaveAsIconWhite = new(@"pack://application:,,,/MVVM/Resources/Images/MainWindow/Menu/saveaswhite.png");
    private static readonly Uri SaveAsIconBlack = new(@"pack://application:,,,/MVVM/Resources/Images/MainWindow/Menu/saveasicon.png");
    private static readonly Uri ReturnIconWhite = new(@"pack://application:,,,/MVVM/Resources/Images/MainWindow/Menu/returnwhite.png");
    private static readonly Uri ReturnIconBlack = new(@"pack://application:,,,/MVVM/Resources/Images/MainWindow/Menu/returnicon.png");
    private static readonly Uri HomeIconWhite = new(@"pack://application:,,,/MVVM/Resources/Images/MainWindow/Menu/homewhite.png");
    private static readonly Uri HomeIconBlack = new(@"pack://application:,,,/MVVM/Resources/Images/MainWindow/Menu/homeicon.png");
    #endregion //Image Uris

    #region Automation Resources
    private const string MuteNameMessage = "Click to mute all sounds.";
    private const string UnmuteNameMessage = "Click to unmute all sounds.";
    private const string FullScreenNameMessage = "Click to make window fullscreen.";
    private const string NotFullScreenNameMessage = "Click to exit fullscreen window.";
    #endregion //Automation Resources

    #region Properties
    public UserControl CurrentView
    {
        get => _currentView;
        set
        {
            if (_currentView != value)
                SetProperty(ref _currentView, value);
            ReturnToGameAvailable = _gameInProgress && _currentView is not PlayGameView
                ? Visibility.Visible : Visibility.Collapsed;
            ReturnHomeAvailable = _currentView is not HomeView ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public bool GameInProgress
    {
        get => _gameInProgress;
        set
        {
            SetProperty(ref _gameInProgress, value);
            ReturnToGameAvailable = _gameInProgress && _currentView is not PlayGameView
                ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public Visibility ReturnToGameAvailable
    {
        get => _returnToGameAvailable;
        set => SetProperty(ref _returnToGameAvailable, value);
    }

    public Visibility ReturnHomeAvailable
    {
        get => _returnHomeAvailable;
        set => SetProperty(ref _returnHomeAvailable, value);
    }

    public bool IsFullScreen
    {
        get => _isFullScreen;
        set
        {
            if (value != _isFullScreen)
            {
                SetProperty(ref _isFullScreen, value);

                //Update image for FullScreen button
                if (DarkBackgroundThemes.Contains(_currentTheme))
                    FullScreenMenuImage = (IsFullScreen) ? FullScreenMaximisedWhite : FullScreenMaximisedWhite;
                else
                    FullScreenMenuImage = (IsFullScreen) ? FullScreenMinimisedBlack : FullScreenMaximisedBlack;

                //Update the Automation Property Name
                FullScreenAutomationPropertyName = _isFullScreen ? NotFullScreenNameMessage : FullScreenNameMessage;

            }
        }
    }

    public bool VolumeMuted
    {
        get => _volumeMuted;
        set
        {
            if (value != _volumeMuted)
            {
                SetProperty(ref _volumeMuted, value);

                //Update image for Mute button
                if (DarkBackgroundThemes.Contains(_currentTheme))
                    MuteMenuImage = (VolumeMuted) ? UnMuteIconWhite : MuteIconWhite;
                else
                    MuteMenuImage = (VolumeMuted) ? UnMuteIconBlack : MuteIconBlack;

                //Update the Automation Property Name
                MuteAutomationPropertyName = _volumeMuted ? UnmuteNameMessage : MuteNameMessage;
            }
        }
    }

    public double BackgroundVolume
    {
        get => _backgroundVolume;
        set
        {
            if (value != BackgroundVolume && value >= 0 && value <= 1)
                SetProperty(ref _backgroundVolume, value);
        }
    }

    public double ForegroundVolume
    {
        get => _foregroundVolume;
        set
        {
            if (value != ForegroundVolume && value >= 0 && value <= 1)
                SetProperty(ref _foregroundVolume, value);
        }
    }

    public Uri? BackgroundMusic
    {
        get => _backgroundMusic;
        set => SetProperty(ref _backgroundMusic, value);
    }

    public Uri? ForegroundSound
    {
        get => _foregroundSound;
        set => SetProperty(ref _foregroundSound, value);
    }

    public Uri? ForegroundSpeech
    {
        get => _foregroundSpeech;
        set => SetProperty(ref _foregroundSpeech, value);
    }

    public Uri MuteMenuImage
    {
        get => _muteMenuImage;
        set => SetProperty(ref _muteMenuImage, value);
    }

    public Uri FullScreenMenuImage
    {
        get => _fullScreenMenuImage;
        set => SetProperty(ref _fullScreenMenuImage, value);
    }

    public Uri LoadIcon
    {
        get => _loadIcon;
        set => SetProperty(ref _loadIcon, value);
    }

    public Uri SaveIcon
    {
        get => _saveIcon;
        set => SetProperty(ref _saveIcon, value);
    }

    public Uri SaveAsIcon
    {
        get => _saveAsIcon;
        set => SetProperty(ref _saveAsIcon, value);
    }

    public Uri ReturnIcon
    {
        get => _returnIcon;
        set => SetProperty(ref _returnIcon, value);
    }

    public Uri HomeIcon
    {
        get => _homeIcon;
        set => SetProperty(ref _homeIcon, value);
    }

    public string SaveStatusMessage
    {
        get => _saveStatusMessage;
        set => SetProperty(ref _saveStatusMessage, value);
    }

    public string MuteAutomationPropertyName
    {
        get => _muteAutomationPropertyName;
        set => SetProperty(ref _muteAutomationPropertyName, value);
    }

    public string FullScreenAutomationPropertyName
    {
        get => _fullScreenAutomationPropertyName;
        set => SetProperty(ref _fullScreenAutomationPropertyName, value);
    }
    #endregion //Properties

    #region Commands
    public ICommand MuteCommand
    {
        get
        {
            _muteCommand ??= new RelayCommand(param => Mute());
            return _muteCommand;
        }
    }

    public ICommand FullScreenCommand
    {
        get
        {
            _fullScreenCommand ??= new RelayCommand(param => ChangeFullScreenSetting());
            return _fullScreenCommand;
        }
    }

    public ICommand SaveCommand
    {
        get
        {
            _saveCommand ??= new RelayCommand(param => RequestGameSaved(), param => CanUseSaveAndLoadGameFunctionality());
            return _saveCommand;
        }
    }

    public ICommand SaveAsCommand
    {
        get
        {
            _saveAsCommand ??= new RelayCommand(param => MoveToSaveAs(), param => CanUseSaveAndLoadGameFunctionality());
            return _saveAsCommand;
        }
    }

    public ICommand LoadCommand
    {
        get
        {
            _loadCommand ??= new RelayCommand(param => MoveToLoadGame(), param => CanUseSaveAndLoadGameFunctionality());
            return _loadCommand;
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

    public ICommand ReturnHomeCommand
    {
        get
        {
            _returnHomeCommand ??= new RelayCommand(param => NavigateTo(typeof(HomeView)));
            return _returnHomeCommand;
        }
    }
    public ICommand ChangeThemeCommand
    {
        get
        {
            _changeThemeCommand ??= new RelayCommand(param =>
            {
                if (param is ThemeNames requestedTheme)
                {
                    ChangeTheme(requestedTheme);
                }
            });
            return _changeThemeCommand;
        }
    }

    public ICommand CloseApplicationCommand
    {
        get
        {
            _closeApplicationCommand ??= new RelayCommand(param => CloseApplication());
            return _closeApplicationCommand;
        }
    }
    #endregion //Commands

    public MainViewModel(IViewFactory viewFactory, IEventAggregator eventAggregator, ISoundService soundService)
    {
        _viewFactory = viewFactory;
        _currentView = _viewFactory.CreateView(typeof(HomeView));

        _eventAggregator = eventAggregator;
        _eventAggregator.GetEvent<NavigationEvent>().Subscribe(param => NavigateTo(param));
        _eventAggregator.GetEvent<ThemeRequestEvent>().Subscribe(ShareUpdatedTheme);
        _eventAggregator.GetEvent<KeyDownFullScreenEvent>().Subscribe(() => OnEscapeKeyDown());
        _eventAggregator.GetEvent<LoadSoundEvent>().Subscribe(param =>
        {
            if (param is Uri uriToLoad)
                ForegroundSound = uriToLoad;
        });
        _eventAggregator.GetEvent<LoadSpeechEvent>().Subscribe(param =>
        {
            if (param is Uri uriToLoad)
                ForegroundSpeech = uriToLoad;
        });
        _eventAggregator.GetEvent<NextSongEvent>().Subscribe(param =>
        {
            if (param is Uri uriToLoad)
                BackgroundMusic = uriToLoad;
        });
        _eventAggregator.GetEvent<StartGameEvent>().Subscribe(UpdateGameStarted);
        _eventAggregator.GetEvent<GameOverEvent>().Subscribe(UpdateGameCompleted);
        _eventAggregator.GetEvent<MoveStatusEvent>().Subscribe(param => UpdateMoveStatus(param));
        _eventAggregator.GetEvent<RequestGameStatusEvent>().Subscribe(ShareGameStatus);
        _eventAggregator.GetEvent<SaveStatusEvent>().Subscribe(param => OnSaveStatusEventReceived(param.ToString()));

        _soundService = soundService;

        _saveStatusMessage = string.Empty;
        _themePaths = [];

        _muteAutomationPropertyName = UnmuteNameMessage;
        _fullScreenAutomationPropertyName = NotFullScreenNameMessage;

        _gameInProgress = false;
        _moveInProgress = false;
        _returnToGameAvailable = Visibility.Collapsed;
        _returnHomeAvailable = Visibility.Collapsed;

        _isFullScreen = true;
        _volumeMuted = false;

        _backgroundVolume = 0.5;
        _foregroundVolume = 0.5;

        _fullScreenMenuImage = _fullScreenMinimisedWhite;
        _muteMenuImage = MuteIconWhite;
        _loadIcon = LoadIconWhite;
        _saveIcon = SaveIconWhite;
        _saveAsIcon = SaveAsIconWhite;
        _returnIcon = ReturnIconWhite;
        _homeIcon = HomeIconWhite;

        _currentTheme = ThemeNames.Classic;
        _themePaths.Add(ThemeNames.Classic, _classicThemeResourceDictionary);
        _themePaths.Add(ThemeNames.Dark, _darkThemeResourceDictionary);
        _themePaths.Add(ThemeNames.Light, _lightThemeResourceDictionary);
        _themePaths.Add(ThemeNames.Neon, _neonThemeResourceDictionary);
        _themePaths.Add(ThemeNames.Neutral, _neutralThemeResourceDictionary);
    }

    #region Methods
    //Methods for handling sound and display
    private void Mute() => VolumeMuted = !VolumeMuted;

    private void ChangeFullScreenSetting()
    {
        _eventAggregator.GetEvent<FullScreenEvent>().Publish(IsFullScreen);

        IsFullScreen = !IsFullScreen;
    }

    private void OnEscapeKeyDown() => IsFullScreen = false;

    private void ReturnToGame() => NavigateTo(typeof(PlayGameView));

    private void ChangeTheme(ThemeNames requestedTheme)
    {
        _currentTheme = requestedTheme;

        if (Application.Current.Resources.MergedDictionaries.Count > 2)
            Application.Current.Resources.MergedDictionaries.RemoveAt(2);

        var newTheme = new ResourceDictionary { Source = _themePaths[requestedTheme] };
        Application.Current.Resources.MergedDictionaries.Add(newTheme);

        if (DarkBackgroundThemes.Contains(requestedTheme))
        {
            MuteMenuImage = (VolumeMuted) ? UnMuteIconWhite : MuteIconWhite;
            FullScreenMenuImage = (IsFullScreen) ? _fullScreenMinimisedWhite : FullScreenMaximisedWhite;
            LoadIcon = LoadIconWhite;
            SaveIcon = SaveIconWhite;
            SaveAsIcon = SaveAsIconWhite;
            ReturnIcon = ReturnIconWhite;
            HomeIcon = HomeIconWhite;
        }
        else
        {
            MuteMenuImage = (VolumeMuted) ? UnMuteIconBlack : MuteIconBlack;
            FullScreenMenuImage = (IsFullScreen) ? FullScreenMinimisedBlack : FullScreenMaximisedBlack;
            LoadIcon = LoadIconBlack;
            SaveIcon = SaveIconBlack;
            SaveAsIcon = SaveAsIconBlack;
            ReturnIcon = ReturnIconBlack;
            HomeIcon = HomeIconBlack;
        }

        ShareUpdatedTheme();
    }

    /// <summary>
    /// Shares the current theme using <see cref="EventAggregator"/> if the theme is updated or a <see 
    /// cref="ThemeRequestEvent"/> is received.
    /// </summary>
    private void ShareUpdatedTheme()
    {
        _eventAggregator.GetEvent<ThemeUpdateEvent>().Publish(_currentTheme);
    }

    //Methods for saving and loading games using menu items
    private void RequestGameSaved()
    {
        _eventAggregator.GetEvent<SaveEvent>().Publish();
    }

    private bool CanUseSaveAndLoadGameFunctionality() => !_moveInProgress;

    private void RequestAutosave() => _eventAggregator.GetEvent<AutosaveEvent>().Publish();

    private void MoveToLoadGame()
    {
        if (CurrentView is PlayGameView)
            RequestAutosave();
        NavigateTo(typeof(LoadGameView));
    }

    private void MoveToSaveAs()
    {
        if (CurrentView is PlayGameView)
            RequestAutosave();
        NavigateTo(typeof(SaveGameView));
    }

    public void OnSaveStatusEventReceived(string saveStatusMessage)
    {
        SaveStatusMessage = saveStatusMessage;
    }

    public void CloseApplication()
    {
        Application.Current.Shutdown();
    }

    //Methods for navigation
    private void NavigateTo(Type newView)
    {
        if (newView is null || !typeof(UserControl).IsAssignableFrom(newView))
        {
            throw new ArgumentException($"The type '{newView}' is not a valid UserControl.");
        }

        CurrentView = (UserControl)_viewFactory.CreateView(newView);
    }


    //Methods for handling start and end of a game
    private void UpdateGameStarted()
    {
        GameInProgress = true;
    }

    private void UpdateGameCompleted()
    {
        GameInProgress = false;
    }

    private void ShareGameStatus()
    {
        _eventAggregator.GetEvent<UpdateGameStatusEvent>().Publish(_gameInProgress);
    }

    private void UpdateMoveStatus(bool isInProgress)
    {
        _moveInProgress = isInProgress;
    }
    #endregion //Methods
}
