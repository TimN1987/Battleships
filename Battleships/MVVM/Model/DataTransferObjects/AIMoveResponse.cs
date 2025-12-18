using Battleships.MVVM.Enums;

namespace Battleships.MVVM.Model.DataTransferObjects;

public sealed class AIMoveResponse
{
    public int CellIndex { get; init; }
    public ShotType ShotType { get; init; }
}
