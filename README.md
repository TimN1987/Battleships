# 🚢 Battleships

[![.NET](https://img.shields.io/badge/.NET-8.0-blue?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-WPF-purple?logo=csharp)](https://learn.microsoft.com/dotnet/csharp/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

A **full-stack** Battleships game with multiple difficulty settings, customisable house rules, encrypted saving and loading functionality, background music and sound effects. The difficulty settings also have the option for an AI player, with the ability to send game data and receive shot selections from a machine-learning opponent.

## ⚙️ Tech Stack:

| Technology | Role |
|------------|------|
| WPF | UI Framework |
| C# | Backend (Models and ViewModels) |
| XAML | UI Markup |
| SQLite | Database management |
| MVVM | Architectural pattern |

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

## 🖼️ Screenshots:

## 📝 Lessons learned:

- **Data storage**
    Serializing the game data to a Json to be encrypted seemed like an excellent idea at the time, especially to ensure that data could not be accessed and changed. It created two major problems - needing to create DTOs to manage the different classes and making it much more difficult to add new data fields without significant rewrites. Next, I will look into Entity Framework or similar for cleaner data management.
- **Separation of concerns**
    While I feel I made good use of object oriented programming (OOP) for large parts of the game design, the viewmodels soon became bloated as they needed to handle resources, game objects and commands. Separating out the resources into (possibly static) classes and creating each command in its own class would help make the viewmodels much cleaner.
- **Events and messaging**
    Later in development, I did separate out lots of the speech, sounds and graphics, but this was built on an overly complicated series of EventAggregator messages. With more experience, care and planning, this could be streamlined to ensure a cleaner and more maintainable system.
- **Styles and themes in XAML**
    As I added more styles, the main resource dictionary became harder to navigate. Using more resource dictionaries, with a sharper focus for each, would help to keep the styles more open to changes and development.

## Background:

In November 2024, a friend gave me the idea to make a Battleships game. I had been planning to look into learning WPF (having so far focused on creating simple applications in WinForms) and decided that this would be a great project to start with. I had soon created a loading screen, but I realised that WPF worked quite differently to WinForms.

After some time working on other projects and reading up on WPF, I returned to my Battleships game in March 2025. This time I was better prepared with some relatively detailed plans and experience of creating simple WPF applications using the Model-View-ViewModel (MVVM) framework.

By summer 2025, I had created the backend of the application with the difficulty settings as well as a basic frontend UI in XAML. There were still a lot of issues to be fixed and features to be completed, but I took a break after some work on training an AI player in Python to integrate into the game.

In October 2025, I returned to the game, fixing the save and load functionality as well as completing the basic UI for game play. The application was now working correctly, but I had a long list of issues to work on over the coming weeks. These included setting up the sound effects and in-game messages, adding Game Over animations and finishing the AI player set up. These issues took me into December 2025.

When I started, I never expected to spend so much time developing this application, nor did I expect the scope to grow so significantly. I am immensely proud of the result, however, and I have learned a huge amount through the process.
