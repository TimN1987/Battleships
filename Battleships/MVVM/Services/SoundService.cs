using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using Battleships.MVVM.Enums;
using Battleships.MVVM.Utilities;
using Battleships.MVVM.ViewModel;
using Battleships.MVVM.View;

namespace Battleships.MVVM.Services;

/// <summary>
/// Interface for the SoundService. It defines the properties that the SoundService uses to load and play 
/// the correct foreground and background sounds.
/// </summary>
public interface ISoundService
{
    public Uri CurrentBackgroundMusic { get; }
    public int BackgroundMusicIndex { get; set; }
    public Uri? CurrentForegroundSound { get; set; }
}

/// <summary>
/// The SoundService is responsible for loading and playing the background music and foreground sounds. It 
/// uses EventAggregator to subscribe to events that tell it which foreground sound to play or when the 
/// background music is ended. It is then able to send the correct Uri to the <see cref="MainViewModel"/> 
/// to load to the appropriate MediaElement.
/// </summary>
public class SoundService : ISoundService
{
    #region Fields
    private readonly IEventAggregator _eventAggregator;
    private readonly Uri _defaultBackgroundMusic;
    private readonly List<Uri> _backgroundMusicList;
    private readonly int _backgroundMusicListSize;
    private int _backgroundMusicIndex;
    private Uri? _currentForegroundSound;
    private readonly Dictionary<GameEvent, Uri[]> _foregroundSounds;
    #endregion //Fields

    #region Properties
    public Uri CurrentBackgroundMusic
    {
        get
        {
            if (_backgroundMusicListSize > 0)
                return _backgroundMusicList[BackgroundMusicIndex];
            return _defaultBackgroundMusic;
        }
    }
    public int BackgroundMusicIndex
    {
        get => _backgroundMusicIndex;
        set
        {
            if (_backgroundMusicIndex != value && value >= 0 && value < _backgroundMusicListSize)
                _backgroundMusicIndex = value;
        }
    }
    public Uri? CurrentForegroundSound
    {
        get => _currentForegroundSound;
        set => _currentForegroundSound = value;
    }
    #endregion //Properties

    public SoundService(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
        _eventAggregator.GetEvent<GameEventEvent>().Subscribe(param =>
        {
            if (param is GameEvent gameEvent)
                OnGameEventReceived(gameEvent);
        });
        _eventAggregator.GetEvent<BackgroundMusicEndedEvent>().Subscribe(() => PublishNextBackgroundMusic());

        _defaultBackgroundMusic = new(@"pack://siteoforigin:,,,/MVVM/Resources/BackgroundMusic/backingone.wav", UriKind.Absolute); //update to correct music
        _backgroundMusicList = [
            new(@"pack://siteoforigin:,,,/MVVM/Resources/BackgroundMusic/backingone.wav", UriKind.Absolute),
            new(@"pack://siteoforigin:,,,/MVVM/Resources/BackgroundMusic/backingtwo.wav", UriKind.Absolute),
            new(@"pack://siteoforigin:,,,/MVVM/Resources/BackgroundMusic/backingthree.wav", UriKind.Absolute)
            ];
        _backgroundMusicListSize = _backgroundMusicList.Count;
        _backgroundMusicIndex = RandomProvider.Instance.Next(_backgroundMusicListSize);
        _foregroundSounds = new() {
            { GameEvent.GameStart, [] },
            { GameEvent.PlayerTurn, [] },
            { GameEvent.ComputerTurn, [] }
        }; //add the sounds here when stored in resources
    }

    /// <summary>
    /// Publishes the Uri for the next background music to be played in the <see cref="MainWindow"/>.
    /// </summary>
    internal void PublishNextBackgroundMusic()
    {
        if (BackgroundMusicIndex < _backgroundMusicListSize - 1)
            BackgroundMusicIndex++;
        else
            BackgroundMusicIndex = 0;
        _eventAggregator.GetEvent<NextSongEvent>().Publish(CurrentBackgroundMusic);
    }

    /// <summary>
    /// Publishes a <see cref="LoadSoundEvent"/> with the Uri for a randomly selected sound effect for the 
    /// given <see cref="GameEvent"/> to be played by a MediaElement in the <see cref="MainWindow"/>.
    /// </summary>
    internal void OnGameEventReceived(GameEvent gameEvent)
    {
        Uri[] sounds = _foregroundSounds[gameEvent];
        int availableSounds = sounds.Length;

        if (availableSounds == 0)
            return;

        int index = RandomProvider.Instance.Next(availableSounds);

        _eventAggregator
            .GetEvent<LoadSoundEvent>()
            .Publish(sounds[index]);
    }
}
