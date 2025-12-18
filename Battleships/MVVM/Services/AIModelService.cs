using System.Net.Http;
using Battleships.MVVM.Enums;
using Battleships.MVVM.Model.DataTransferObjects;

namespace Battleships.MVVM.Services;

public interface IAIModelService
{
    int SelectNextShot(GameStateDTO gameStateDTO, out ShotType shotType);
}

public class AIModelService : IAIModelService
{
    private readonly HttpClient _httpClient = new();

    public int SelectNextShot(GameStateDTO gameStateDTO, out ShotType shotType)
    {
        shotType = ShotType.Single;
        return 0;
    }
}
