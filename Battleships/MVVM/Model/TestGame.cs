using Battleships.MVVM.Factories;
using Battleships.MVVM.Structs;

namespace Battleships.MVVM.Model
{
    public class TestGame : Game
    {
        public string Name { get; set; }
        public int Score { get; set; }

        // Pass the required parameter to the base class constructor
        public TestGame(ILoggerFactory eventLogger, GameSetUpInformation information) : base(eventLogger, information)
        {
            Name = "TestGame";
            Score = 1000;
        }

        public string DeclareName()
        {
            return Name;
        }
    }
}
