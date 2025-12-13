namespace Battleships.MVVM.Utilities;

public static class GameOverProvider
{
    private static bool _playerWins = false;
    private static int _messageIndex = 0;
    private static string[] _playerWinsMessages = [];
    private static string[] _computerWinsMessages = [];
    private static int _gifIndex = 0;
    private static Uri[] _playerWinsGifs = [];
    private static Uri[] _computerWinsGifs = [];

    // Constant messages

    // Game over gifs

    // Methods

    /// <summary>
    /// Returns a game over message based on whether the player wins or the computer wins. Used to update the 
    /// UI with a relevant message at the end of the game.
    /// </summary>
    public static string GetGameOverMessage(bool playerWins)
    {
        _playerWins = playerWins;
        return _playerWins ? _playerWinsMessages[_messageIndex] : _computerWinsMessages[_messageIndex];
    }

    /// <summary>
    /// Returns a game over gif based on whether the player wins or the computer wins. Used to update the UI 
    /// with a relevant gif at the end of the game.
    /// </summary>
    public static Uri GetGameOverGif(bool playerWins)
    {
        _playerWins = playerWins;
        return _playerWins ? _playerWinsGifs[_gifIndex] : _computerWinsGifs[_gifIndex];

    }
}
