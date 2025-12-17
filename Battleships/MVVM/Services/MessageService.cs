using Battleships.MVVM.Enums;
using Battleships.MVVM.Utilities;

namespace Battleships.MVVM.Services;

public interface IMessageService
{
    void RequestMessage(GameEvent message);
    void GetGameStartMessage();
    void GetGameLoadedMessage();
    void GetPlayerTurnMessage();
    void GetComputerTurnMessage();
    void GetPlayerSunkShipMessage();
    void GetComputerSunkShipMessage();
    void GetPlayerHitShipMessage();
    void GetComputerHitShipMessage();
    void GetPlayerMissedMessage();
    void GetComputerMissedMessage();
}

public class MessageService : IMessageService
{
    // Constant Messages
    private const string GameStartMessageOne = "Welcome to the battle, sailor.";
    private const string GameStartMessageTwo = "Welcome aboard, sailor. You're just in time for the battle.";
    private const string GameStartMessageThree = "All hands on deck. Enemy approaching. Do your best, sailor.";

    private const string GameLoadedMessageOne = "Welcome back sailor. Time to finish the battle.";
    private const string GameLoadedMessageTwo = "Back to work sailor. It's time to end this!";
    private const string GameLoadedMessageThree = "Good to have you back, sailor. Let's get to work.";

    private const string PlayerTurnMessageOne = "It's your turn, sailor. Fire away!";
    private const string PlayerTurnMessageTwo = "Man the guns! Time to shoot!";
    private const string PlayerTurnMessageThree = "Get ready, sailor. Time to attack!";

    private const string ComputerTurnMessageOne = "Brace yourself. Enemy attack incoming!";
    private const string ComputerTurnMessageTwo = "Your turn's over. Prepare for an enemy attack";
    private const string ComputerTurnMessageThree = "Attack incoming! Look out, sailor!";

    private const string PlayerSunkShipMessageOne = "You've sunk an enemy ship! Well done, sailor.";
    private const string PlayerSunkShipMessageTwo = "Direct hit! Another enemy ship has been sunk.";
    private const string PlayerSunkShipMessageThree = "Excellent shooting, sailor! You've sunk an enemy ship.";

    private const string ComputerSunkShipMessageOne = "An enemy has sunk one of your ships. Stay strong, sailor.";
    private const string ComputerSunkShipMessageTwo = "Your ship has been hit and sunk. Don't lose hope, sailor.";
    private const string ComputerSunkShipMessageThree = "Tough luck, sailor. An enemy has sunk one of your ships.";

    private const string PlayerHitShipMessageOne = "Direct hit on the enemy ship! Keep it up, sailor.";
    private const string PlayerHitShipMessageTwo = "Nice shot! You've hit an enemy ship.";
    private const string PlayerHitShipMessageThree = "Good aim, sailor! You've struck an enemy ship.";

    private const string ComputerHitShipMessageOne = "The enemy has hit one of your ships. Stay alert, sailor.";
    private const string ComputerHitShipMessageTwo = "Your ship has been hit by the enemy. Keep your wits about you, sailor.";
    private const string ComputerHitShipMessageThree = "Watch out, sailor! The enemy has struck one of your ships.";

    private const string PlayerMissedMessageOne = "You missed the enemy ship. Try again, sailor.";
    private const string PlayerMissedMessageTwo = "No hit this time, sailor. Adjust your aim and fire again.";
    private const string PlayerMissedMessageThree = "The enemy ship remains unscathed. Take another shot, sailor.";

    private const string ComputerMissedMessageOne = "The enemy missed your ship. Stay focused, sailor.";
    private const string ComputerMissedMessageTwo = "Your ship remains safe for now. Keep your guard up, sailor.";
    private const string ComputerMissedMessageThree = "The enemy's shot went wide. Don't let your guard down, sailor.";


    // Audio Uris
    private readonly Uri _gameStartsAudioOne = new(@"pack://siteoforigin:,,,/MVVM/Resources/Speech/gamestartaudioone.wav", UriKind.Absolute);
    private readonly Uri _gameStartsAudioTwo = new(@"pack://siteoforigin:,,,/MVVM/Resources/Speech/gamestartaudiotwo.wav", UriKind.Absolute);
    private readonly Uri _gameStartsAudioThree = new(@"pack://siteoforigin:,,,/MVVM/Resources/Speech/gamestartaudiothree.wav", UriKind.Absolute);

    private readonly Uri _gameLoadedAudioOne = new(@"pack://siteoforigin:,,,/MVVM/Resources/Speech/gameloadedaudioone.wav", UriKind.Absolute);
    private readonly Uri _gameLoadedAudioTwo = new(@"pack://siteoforigin:,,,/MVVM/Resources/Speech/gameloadedaudiotwo.wav", UriKind.Absolute);
    private readonly Uri _gameLoadedAudioThree = new(@"pack://siteoforigin:,,,/MVVM/Resources/Speech/gameloadedaudiothree.wav", UriKind.Absolute);

    private readonly Uri _playerTurnAudioOne = new(@"pack://siteoforigin:,,,/MVVM/Resources/Speech/playerturnaudioone.wav", UriKind.Absolute);
    private readonly Uri _playerTurnAudioTwo = new(@"pack://siteoforigin:,,,/MVVM/Resources/Speech/playerturnaudiotwo.wav", UriKind.Absolute);
    private readonly Uri _playerTurnAudioThree = new(@"pack://siteoforigin:,,,/MVVM/Resources/Speech/playerturnaudiothree.wav", UriKind.Absolute);

    private readonly Uri _computerTurnAudioOne = new(@"pack://siteoforigin:,,,/MVVM/Resources/Speech/computerturnaudioone.wav", UriKind.Absolute);
    private readonly Uri _computerTurnAudioTwo = new(@"pack://siteoforigin:,,,/MVVM/Resources/Speech/computerturnaudiotwo.wav", UriKind.Absolute);
    private readonly Uri _computerTurnAudioThree = new(@"pack://siteoforigin:,,,/MVVM/Resources/Speech/computerturnaudiothree.wav", UriKind.Absolute);

    private readonly Uri _playerSunkShipAudioOne = new(@"pack://siteoforigin:,,,/MVVM/Resources/Speech/computerturnaudioone.wav", UriKind.Absolute);
    private readonly Uri _playerSunkShipAudioTwo = new(@"pack://siteoforigin:,,,/MVVM/Resources/Speech/computerturnaudioone.wav", UriKind.Absolute);
    private readonly Uri _playerSunkShipAudioThree = new(@"pack://siteoforigin:,,,/MVVM/Resources/Speech/computerturnaudioone.wav", UriKind.Absolute);

    private readonly Uri _computerSunkShipAudioOne = new(@"pack://siteoforigin:,,,/MVVM/Resources/Speech/computerturnaudioone.wav", UriKind.Absolute);
    private readonly Uri _computerSunkShipAudioTwo = new(@"pack://siteoforigin:,,,/MVVM/Resources/Speech/computerturnaudioone.wav", UriKind.Absolute);
    private readonly Uri _computerSunkShipAudioThree = new(@"pack://siteoforigin:,,,/MVVM/Resources/Speech/computerturnaudioone.wav", UriKind.Absolute);

    private readonly Uri _playerHitShipAudioOne = new(@"pack://siteoforigin:,,,/MVVM/Resources/Speech/computerturnaudioone.wav", UriKind.Absolute);
    private readonly Uri _playerHitShipAudioTwo = new(@"pack://siteoforigin:,,,/MVVM/Resources/Speech/computerturnaudioone.wav", UriKind.Absolute);
    private readonly Uri _playerHitShipAudioThree = new(@"pack://siteoforigin:,,,/MVVM/Resources/Speech/computerturnaudioone.wav", UriKind.Absolute);

    private readonly Uri _computerHitShipAudioOne = new(@"pack://siteoforigin:,,,/MVVM/Resources/Speech/computerturnaudioone.wav", UriKind.Absolute);
    private readonly Uri _computerHitShipAudioTwo = new(@"pack://siteoforigin:,,,/MVVM/Resources/Speech/computerturnaudioone.wav", UriKind.Absolute);
    private readonly Uri _computerHitShipAudioThree = new(@"pack://siteoforigin:,,,/MVVM/Resources/Speech/computerturnaudioone.wav", UriKind.Absolute);

    private readonly Uri _playerMissedAudioOne = new(@"pack://siteoforigin:,,,/MVVM/Resources/Speech/computerturnaudioone.wav", UriKind.Absolute);
    private readonly Uri _playerMissedAudioTwo = new(@"pack://siteoforigin:,,,/MVVM/Resources/Speech/computerturnaudioone.wav", UriKind.Absolute);
    private readonly Uri _playerMissedAudioThree = new(@"pack://siteoforigin:,,,/MVVM/Resources/Speech/computerturnaudioone.wav", UriKind.Absolute);

    private readonly Uri _computerMissedAudioOne = new(@"pack://siteoforigin:,,,/MVVM/Resources/Speech/computerturnaudioone.wav", UriKind.Absolute);
    private readonly Uri _computerMissedAudioTwo = new(@"pack://siteoforigin:,,,/MVVM/Resources/Speech/computerturnaudioone.wav", UriKind.Absolute);
    private readonly Uri _computerMissedAudioThree = new(@"pack://siteoforigin:,,,/MVVM/Resources/Speech/computerturnaudioone.wav", UriKind.Absolute);

    // Message Arrays
    private readonly string[] _gameStartMessages;
    private readonly string[] _gameLoadedMessages;
    private readonly string[] _playerTurnMessages;
    private readonly string[] _computerTurnMessages;
    private readonly string[] _playerSunkShipMessages;
    private readonly string[] _computerSunkShipMessages;
    private readonly string[] _playerHitShipMessages;
    private readonly string[] _computerHitShipMessages;
    private readonly string[] _playerMissedMessages;
    private readonly string[] _computerMissedMessages;

    // Audio Arrays
    private readonly Uri[] _gameStartAudio;
    private readonly Uri[] _gameLoadedAudio;
    private readonly Uri[] _playerTurnAudio;
    private readonly Uri[] _computerTurnAudio;
    private readonly Uri[] _playerSunkShipAudio;
    private readonly Uri[] _computerSunkShipAudio;
    private readonly Uri[] _playerHitShipAudio;
    private readonly Uri[] _computerHitShipAudio;
    private readonly Uri[] _playerMissedAudio;
    private readonly Uri[] _computerMissedAudio;

    // Fields
    private readonly IEventAggregator _eventAggregator;

    public MessageService(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator
            ?? throw new ArgumentNullException(nameof(eventAggregator));

        _eventAggregator
            .GetEvent<GameEventEvent>()
            .Subscribe(param => RequestMessage(param));

        _gameStartMessages = [
            GameStartMessageOne,
            GameStartMessageTwo,
            GameStartMessageThree
            ];
        _gameLoadedMessages = [
            GameLoadedMessageOne,
            GameLoadedMessageTwo,
            GameLoadedMessageThree
            ];
        _playerTurnMessages = [
            PlayerTurnMessageOne,
            PlayerTurnMessageTwo,
            PlayerTurnMessageThree
            ];
        _computerTurnMessages = [
            ComputerTurnMessageOne,
            ComputerTurnMessageTwo,
            ComputerTurnMessageThree
            ];
        _playerSunkShipMessages = [
            PlayerSunkShipMessageOne,
            PlayerSunkShipMessageTwo,
            PlayerSunkShipMessageThree
            ];
        _computerSunkShipMessages = [
            ComputerSunkShipMessageOne,
            ComputerSunkShipMessageTwo,
            ComputerSunkShipMessageThree
            ];
        _playerHitShipMessages = [
            PlayerHitShipMessageOne,
            PlayerHitShipMessageTwo,
            PlayerHitShipMessageThree
            ];
        _computerHitShipMessages = [
            ComputerHitShipMessageOne,
            ComputerHitShipMessageTwo,
            ComputerHitShipMessageThree
            ];
        _playerMissedMessages = [
            PlayerMissedMessageOne,
            PlayerMissedMessageTwo,
            PlayerMissedMessageThree
            ];
        _computerMissedMessages = [
            ComputerMissedMessageOne,
            ComputerMissedMessageTwo,
            ComputerMissedMessageThree
            ];

        _gameStartAudio = [
            _gameStartsAudioOne,
            _gameStartsAudioTwo,
            _gameStartsAudioThree
            ];
        _gameLoadedAudio = [
            _gameLoadedAudioOne,
            _gameLoadedAudioTwo,
            _gameLoadedAudioThree
            ];
        _playerTurnAudio = [
            _playerTurnAudioOne,
            _playerTurnAudioTwo,
            _playerTurnAudioThree
            ];
        _computerTurnAudio = [
            _computerTurnAudioOne,
            _computerTurnAudioTwo,
            _computerTurnAudioThree
            ];
        _playerSunkShipAudio = [
            _playerSunkShipAudioOne,
            _playerSunkShipAudioTwo,
            _playerSunkShipAudioThree
            ];
        _computerSunkShipAudio = [
            _computerSunkShipAudioOne,
            _computerSunkShipAudioTwo,
            _computerSunkShipAudioThree
            ];
        _playerHitShipAudio = [
            _playerHitShipAudioOne,
            _playerHitShipAudioTwo,
            _playerHitShipAudioThree
            ];
        _computerHitShipAudio = [
            _computerHitShipAudioOne,
            _computerHitShipAudioTwo,
            _computerHitShipAudioThree
            ];
        _playerMissedAudio = [
            _playerMissedAudioOne,
            _playerMissedAudioTwo,
            _playerMissedAudioThree
            ];
        _computerMissedAudio = [
            _computerMissedAudioOne,
            _computerMissedAudioTwo,
            _computerMissedAudioThree
            ];
    }

    // Get Message Methods
    public void RequestMessage(GameEvent message)
    {
        switch (message)
        {
            case GameEvent.GameStart:
                GetGameStartMessage();
                break;
            case GameEvent.GameLoaded:
                GetGameLoadedMessage();
                break;
            case GameEvent.PlayerTurn:
                GetPlayerTurnMessage();
                break;
            case GameEvent.ComputerTurn:
                GetComputerTurnMessage();
                break;
            case GameEvent.PlayerSunkShip:
                GetPlayerSunkShipMessage();
                break;
            case GameEvent.ComputerSunkShip:
                GetComputerSunkShipMessage();
                break;
            case GameEvent.PlayerHitShip:
                GetPlayerHitShipMessage();
                break;
            case GameEvent.ComputerHitShip:
                GetComputerHitShipMessage();
                break;
            case GameEvent.PlayerMissed:
                GetPlayerMissedMessage();
                break;
            case GameEvent.ComputerMissed:
                GetComputerMissedMessage();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(message));
        }
    }

    public void GetGameStartMessage()
    {
        int messageTotal = _gameStartMessages.Length;
        int index = RandomProvider.Instance.Next(messageTotal);

        _eventAggregator
            .GetEvent<UserMessageEvent>()
            .Publish(_gameStartMessages[index]);
        _eventAggregator
            .GetEvent<LoadSpeechEvent>()
            .Publish(_gameStartAudio[index]);
    }
    public void GetGameLoadedMessage()
    {
        int messageTotal = _gameStartMessages.Length;
        int index = RandomProvider.Instance.Next(messageTotal);

        _eventAggregator
            .GetEvent<UserMessageEvent>()
            .Publish(_gameLoadedMessages[index]);
        _eventAggregator
            .GetEvent<LoadSpeechEvent>()
            .Publish(_gameLoadedAudio[index]);
    }
    public void GetPlayerTurnMessage()
    {
        int messageTotal = _playerTurnMessages.Length;
        int index = RandomProvider.Instance.Next(messageTotal);

        _eventAggregator
            .GetEvent<UserMessageEvent>()
            .Publish(_playerTurnMessages[index]);
        _eventAggregator
            .GetEvent<LoadSpeechEvent>()
            .Publish(_playerTurnAudio[index]);
    }
    public void GetComputerTurnMessage()
    {
        int messageTotal = _computerTurnMessages.Length;
        int index = RandomProvider.Instance.Next(messageTotal);

        _eventAggregator
            .GetEvent<UserMessageEvent>()
            .Publish(_computerTurnMessages[index]);
        _eventAggregator
            .GetEvent<LoadSpeechEvent>()
            .Publish(_computerTurnAudio[index]);
    }
    public void GetPlayerSunkShipMessage()
    {
        int messageTotal = _playerSunkShipMessages.Length;
        int index = RandomProvider.Instance.Next(messageTotal);
        _eventAggregator
            .GetEvent<UserMessageEvent>()
            .Publish(_playerSunkShipMessages[index]);
        _eventAggregator
            .GetEvent<LoadSpeechEvent>()
            .Publish(_playerSunkShipAudio[index]);
    }
    public void GetComputerSunkShipMessage()
    {
        int messageTotal = _computerSunkShipMessages.Length;
        int index = RandomProvider.Instance.Next(messageTotal);
        _eventAggregator
            .GetEvent<UserMessageEvent>()
            .Publish(_computerSunkShipMessages[index]);
        _eventAggregator
            .GetEvent<LoadSpeechEvent>()
            .Publish(_computerSunkShipAudio[index]);
    }
    public void GetPlayerHitShipMessage()
    {
        int messageTotal = _playerHitShipMessages.Length;
        int index = RandomProvider.Instance.Next(messageTotal);
        _eventAggregator
            .GetEvent<UserMessageEvent>()
            .Publish(_playerHitShipMessages[index]);
        _eventAggregator
            .GetEvent<LoadSpeechEvent>()
            .Publish(_playerHitShipAudio[index]);
    }
    public void GetComputerHitShipMessage()
    {
        int messageTotal = _computerHitShipMessages.Length;
        int index = RandomProvider.Instance.Next(messageTotal);
        _eventAggregator
            .GetEvent<UserMessageEvent>()
            .Publish(_computerHitShipMessages[index]);
        _eventAggregator
            .GetEvent<LoadSpeechEvent>()
            .Publish(_computerHitShipAudio[index]);
    }
    public void GetPlayerMissedMessage()
    {
        int messageTotal = _playerMissedMessages.Length;
        int index = RandomProvider.Instance.Next(messageTotal);
        _eventAggregator
            .GetEvent<UserMessageEvent>()
            .Publish(_playerMissedMessages[index]);
        _eventAggregator
            .GetEvent<LoadSpeechEvent>()
            .Publish(_playerMissedAudio[index]);
    }
    public void GetComputerMissedMessage()
    {
        int messageTotal = _computerMissedMessages.Length;
        int index = RandomProvider.Instance.Next(messageTotal);
        _eventAggregator
            .GetEvent<UserMessageEvent>()
            .Publish(_computerMissedMessages[index]);
        _eventAggregator
            .GetEvent<LoadSpeechEvent>()
            .Publish(_computerMissedAudio[index]);
    }
}
