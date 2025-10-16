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

namespace Battleships.MVVM.Services
{
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
        private readonly Dictionary<ForegroundSoundType, Uri> _foregroundSounds;
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
            _eventAggregator.GetEvent<ForegroundSoundEvent>().Subscribe(param =>
            {
                if (param is ForegroundSoundType foregroundSound)
                    OnForegroundSoundReceived(foregroundSound);
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
            _foregroundSounds = []; //add the sounds here when stored in resources
        }

        /// <summary>
        /// Increments the background music index to the next song in the list. If the index is at the end of 
        /// the list, it resets to 0. This is used when the background music ends to load the next song.
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
        /// Loads the foreground sound based on the type of sound that is passed in. The sound is loaded from 
        /// the <see cref="_foregroundSounds"/> dictionary and sent as a <see cref="LoadSoundEvent"/>./>
        /// </summary>
        /// <param name="soundToPlay"></param>
        internal void OnForegroundSoundReceived(ForegroundSoundType soundToPlay)
        {
            var uriToLoad = _foregroundSounds[soundToPlay];
            _eventAggregator.GetEvent<LoadSoundEvent>().Publish(uriToLoad);
        }
    }
}
