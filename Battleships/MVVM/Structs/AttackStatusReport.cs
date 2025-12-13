namespace Battleships.MVVM.Structs
{
    public readonly struct AttackStatusReport(bool isGameOver, IReadOnlyList<SingleTurnReport> singleTurnReports)
    {
        public bool IsGameOver { get; } = isGameOver;
        public IReadOnlyList<SingleTurnReport> Reports { get; } = singleTurnReports;
    }
}
