namespace Battleships.MVVM.Utilities;

public static class GameOverProvider
{
    // Player wins messages
    const string PlayerWinMessageOne = "Congratulations! You've sunk all enemy ships and won the battle!";
    const string PlayerWinMessageTwo = "Victory is yours! The enemy fleet has been defeated!";
    const string PlayerWinMessageThree = "Well done, Captain! You've outmaneuvered the enemy and secured a win!";
    const string PlayerWinMessageFour = "You've emerged victorious! The seas are yours!";
    const string PlayerWinMessageFive = "Fantastic job! You've successfully destroyed the enemy fleet!";

    // Computer wins messages
    const string ComputerWinMessageOne = "Defeat! The enemy fleet has sunk all your ships!";
    const string ComputerWinMessageTwo = "The enemy has won! Your fleet has been destroyed!";
    const string ComputerWinMessageThree = "Alas, Captain! The enemy has outmaneuvered you and claimed victory!";
    const string ComputerWinMessageFour = "The seas belong to the enemy now! Your ships have been sunk!";
    const string ComputerWinMessageFive = "The enemy fleet has triumphed! Better luck next time!";

    // Player wins gifs

    // Computer wins gifs


    // Static fields
    private static bool _playerWins = false;
    private static int _messageIndex = 0;
    private static string[] _playerWinsMessages = [
        PlayerWinMessageOne,
        PlayerWinMessageTwo,
        PlayerWinMessageThree,
        PlayerWinMessageFour,
        PlayerWinMessageFive
    ];
    private static string[] _computerWinsMessages = [
        ComputerWinMessageOne,
        ComputerWinMessageTwo,
        ComputerWinMessageThree,
        ComputerWinMessageFour,
        ComputerWinMessageFive
    ];
    private static int _gifIndex = 0;
    private static Uri[] _playerWinsGifs = [];
    private static Uri[] _computerWinsGifs = [];

    // Methods

    /// <summary>
    /// Returns a game over message based on whether the player wins or the computer wins. Used to update the 
    /// UI with a relevant message at the end of the game.
    /// </summary>
    public static string GetGameOverMessage(bool playerWins)
    {
        _playerWins = playerWins;
        _messageIndex = RandomProvider.Instance.Next(
            0,
            _playerWins ? _playerWinsMessages.Length : _computerWinsMessages.Length
        );
        return _playerWins ? _playerWinsMessages[_messageIndex] : _computerWinsMessages[_messageIndex];
    }

    /// <summary>
    /// Returns a game over gif based on whether the player wins or the computer wins. Used to update the UI 
    /// with a relevant gif at the end of the game.
    /// </summary>
    public static Uri GetGameOverGif(bool playerWins)
    {
        _playerWins = playerWins;
        _gifIndex = RandomProvider.Instance.Next(
            0,
            _playerWins ? _playerWinsGifs.Length : _computerWinsGifs.Length
        );
        return _playerWins ? _playerWinsGifs[_gifIndex] : _computerWinsGifs[_gifIndex];

    }
}
