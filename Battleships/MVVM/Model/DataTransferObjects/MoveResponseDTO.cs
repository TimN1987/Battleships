using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Battleships.MVVM.Model.DataTransferObjects
{
    /// <summary>
    /// A class for handling the move information returned from a request to the AI Model to ensure the 
    /// information can be used.
    /// </summary>
    public class MoveResponseDTO
    {
        [JsonProperty("position")]
        public int Position { get; set; }

        [JsonProperty("shot_type")]
        public int ShotType { get; set; } = 0;
    }
}
