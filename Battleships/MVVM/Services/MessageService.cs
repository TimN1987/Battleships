using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battleships.MVVM.Utilities;
using Battleships.MVVM.Enums;

namespace Battleships.MVVM.Services;

public interface IMessageService
{
    void RequestMessage(GameEvent message);
    void GetGameStartMessage();
    void GetGameLoadedMessage();
    void GetGameOverMessage();
    void GetPlayerTurnMessage();
    void GetComputerTurnMessage();
    void GetShotMissedMessage();
    void GetShotHitMessage();
    void GetShipSunkMessage();
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

    // Message Arrays
    private string[] _gameStartMessages;
    private string[] _gameLoadedMessages;
    private string[] _gameOverMessages;
    private string[] _playerTurnMessages;
    private string[] _computerTurnMessages;
    private string[] _shotMissedMessages;
    private string[] _shotHitMessages;
    private string[] _shipSunkMessages;

    // Audio Arrays
    private Uri[] _gameStartAudio;
    private Uri[] _gameLoadedAudio;
    private Uri[] _gameOverAudio;
    private Uri[] _playerTurnAudio;
    private Uri[] _computerTurnAudio;
    private Uri[] _shotMissedAudio;
    private Uri[] _shotHitAudio;
    private Uri[] _shipSunkAudio;


    // Fields
    private IEventAggregator _eventAggregator;

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
        _gameOverMessages = [];
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
        _shotMissedMessages = [];
        _shotHitMessages = [];
        _shipSunkMessages = [];

        _gameStartAudio = [
            _gameStartsAudioOne,
            _gameStartsAudioTwo,
            _gameStartsAudioThree
            ];
        _gameLoadedAudio = [
            _gameStartsAudioOne,
            _gameStartsAudioTwo,
            _gameStartsAudioThree
            ];
        _gameOverAudio = [];
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
        _shotMissedAudio = [];
        _shotHitAudio = [];
        _shipSunkAudio = [];
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
    public void GetGameOverMessage()
    {
        int messageTotal = _gameOverMessages.Length;
        int index = RandomProvider.Instance.Next(messageTotal);

        _eventAggregator
            .GetEvent<UserMessageEvent>()
            .Publish(_gameOverMessages[index]);
        _eventAggregator
            .GetEvent<LoadSpeechEvent>()
            .Publish(_gameOverAudio[index]);
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
    public void GetShotMissedMessage()
    {
        int messageTotal = _shotMissedMessages.Length;
        int index = RandomProvider.Instance.Next(messageTotal);

        _eventAggregator
            .GetEvent<UserMessageEvent>()
            .Publish(_shotMissedMessages[index]);
        _eventAggregator
            .GetEvent<LoadSpeechEvent>()
            .Publish(_shotMissedAudio[index]);
    }
    public void GetShotHitMessage()
    {
        int messageTotal = _shotHitMessages.Length;
        int index = RandomProvider.Instance.Next(messageTotal);

        _eventAggregator
            .GetEvent<UserMessageEvent>()
            .Publish(_shotHitMessages[index]);
        _eventAggregator
            .GetEvent<LoadSpeechEvent>()
            .Publish(_shotHitAudio[index]);
    }
    public void GetShipSunkMessage()
    {
        int meesageTotal = _shipSunkMessages.Length;
        int index = RandomProvider.Instance.Next(meesageTotal);

        _eventAggregator
            .GetEvent<UserMessageEvent>()
            .Publish(_shipSunkMessages[index]);
        _eventAggregator
            .GetEvent<LoadSpeechEvent>()
            .Publish(_shipSunkAudio[index]);
    }
}
