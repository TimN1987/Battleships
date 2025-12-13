using Battleships.MVVM.Enums;

namespace Battleships.MVVM.Model.DataTransferObjects
{
    /// <summary>
    /// A class for storing <see cref="ComputerPlayer"/> state information to enable simple Json serialization 
    /// and data persistance.
    /// </summary>
    public class ComputerPlayerDTO
    {
        public RandomShotPickerDTO RandomShotPickerDTO { get; set; } = new RandomShotPickerDTO();
        public GameDifficulty GameDifficulty { get; set; } = GameDifficulty.Easy;
        public bool ShipsCanTouch { get; set; } = false;
        public int[] ProbabilityDensityMap { get; set; } = [];
        public int[] Directions { get; set; } = [];
        public List<int> AvailablePositions { get; set; } = [];
        public bool AirstrikeAllowed { get; set; } = false;
        public bool BombardmentAllowed { get; set; } = false;
        public int AirstrikeHitCount { get; set; } = 0;
        public int BombardmentHitCount { get; set; } = 0;
        public int MaximumShipSize { get; set; } = 5;
        public List<(int gridPosition, ShotType shotType)> CompletedShots { get; set; } = [];
    }
}
