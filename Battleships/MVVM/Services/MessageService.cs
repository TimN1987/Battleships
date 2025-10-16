using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battleships.MVVM.Utilities;

namespace Battleships.MVVM.Services
{
    public interface IMessageService
    {
        string GetGameStartMessage();
        string GetGameOverMessage();
        string GetPlayerTurnMessage();
        string GetComputerTurnMessage();
        string GetShotMissedMessage();
        string GetShotHitMessage();
        string GetShipSunkMessage();
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

        public MessageService()
        {
            _gameStartMessages = [ GameStartMessageOne, GameStartMessageTwo, GameStartMessageThree ];
            _gameOverMessages = [];
            _playerTurnMessages = [];
            _computerTurnMessages = [];
            _shotMissedMessages = [];
            _shotHitMessages = [];
            _shipSunkMessages = [];
        }

        #region Get Message Methods
        public string GetGameStartMessage()
        {
            int messageTotal = _gameStartMessages.Length;
            int index = RandomProvider.Instance.Next(messageTotal);

            return _gameStartMessages[index];
        }
        public string GetGameOverMessage()
        {
            int messageTotal = _gameOverMessages.Length;
            int index = RandomProvider.Instance.Next(messageTotal);

            return _gameOverMessages[index];
        }
        public string GetPlayerTurnMessage()
        {
            int messageTotal = _playerTurnMessages.Length;
            int index = RandomProvider.Instance.Next(messageTotal);

            return _playerTurnMessages[index];
        }
        public string GetComputerTurnMessage()
        {
            int messageTotal = _computerTurnMessages.Length;
            int index = RandomProvider.Instance.Next(messageTotal);

            return _computerTurnMessages[index];
        }
        public string GetShotMissedMessage()
        {
            int messageTotal = _shotMissedMessages.Length;
            int index = RandomProvider.Instance.Next(messageTotal);

            return _shotMissedMessages[index];
        }
        public string GetShotHitMessage()
        {
            int messageTotal = _shotHitMessages.Length;
            int index = RandomProvider.Instance.Next(messageTotal);

            return _shotHitMessages[index];
        }
        public string GetShipSunkMessage()
        {
            int meesageTotal = _shipSunkMessages.Length;
            int index = RandomProvider.Instance.Next(meesageTotal);

            return _shipSunkMessages[index];
        }
        #endregion //Get Message Methods
    }
}
