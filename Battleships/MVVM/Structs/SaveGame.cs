using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Battleships.MVVM.View;

namespace Battleships.MVVM.Structs
{
    /// <summary>
    /// A struct for storing save game information to display in lists on the <see cref="SaveGameView"/> and 
    /// the <see cref="LoadGameView"/>.
    /// </summary>
    public struct SaveGame
    {
        public string GameName { get; set; }
        public string SaveDate { get; set; }
        public string SaveTime { get; set; }
        public int SaveSlot { get; set; }

        public SaveGame(int placeHolder)
        {
            GameName = "Empty";
            SaveDate = string.Empty;
            SaveTime = string.Empty;
            SaveSlot = placeHolder;
        }

        public SaveGame(string gameName, DateTime saveDateTime, int saveSlot)
        {
            var dateTimeString = saveDateTime.ToString().Split(' ');

            GameName = gameName;
            SaveDate = dateTimeString[0];
            SaveTime = dateTimeString[1];
            SaveSlot = saveSlot;
        }
    }
}
