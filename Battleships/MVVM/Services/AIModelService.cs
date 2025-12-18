using System.Net.Http;
using System.Net.Http.Json;
using Battleships.MVVM.Enums;
using Battleships.MVVM.Model.DataTransferObjects;

namespace Battleships.MVVM.Services;

public interface IAIModelService
{
    int SelectNextShot(GameStateDTO gameStateDTO, out ShotType shotType);
}

public sealed class AIModelService : IAIModelService
{
    private readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("http://localhost:8000/"),
        Timeout = TimeSpan.FromSeconds(2)
    };

    public int SelectNextShot(GameStateDTO gameStateDTO, out ShotType shotType)
    {
        var response = RequestShotInformation(gameStateDTO).GetAwaiter().GetResult();

        shotType = response.ShotType;
        return response.CellIndex;
    }

    private async Task<AIMoveResponse> RequestShotInformation(GameStateDTO gameStateDTO)
    {
        var httpResponse = await _httpClient.PostAsJsonAsync("next-move", gameStateDTO);

        httpResponse.EnsureSuccessStatusCode();

        var result = await httpResponse.Content.ReadFromJsonAsync<AIMoveResponse>();

        return result is null ? throw new InvalidOperationException("AI returned no move.") : result;
    }
}
