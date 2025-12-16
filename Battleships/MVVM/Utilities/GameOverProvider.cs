namespace Battleships.MVVM.Utilities;

public static class GameOverProvider
{
    // Player wins messages
    private const string PlayerWinMessageOne = "Congratulations! You've sunk all enemy ships and won the battle!";
    private const string PlayerWinMessageTwo = "Victory is yours! The enemy fleet has been defeated!";
    private const string PlayerWinMessageThree = "Well done, Captain! You've outmaneuvered the enemy and secured a win!";
    private const string PlayerWinMessageFour = "You've emerged victorious! The seas are yours!";
    private const string PlayerWinMessageFive = "Fantastic job! You've successfully destroyed the enemy fleet!";

    // Computer wins messages
    private const string ComputerWinMessageOne = "Defeat! The enemy fleet has sunk all your ships!";
    private const string ComputerWinMessageTwo = "The enemy has won! Your fleet has been destroyed!";
    private const string ComputerWinMessageThree = "Alas, Captain! The enemy has outmaneuvered you and claimed victory!";
    private const string ComputerWinMessageFour = "The seas belong to the enemy now! Your ships have been sunk!";
    private const string ComputerWinMessageFive = "The enemy fleet has triumphed! Better luck next time!";

    // Player wins gifs
    private static readonly Uri _playerWinsGif1 = new(@"pack://application:,,,/MVVM/Resources/Images/GameOver/playerwinsgif1.gif");
    private static readonly Uri _playerWinsGif2 = new(@"pack://application:,,,/MVVM/Resources/Images/GameOver/playerwinsgif2.gif");
    private static readonly Uri _playerWinsGif3 = new(@"pack://application:,,,/MVVM/Resources/Images/GameOver/playerwinsgif3.gif");

    // Computer wins gifs
    private static readonly Uri _computerWinsGif1 = new(@"pack://application:,,,/MVVM/Resources/Images/GameOver/computerwinsgif1.png");

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
    private static Uri[] _playerWinsGifs = [
        _playerWinsGif1,
        _playerWinsGif2,
        _playerWinsGif3
    ];
    private static Uri[] _computerWinsGifs = [
        _computerWinsGif1
    ];

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
