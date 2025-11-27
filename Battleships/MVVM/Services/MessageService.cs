using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battleships.MVVM.Utilities;
using Battleships.MVVM.Enums;

namespace Battleships.MVVM.Services
{
    public interface IMessageService
    {
        void RequestMessage(GameEvent message);
        void GetGameStartMessage();
        void GetGameOverMessage();
        void GetPlayerTurnMessage();
        void GetComputerTurnMessage();
        void GetShotMissedMessage();
        void GetShotHitMessage();
        void GetShipSunkMessage();
    }
    
    public class MessageService : IMessageService
    {
        #region Constant Messages
        private const string GameStartMessageOne = "Welcome to the game, sailor.";
        private const string GameStartMessageTwo = "Welcome aboard, sailor. You're just in time for the battle.";
        private const string GameStartMessageThree = "All hands on deck. Enemy approaching. Do your best, sailor.";
        #endregion //Constant Messages

        #region Message Arrays
        private string[] _gameStartMessages;
        private string[] _gameOverMessages;
        private string[] _playerTurnMessages;
        private string[] _computerTurnMessages;
        private string[] _shotMissedMessages;
        private string[] _shotHitMessages;
        private string[] _shipSunkMessages;
        #endregion //Message Arrays

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
            _gameOverMessages = [];
            _playerTurnMessages = [];
            _computerTurnMessages = [];
            _shotMissedMessages = [];
            _shotHitMessages = [];
            _shipSunkMessages = [];
        }

        #region Get Message Methods
        public void RequestMessage(GameEvent message)
        {
            switch (message)
            {
                case GameEvent.GameStart: 
                    GetGameStartMessage(); 
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
                
        }
        public void GetGameOverMessage()
        {
            int messageTotal = _gameOverMessages.Length;
            int index = RandomProvider.Instance.Next(messageTotal);

            _eventAggregator
                .GetEvent<UserMessageEvent>()
                .Publish(_gameOverMessages[index]);
        }
        public void GetPlayerTurnMessage()
        {
            int messageTotal = _playerTurnMessages.Length;
            int index = RandomProvider.Instance.Next(messageTotal);

            _eventAggregator
                .GetEvent<UserMessageEvent>()
                .Publish(_playerTurnMessages[index]);
        }
        public void GetComputerTurnMessage()
        {
            int messageTotal = _computerTurnMessages.Length;
            int index = RandomProvider.Instance.Next(messageTotal);

            _eventAggregator
                .GetEvent<UserMessageEvent>()
                .Publish(_computerTurnMessages[index]);
        }
        public void GetShotMissedMessage()
        {
            int messageTotal = _shotMissedMessages.Length;
            int index = RandomProvider.Instance.Next(messageTotal);

            _eventAggregator
                .GetEvent<UserMessageEvent>()
                .Publish(_shotMissedMessages[index]);
        }
        public void GetShotHitMessage()
        {
            int messageTotal = _shotHitMessages.Length;
            int index = RandomProvider.Instance.Next(messageTotal);

            _eventAggregator
                .GetEvent<UserMessageEvent>()
                .Publish(_shotHitMessages[index]);
        }
        public void GetShipSunkMessage()
        {
            int meesageTotal = _shipSunkMessages.Length;
            int index = RandomProvider.Instance.Next(meesageTotal);

            _eventAggregator
                .GetEvent<UserMessageEvent>()
                .Publish(_shipSunkMessages[index]);
        }
        #endregion //Get Message Methods
    }
}
