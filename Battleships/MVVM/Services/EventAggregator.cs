using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Battleships.MVVM.Enums;
using Battleships.MVVM.Structs;
using Battleships.MVVM.ViewModel;
using Battleships.MVVM.View;
using Battleships.MVVM.Model;
using Battleships.MVVM.Model.DataTransferObjects;
using System.Diagnostics;

namespace Battleships.MVVM.Services;

/// <summary>
/// Publishes the type of UserControl selected to inform the <see cref="MainViewModel"/> which UserControl to load 
/// to its ContentControl.
/// </summary>
public class NavigationEvent : PubSubEvent<Type>
{
}

/// <summary>
/// Publishes a request for the current <see cref="ThemeNames"/> type.
/// </summary>
public class ThemeRequestEvent : PubSubEvent
{
}

/// <summary>
/// Publishes the current <see cref="ThemeNames"/> when the theme is updated or a request is received.
/// </summary>
public class ThemeUpdateEvent : PubSubEvent<ThemeNames>
{
}

/// <summary>
/// Published when the menu bar save button is clicked to request that the <see cref="PlayGameViewModel"/> 
/// calls its SaveGame method.
/// </summary>
public class SaveEvent : PubSubEvent
{
}

/// <summary>
/// Published when the <see cref="SaveGameView"/> is used to set the save game name and slot. Requests that 
/// the <see cref="PlayGameViewModel"/> updates the game information and calls its SaveGame method.
/// </summary>
public class SaveAsEvent : PubSubEvent<(string, int)>
{
}

/// <summary>
/// Published when a menu button navigates away from the <see cref="PlayGameView"/> to request an that the 
/// game is autosaved.
/// </summary>
public class AutosaveEvent : PubSubEvent
{
}

/// <summary>
/// Publishes the status of the save game to inform the MainViewModel which save status message to display.
/// </summary>
public class SaveStatusEvent : PubSubEvent<string>
{
}

/// <summary>
/// Publishes when all settings have been entered and the <see cref="PlayGameViewModel"/> needs to create a new game.
/// </summary>
public class CreateGameEvent : PubSubEvent<GameSetUpInformation>
{
}

/// <summary>
/// Publishes when the game is loaded to the <see cref="PlayGameViewModel"/> to inform the MainViewModel 
/// and the <see cref="GameSetUpService"/> that the game is loaded.
/// </summary>
public class StartGameEvent : PubSubEvent
{
}

/// <summary>
/// Published when a transient view needs to check if a game is in progress to ensure that it is displayed 
/// correctly with the appropriate functionality.
/// </summary>
public class RequestGameStatusEvent : PubSubEvent
{
}

/// <summary>
/// Publishes the current game status when requested to ensure that transient views can display correctly 
/// with the appropriate functionality for the current game status.
/// </summary>
public class UpdateGameStatusEvent : PubSubEvent<bool>
{
}

/// <summary>
/// Publishes when the game is over to inform the application that no game is currently in progress to 
/// ensure that menus and buttons are correctly displayed.
/// </summary>
public class GameOverEvent : PubSubEvent
{
}

/// <summary>
/// Publishes the status of a move with a boolean value to indicate if the move is in progress. Ensures 
/// that games are not saved or loaded during a turn.
/// </summary>
public class MoveStatusEvent : PubSubEvent<bool>
{
}

// <summary>
/// Published when a game is loaded including the <see cref="GameDTO"/> instance to use in the <see 
/// cref="Game"/> constructor.
/// </summary>
public class GameLoadedEvent : PubSubEvent<GameDTO>
{
}

/// <summary>
/// Published when the autosave game needs to be loaded telling the <see cref="PlayGameViewModel"/> to 
/// load the game.
/// </summary>
public class LoadAutosaveEvent : PubSubEvent
{
}

/// <summary>
/// Published when a named game needs to be loaded, passing the name of the game and its save slot to the 
/// <see cref="PlayGameViewModel"/>.
/// </summary>
public class LoadGameEvent : PubSubEvent<(string, int)>
{
}

public class ClearDataEvent : PubSubEvent
{
}

/// <summary>
/// Publishes the Uri for the sound that the <see cref="MainViewModel"/> should load to the MediaElement.
/// </summary>
public class LoadSoundEvent : PubSubEvent<Uri>
{
}

/// <summary>
/// Publishes the Uri for the speech that the <see cref="MainViewModel"/> should load to the MediaElement.
/// </summary>
public class LoadSpeechEvent : PubSubEvent<Uri>
{
}

/// <summary>
/// Publishes the Uri for the captain image to be displayed in the <see cref="PlayGameView"/> based on the 
/// latest <see cref="GameEvent"/>.
/// </summary>
public class LoadCaptainEvent : PubSubEvent<Uri>
{
}

/// <summary>
/// Publishes when the background music has ended to inform the <see cref="SoundService"/> that it needs to 
/// send the next background music to the <see cref="MainViewModel"/> to load.
/// </summary>
public class BackgroundMusicEndedEvent : PubSubEvent
{
}

/// <summary>
/// Publishes the Uri for the background music that the <see cref="MainViewModel"/> should load to the 
/// MediaElement.
/// </summary>
public class NextSongEvent : PubSubEvent<Uri>
{
}

/// <summary>
/// Publishes the status of the full screen to inform the <see cref="MainWindow"> which full screen 
/// status to display.
/// </summary>
public class FullScreenEvent : PubSubEvent<bool>
{
}

/// <summary>
/// Publishes the status of the key down to inform the <see cref="MainViewModel"/> that the full screen 
/// status has changed.
/// </summary>
public class KeyDownFullScreenEvent : PubSubEvent
{
}

/// <summary>
/// Publishes the status of the theme to inform the <see cref="MainViewModel"> which theme to load.
/// </summary>
public class ChangeThemeEvent : PubSubEvent<string>
{
}

/// <summary>
/// Publishes the type of event that has heppened in a game to keep the user updated with messages and 
/// sound effects..
/// </summary>
public class GameEventEvent : PubSubEvent<GameEvent>
{
}

/// <summary>
/// Publishes a message to be displayed for the user in the <see cref="PlayGameView"/> to explain what 
/// has happened.
/// </summary>
public class UserMessageEvent : PubSubEvent<string>
{
}
