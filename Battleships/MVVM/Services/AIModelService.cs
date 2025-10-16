using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Battleships.MVVM.Enums;
using Battleships.MVVM.Model.DataTransferObjects;
using Newtonsoft.Json;

namespace Battleships.MVVM.Services
{
    public interface IAIModelService
    {
        int SelectNextSHot(in GameStateDTO gameStateDTO, out ShotType shotType);
    }
    
    public class AIModelService : IAIModelService
    {
        private readonly HttpClient _httpClient;

        public AIModelService()
        {

        }

        public int SelectNextSHot(in GameStateDTO gameStateDTO, out ShotType shotType)
        {
            shotType = ShotType.Single;
            return 0;
        }

        internal int RequestShotInformation(in GameStateDTO gameStateDTO, out ShotType shotType)
        {
            shotType = ShotType.Single;

            var featureDataJson = JsonConvert.SerializeObject(gameStateDTO, Formatting.None);
            var httpContent = new StringContent(featureDataJson, System.Text.Encoding.UTF8, "application/json");

            var response = _httpClient.PostAsync("/get_move", httpContent).Result;
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("AI server error: " + response.StatusCode);
                return -1;
            }

            var responseJson = response.Content.ReadAsStringAsync().Result;
            var moveResult = JsonConvert.DeserializeObject<MoveResponseDTO>(responseJson);

            if (moveResult is null)
                return -1;

            shotType = Enum.IsDefined(typeof(ShotType), moveResult.ShotType)
                ? (ShotType)moveResult.ShotType
                : ShotType.Single;

            return moveResult.Position;
        }
    }
}
