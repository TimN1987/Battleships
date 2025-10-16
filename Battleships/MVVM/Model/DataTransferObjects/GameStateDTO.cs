using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace Battleships.MVVM.Model.DataTransferObjects
{
    /// <summary>
    /// A class for storing game state information to enable simple Json serialization for transfer to the 
    /// AI player model server.
    /// </summary>
    public class GameStateDTO
    {
        [JsonProperty("grid")]
        public int[] Grid { get; init; } = new int[100];

        [JsonProperty("remaining_ships")]
        public int[] RemainingShipSizes { get; init; } = [5, 4, 3, 3, 2];

        [JsonProperty("ships_can_touch")]
        public bool ShipsCanTouch { get; init; } = false;

        [JsonProperty("airstrike")]
        public bool AirstrikeAllowed { get; init; } = false;

        [JsonProperty("bombardment")]
        public bool BombardmentAllowed { get; init; } = false;

        [JsonProperty("airstrike_hit_count")]
        public int AirstrikeHitCount { get; init; } = 0;

        [JsonProperty("bombardment_hit_count")]
        public int BombardmentHitCount { get; init; } = 0;

        [JsonProperty("airstrike_available")]
        public bool AirstrikeAvailable { get; init; } = false;

        [JsonProperty("bombardment_available")]
        public bool BombardmentAvailable { get; init; } = false;

        public GameStateDTO() { }

        public GameStateDTO(
        int[] grid,
        int[] remainingShipSizes,
        bool shipsCanTouch,
        bool airstrikeAllowed,
        bool bombardmentAllowed,
        int airstrikeHitCount,
        int bombardmentHitCount,
        bool airstrikeAvailable,
        bool bombardmentAvailable
        )
        {
            Grid = grid ?? new int [100];
            RemainingShipSizes = remainingShipSizes ?? [5, 4, 3, 3, 2];
            ShipsCanTouch = shipsCanTouch;
            AirstrikeAllowed = airstrikeAllowed;
            BombardmentAllowed = bombardmentAllowed;
            AirstrikeHitCount = airstrikeHitCount;
            BombardmentHitCount = bombardmentHitCount;
            AirstrikeAvailable = airstrikeAvailable;
            BombardmentAvailable = bombardmentAvailable;
        }
    }
}
