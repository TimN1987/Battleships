using System.Net.Http;
using Battleships.MVVM.Enums;
using Battleships.MVVM.Model.DataTransferObjects;

namespace Battleships.MVVM.Services;

public interface IAIModelService
{
    int SelectNextShot(GameStateDTO gameStateDTO, out ShotType shotType);
}

public sealed class AIModelService(HttpClient httpClient) : IAIModelService
{
    private readonly HttpClient _httpClient = httpClient
        ?? throw new ArgumentNullException(nameof(httpClient));

    public int SelectNextShot(GameStateDTO gameStateDTO, out ShotType shotType)
    {
        shotType = ShotType.Single;
        return 0;
    }
}
