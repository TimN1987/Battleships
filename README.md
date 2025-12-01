# 🚢 Battleships

[![.NET](https://img.shields.io/badge/.NET-8.0-blue?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-WPF-purple?logo=csharp)](https://learn.microsoft.com/dotnet/csharp/)
[![WPF](https://img.shields.io/badge/UI-WPF-5C2D91?logo=windows)]()
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

A **full-stack** Battleships game with multiple difficulty settings, customisable house rules, encrypted saving and loading functionality, background music and sound effects. The difficulty settings also have the option for an AI player, with the ability to send game data and receive shot selections from a machine-learning opponent.

## ✨ Features:

- 🖱️ **Interactive game screen with mouse and keyboard controls**

    The game view includes two grids as well as a Captain to give game updates and special shot selection. The main grid can be controlled with keyboard directions or mouse movements, providing interactive updates when a shot is taken. The second grid shows the results of the computer player's shots.

- 🛳️ **C# model using OOP**

    The model consists of different objects for each part of the game (e.g. game, board, ships, computer player). This allows each part of the model to manage its role in a game - for example, the ships track where they have been hit and when they are sunk.

- 🗺️ **Probabilistic heat map**

    The hard mode for the computer player is built around a probabilistic heat map. This makes adjustments each turn based on the outcome of the previous shot to give an estimated probability (for example, 0 chance once a cell is hit or increase the chance when an adjacent cell is hit). This proved to be a more practical solution that a Monte-Carlo algorithm or accurate probability calculations each turn.

- 📺 **Animations and sound effects**

    Storyboards were used to include animations during game play, including accurately positioned explosions using data bindings and simple converter calculations. Message and Sound services provide a loop of background music as well as game updates and sound effects in response to key events. The menu includes a mute button as well as volume controls for background music and foreground sounds.

- 💾 **Encrypted save/load game functionality**

    A SQLite database is used to store game data. The data is packaged into a series of **Data Transfer Objects** (DTOs) for easy JSON serialization. This serialized data is encrypted with AES to ensure that game data cannot easily be read or manipulated. An autosave file is updated after every shot to ensure games can be continued easily.

- 📋 **Menu bar with game, sound and theme options**

    Each view is hosted in a ContentControl under a menu bar, containing game options (e.g. Save, Load and Return Home), volume controls (adjusting the volume settings for the different MediaElements) and theme options. ResourceDictionaries in XAML contain different color definitions which can be selected from the menu bar, for example Classic and Neon themes. The theme effects are applied throughout the game screens.

