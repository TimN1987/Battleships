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

        _captainImages = new()
        {
            { GameEvent.GameStart, [
                _staticCaptainOne
                ] }
        };
    }

    public void ProvideCaptainImage(GameEvent gameEvent)
    {
        Uri[] images = _captainImages.GetValueOrDefault(gameEvent) ?? [];
        int index = RandomProvider.Instance.Next(images.Length);

        _eventAggregator
            .GetEvent<LoadCaptainEvent>()
            .Publish(images[index]);
    }
}
