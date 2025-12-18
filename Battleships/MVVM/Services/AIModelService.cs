using System.Net.Http;
using System.Net.Http.Json;
using Battleships.MVVM.Enums;
using Battleships.MVVM.Model.DataTransferObjects;

namespace Battleships.MVVM.Services;

public interface IAIModelService
{
    int SelectNextShot(GameStateDTO gameStateDTO, out ShotType shotType);
}

public sealed class AIModelService() : IAIModelService
{
    private readonly HttpClient _httpClient = new();

    public int SelectNextShot(GameStateDTO gameStateDTO, out ShotType shotType)
    {
        var response = RequestShotInformation(gameStateDTO).GetAwaiter().GetResult();

        shotType = response.ShotType;
        return response.CellIndex;
    }

    private async Task<AIMoveResponse> RequestShotInformation(GameStateDTO gameStateDTO)
    {
        var httpResponse = await _httpClient.PostAsJsonAsync(
            "http://localhost:8000/next-move",
            gameStateDTO);

        httpResponse.EnsureSuccessStatusCode();

        var result = await httpResponse.Content.ReadFromJsonAsync<AIMoveResponse>();

        if (result is null)
            throw new InvalidOperationException("AI returned no move.");

        return result;
    }
}
