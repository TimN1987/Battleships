using Battleships.MVVM.Enums;
using Battleships.MVVM.Utilities;

namespace Battleships.MVVM.Services;

public interface ICaptainService
{
    void ProvideCaptainImage(GameEvent gameEvent);
}

public class CaptainService : ICaptainService
{
    // Images
    private readonly Uri _staticCaptainOne = new(@"pack://application:,,,/MVVM/Resources/Images/Captains/staticcaptainone.png", UriKind.Absolute);

    // Fields
    private IEventAggregator _eventAggregator;
    private readonly Dictionary<GameEvent, Uri[]> _captainImages;
    private readonly Uri[] _talkingImages;
    private readonly Uri[] _computerHitImages;
    private readonly Uri[] _playerHitImages;
    private readonly Uri[] _computerMissImages;
    private readonly Uri[] _playerMissImages;

    public CaptainService(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator 
            ?? throw new ArgumentNullException(nameof(eventAggregator));

        _eventAggregator
            .GetEvent<GameEventEvent>()
            .Subscribe(param =>
            {
                if (param is GameEvent gameEvent)
                    ProvideCaptainImage(gameEvent);
            });

        _talkingImages = [
            new(@"pack://application:,,,/MVVM/Resources/Images/Captains/captaintalkingone.gif", UriKind.Absolute)
            ];
        _computerHitImages = [];
        _playerHitImages = [];
        _computerMissImages = [];
        _playerMissImages = [];

        _captainImages = new()
        {
            { GameEvent.GameStart, _talkingImages },
            { GameEvent.PlayerTurn, _talkingImages },
            { GameEvent.ComputerTurn, _talkingImages }
        };
    }

    public void ProvideCaptainImage(GameEvent gameEvent)
    {
        Uri[] images = _captainImages.GetValueOrDefault(gameEvent) ?? [];
        int index = RandomProvider.Instance.Next(images.Length);

        if (images.Length == 0)
            return;

        _eventAggregator
            .GetEvent<LoadCaptainEvent>()
            .Publish(images[index]);
    }
}
