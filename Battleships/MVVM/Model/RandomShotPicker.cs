using Battleships.MVVM.Enums;
using Battleships.MVVM.Model.DataTransferObjects;
using Battleships.MVVM.Utilities;

namespace Battleships.MVVM.Model
{
    /// <summary>
    /// Defines a contract for the generation of validated random shots of a given type to enable a computer 
    /// player to select valid moves. Facilitates the use of diagonal strategies for more efficient ship 
    /// hunting.
    /// </summary>
    public interface IRandomShotPicker
    {
        int GenerateRandomShot(ref ShotType shotType, int? diagonalSpacing = null);
        void UpdateShotPickerLists(int gridPosition, ShotType shotType);
    }

    /// <summary>
    /// A class for generating random shots for a computer player in the Battleships game. It contains methods 
    /// to generate lists of possible shots for different random strategies. It provides the functionality to 
    /// select a random shot of a particular type with full validation.
    /// </summary>
    public class RandomShotPicker : IRandomShotPicker
    {
        #region Fields
        private static readonly Dictionary<ShotType, int[]> _shotDeltas = new()
        {
            { ShotType.Single, [0] },
            { ShotType.AirstrikeUpRight, [0, -9, -18] },
            { ShotType.AirstrikeDownRight, [0, 11, 22] },
            { ShotType.Bombardment, [0, 1, -1, 10, -10] }
        };

        private readonly IEnumerable<int> _gridRange;
        private List<int> _availableShots;
        private List<int> _availableDiagonalSpacingTwoShots;
        private List<int> _availableDiagonalSpacingThreeShots;
        private List<int> _availableDiagonalSpacingFourShots;
        private List<int> _availableDiagonalSpacingFiveShots;
        #endregion //Fields

        /// <summary>
        /// Parameterless constructor used for new game instantiation.
        /// </summary>
        public RandomShotPicker()
        {
            _gridRange = Enumerable.Range(0, 100);

            _availableShots = GenerateAllShotsList();
            _availableDiagonalSpacingTwoShots = GenerateDiagonalShotsList(2);
            _availableDiagonalSpacingThreeShots = GenerateDiagonalShotsList(3);
            _availableDiagonalSpacingFourShots = GenerateDiagonalShotsList(4);
            _availableDiagonalSpacingFiveShots = GenerateDiagonalShotsList(5);
        }

        /// <summary>
        /// <see cref="RandomShotPickerDTO"/> used to load previous game state data during the load game process.
        /// </summary>
        /// <param name="randomShotPickerDTO">A <see cref="RandomShotPickerDTO"/> containing loaded information 
        /// to return to a previous game state.</param>
        public RandomShotPicker(RandomShotPickerDTO randomShotPickerDTO)
        {
            _gridRange = Enumerable.Range(0, 100);

            _availableShots = randomShotPickerDTO.AvailableShots;
            _availableDiagonalSpacingTwoShots = randomShotPickerDTO.AvailableDiagonalSpacingTwoShots;
            _availableDiagonalSpacingThreeShots = randomShotPickerDTO.AvailableDiagonalSpacingThreeShots;
            _availableDiagonalSpacingFourShots = randomShotPickerDTO.AvailableDiagonalSpacingFourShots;
            _availableDiagonalSpacingFiveShots = randomShotPickerDTO.AvailableDiagonalSpacingFiveShots;
        }

        #region Initialization Methods
        /// <summary>
        /// Creates a list of all possible shot positions for the start of the game.
        /// </summary>
        /// <returns>A list of integers representing all the possible shots.</returns>
        private List<int> GenerateAllShotsList()
        {
            return _gridRange.ToList();
        }

        /// <summary>
        /// Creates a list of all possible shots following a diagonal pattern with the given spacing. Enables 
        /// a computer player to easily follow a diagonal lines strategy for shot placement.
        /// </summary>
        /// <param name="spacing">An integer representing the gap between shots in the same row or column. 
        /// The spacing must be between two and five.</param>
        /// <returns>A list of integers representing the possible shots in the given pattern.</returns>
        /// <remarks>The diagonal patterns allow the computer player to spread out moves for better grid 
        /// coverage without leaving gaps for ships of a certain size.</remarks>
        private List<int> GenerateDiagonalShotsList(int spacing)
        {
            if (!Enumerable.Range(2, 4).Contains(spacing))
                return [];

            var random = RandomProvider.Instance.Next(2);

            bool slopingUp = (random == 0);

            if (slopingUp)
                return _gridRange
                    .Where(number => ((number / 10) + (number % 10)) % spacing == 0)
                    .ToList();

            else
                return _gridRange
                    .Where(number => ((number / 10) - (number % 10) + 9 * spacing) % spacing == 0)
                    .ToList();
        }
        #endregion //Initialization methods

        #region Shot Selection Methods
        /// <summary>
        /// Generates a random shot of the given <see cref="ShotType"/> that follows a diagonal strategy with 
        /// the given <paramref name="diagonalSpacing"/>. Provides validation to ensure that the shot can be 
        /// successfully played.
        /// </summary>
        /// <param name="shotType">The <see cref="ShotType"/> that the computer player has chosen to use.</param>
        /// <param name="diagonalSpacing">An integer representing the spacing between the diagonal lines.</param>
        /// <returns>An integer representing a valid grid position to target with the selected <see 
        /// cref="ShotType"/>.</returns>
        /// <remarks>Includes a counter to ensure that the attempts to generate a valid shot are limited 
        /// before switching to a single shot type. The ref parameter <paramref name="shotType"/> ensure that 
        /// the calling method has its shot type updated if necessary.</remarks>
        public int GenerateRandomShot(ref ShotType shotType, int? diagonalSpacing = null)
        {
            bool isValidShot = false;
            int gridPosition = 0;

            var selectionList = new List<int>();

            //Multi target shots do not always fit on diagonal lines.
            if (shotType is not ShotType.Single)
                selectionList = _availableShots;

            while (selectionList.Count == 0)
            {
                selectionList = diagonalSpacing switch
                {
                    2 => _availableDiagonalSpacingTwoShots,
                    3 => _availableDiagonalSpacingThreeShots,
                    4 => _availableDiagonalSpacingFourShots,
                    5 => _availableDiagonalSpacingFiveShots,
                    _ => _availableShots
                };

                //Try closer spacing if no available shots.
                if (selectionList.Count == 0)
                    diagonalSpacing--;

                //Return -1 if there are no available shots for any spacing.
                if (diagonalSpacing == 0 && selectionList.Count == 0)
                    return -1;
            }

            var attemptCount = 0;

            while (!isValidShot)
            {
                gridPosition = ReturnRandomElement(selectionList);

                if (gridPosition == -1)
                    gridPosition = ReturnRandomElement(_availableShots);

                isValidShot = GenerateValidShotPositions(gridPosition, shotType, out _);

                attemptCount++;

                if (attemptCount > 10) //Will return a valid single cell shot on its first attempt
                {
                    shotType = ShotType.Single;
                    isValidShot = GenerateValidShotPositions(gridPosition, ShotType.Single, out _);
                }
            }

            UpdateShotPickerLists(gridPosition, shotType);

            return gridPosition;
        }

        /// <summary>
        /// Uses the given <paramref name="gridPosition"/> and <paramref name="shotType"/> to update the 
        /// available postion lists to ensure that future shots are valid.
        /// </summary>
        /// <param name="gridPosition">An integer representing the position of the shot to be updated.</param>
        /// <param name="shotType">The <see cref="ShotType"/> that needs to be updated.</param>
        /// <remarks>Must be called from the <see cref="Board"/> class if shots have been played without 
        /// using the <see cref="RandomShotPicker"/> to avoid duplication of shots.</remarks>
        public void UpdateShotPickerLists(int gridPosition, ShotType shotType)
        {

            if (GenerateValidShotPositions(gridPosition, shotType, out List<int> shotPositions))
            {
                _availableShots = _availableShots
                    .Except(shotPositions)
                    .ToList();

                _availableDiagonalSpacingTwoShots = _availableDiagonalSpacingTwoShots
                    .Except(shotPositions)
                    .ToList();

                _availableDiagonalSpacingThreeShots = _availableDiagonalSpacingThreeShots
                    .Except(shotPositions)
                    .ToList();

                _availableDiagonalSpacingFourShots = _availableDiagonalSpacingFourShots
                    .Except(shotPositions)
                    .ToList();

                _availableDiagonalSpacingFiveShots = _availableDiagonalSpacingFiveShots
                    .Except(shotPositions)
                    .ToList();
            }
        }

        /// <summary>
        /// Checks if a given shot can be played and generates a list of all the grid positions attacked by that 
        /// shot. Provides validation to ensure that the shot does not go out of bounds and that all the positions 
        /// to be attacked are available.
        /// </summary>
        /// <param name="gridPosition">An integer representing the selected grid position.</param>
        /// <param name="shotType">The <see cref="ShotType"/> that should be played.</param>
        /// <param name="shotPositions">A list of integers representing all the grid positions that would be 
        /// hit by the shot. Generates an empty list if the shot is invalid.</param>
        /// <returns>A boolean value indicating whether the shot is valid.</returns>
        /// <remarks>If false is returned, the calling method will need to find an alternative shot.</remarks>
        private bool GenerateValidShotPositions(int gridPosition, ShotType shotType, out List<int> shotPositions)
        {
            var row = gridPosition / 10;
            var column = gridPosition % 10;
            shotPositions = [];

            if (shotType == ShotType.AirstrikeUpRight && (column > 7 || row < 2))
                return false;

            if (shotType == ShotType.AirstrikeDownRight && (column > 7 || row > 7))
                return false;

            if (shotType == ShotType.Bombardment && (column < 1 || column > 8 || row < 1 || row > 8))
                return false;

            var deltas = _shotDeltas.GetValueOrDefault(shotType, [0]);

            shotPositions = deltas.Select(d => gridPosition + d).ToList();

            if (shotPositions.Except(_availableShots).Any())
            {
                shotPositions = [];
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns a random element from a given list of integers.
        /// </summary>
        /// <param name="possibleShots">A list of integer values representing possible grid positions to attack.</param>
        /// <returns>An integer value representing the randomly selected grid position to attack. Returns -1, 
        /// if the given list is null or empty.</returns>
        /// <remarks>The calling method needs to provide a fallback in case the return value is -1.</remarks>
        private static int ReturnRandomElement(List<int> possibleShots)
        {
            if (possibleShots is null || possibleShots.Count == 0)
                return -1;

            var length = possibleShots.Count;
            var index = RandomProvider.Instance.Next(0, length);

            return possibleShots[index];
        }

        #endregion //Shot Selection Methods

        /// <summary>
        /// Generates a <see cref="RandomShotPickerDTO"/> for Json serialization to ensure simple data storage.
        /// </summary>
        /// <returns>A <see cref="RandomShotPickerDTO"/> instance containing key information in its 
        /// current state.</returns>
        public RandomShotPickerDTO GetDTO()
        {
            return new RandomShotPickerDTO
            {
                AvailableShots = _availableShots,
                AvailableDiagonalSpacingTwoShots = _availableDiagonalSpacingTwoShots,
                AvailableDiagonalSpacingThreeShots = _availableDiagonalSpacingThreeShots,
                AvailableDiagonalSpacingFourShots = _availableDiagonalSpacingFourShots,
                AvailableDiagonalSpacingFiveShots = _availableDiagonalSpacingFiveShots
            };
        }
    }
}
