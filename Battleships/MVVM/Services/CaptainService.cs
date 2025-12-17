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

    private readonly Uri _talkingCaptainOne = new(@"pack://application:,,,/MVVM/Resources/Images/Captains/captaintalkingone.gif", UriKind.Absolute);

    // Fields
    private IEventAggregator _eventAggregator;
    private readonly Dictionary<GameEvent, Uri[]> _captainImages;
    private readonly Uri[] _talkingImages;
    private readonly Uri[] _playerSunkImages;
    private readonly Uri[] _computerSunkImages;
    private readonly Uri[] _playerHitImages;
    private readonly Uri[] _computerHitImages;
    private readonly Uri[] _playerMissImages;
    private readonly Uri[] _computerMissImages;

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
            _talkingCaptainOne
            ];
        _playerSunkImages = [];
        _computerSunkImages = [];
        _playerHitImages = [];
        _computerHitImages = [];
        _playerMissImages = [];
        _computerMissImages = [];

        _captainImages = new()
        {
            { GameEvent.GameStart, _talkingImages },
            { GameEvent.PlayerTurn, _talkingImages },
            { GameEvent.ComputerTurn, _talkingImages },
            { GameEvent.PlayerSunkShip, _playerSunkImages },
            { GameEvent.ComputerSunkShip, _computerSunkImages },
            { GameEvent.PlayerHitShip, _playerHitImages },
            { GameEvent.ComputerHitShip, _computerHitImages },
            { GameEvent.PlayerMissed, _playerMissImages },
            { GameEvent.ComputerMissed, _computerMissImages }
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
