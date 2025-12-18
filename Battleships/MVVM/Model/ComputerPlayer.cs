using Battleships.MVVM.Enums;
using Battleships.MVVM.Model.DataTransferObjects;
using Battleships.MVVM.Services;
using Battleships.MVVM.Structs;
using Battleships.MVVM.Utilities;

namespace Battleships.MVVM.Model;

/// <summary>
/// Defines a contract for generating computer moves in Battleships with different difficulty levels and 
/// strategies.
/// </summary>
public interface IComputerPlayer
{
    int ChooseNextMove(GridCellState[] playerGrid, List<int> remainingShipSizes, SingleTurnReport lastMoveReport, out ShotType shotType);

}

/// <summary>
/// This class implements the IComputerPlayer and provides methods for choosing computer moves with different 
/// difficulty levels. It includes a <see cref="RandomShotPicker"/> as well as a probability density map for 
/// a wide range of strategies for computer shots.
/// </summary>
public class ComputerPlayer : IComputerPlayer
{
    #region Constants
    protected const int AirstrikeRequiredHitsToActivate = 5;
    protected const int BombardmentRequiredHitsToActivate = 7;
    #endregion //Constants

    #region Fields
    private readonly RandomShotPicker _randomShotPicker;
    private readonly IAIModelService _aiModelService;
    private readonly Dictionary<ShipType, int> _shipSizes;
    private readonly GameDifficulty _gameDifficulty;
    private readonly bool _shipsCanTouch;
    private readonly int[] _probabilityDensityMap;
    private readonly int[] _directions;

    private static readonly Dictionary<ShotType, int[]> _shotDeltas = new()
    {
        { ShotType.Single, [0] },
        { ShotType.AirstrikeUpRight, [0, -9, -18] },
        { ShotType.AirstrikeDownRight, [0, 11, 22] },
        { ShotType.Bombardment, [0, 1, -1, 10, -10] }
    };

    private readonly List<int> _availablePositions;

    private readonly bool _airstrikeAllowed;
    private readonly bool _bombardmentAllowed;
    private int _airstrikeHitCount;
    private int _bombardmentHitCount;

    private int _maximumShipSize;

    private readonly List<(int gridPosition, ShotType shotType)> _completedShots;
    #endregion //Fields

    #region Properties
    public bool AirstrikeActivated => _airstrikeAllowed && (_airstrikeHitCount >= AirstrikeRequiredHitsToActivate);
    public bool BombardmentActivated => _bombardmentAllowed && (_bombardmentHitCount >= BombardmentRequiredHitsToActivate);
    #endregion //Properties

    /// <summary>
    /// Primary constructor uses <see cref="GameSetUpInformation"/> for instantiation during a new game 
    /// set up.
    /// </summary>
    /// <param name="information">The set up information stored in a <see cref="GameSetUpInformation"/> 
    /// instance.</param>
    public ComputerPlayer(GameSetUpInformation information)
    {
        _randomShotPicker = new RandomShotPicker();
        _aiModelService = new AIModelService();
        _gameDifficulty = information.Difficulty;
        _shipsCanTouch = information.ShipsCanTouch;
        _airstrikeAllowed = information.AirstrikeAllowed;
        _bombardmentAllowed = information.BombardmentAllowed;

        _availablePositions = [.. Enumerable.Range(0, 100)];

        _maximumShipSize = 5;
        _probabilityDensityMap = SetUpProbabilityDensityMap();
        _directions = [1, -1, 10, -10];

        _shipSizes = new Dictionary<ShipType, int>
        {
            { ShipType.Battleship, 4 },
            { ShipType.Carrier, 5 },
            { ShipType.Cruiser, 3 },
            { ShipType.Destroyer, 2 },
            { ShipType.Submarine, 3 }
        };

        _completedShots = [];
    }

    /// <summary>
    /// <see cref="ComputerPlayerDTO"/> used to load previous game state data during the load game process.
    /// </summary>
    /// <param name="computerPlayerDTO">A <see cref="ComputerPlayerDTO"/> containing loaded information 
    /// to return to a previous game state.</param>
    public ComputerPlayer(ComputerPlayerDTO computerPlayerDTO)
    {
        _shipSizes = new Dictionary<ShipType, int>
        {
            { ShipType.Battleship, 4 },
            { ShipType.Carrier, 5 },
            { ShipType.Cruiser, 3 },
            { ShipType.Destroyer, 2 },
            { ShipType.Submarine, 3 }
        };

        _randomShotPicker = new RandomShotPicker(computerPlayerDTO.RandomShotPickerDTO);
        _aiModelService = new AIModelService();
        _gameDifficulty = computerPlayerDTO.GameDifficulty;
        _shipsCanTouch = computerPlayerDTO.ShipsCanTouch;
        _probabilityDensityMap = computerPlayerDTO.ProbabilityDensityMap;
        _directions = computerPlayerDTO.Directions;
        _availablePositions = computerPlayerDTO.AvailablePositions;
        _airstrikeAllowed = computerPlayerDTO.AirstrikeAllowed;
        _bombardmentAllowed = computerPlayerDTO.BombardmentAllowed;
        _airstrikeHitCount = computerPlayerDTO.AirstrikeHitCount;
        _bombardmentHitCount = computerPlayerDTO.BombardmentHitCount;
        _maximumShipSize = computerPlayerDTO.MaximumShipSize;
        _completedShots = computerPlayerDTO.CompletedShots;
    }

    #region Shot Selection Methods
    /// <summary>
    /// Selects the next computer move based on the difficulty setting and the updated game information 
    /// from the last turn.
    /// </summary>
    /// <param name="playerGrid">A <see cref="GridCellState"/> array representing the player board.</param>
    /// <param name="remainingShipSizes">A list of integers representing the sizes of unsunk player ships.</param>
    /// <param name="lastMoveReport">The <see cref="SingleTurnReport"/> from the last turn used to update 
    /// stored game information to support decision making.</param>
    /// <param name="shotType">Returns the selected shot type generated for the move using the out keyword. 
    /// Used to complete the <see cref="SingleTurnReport"/>.</param>
    /// <returns>An integer value representing the selected grid position.</returns>
    public int ChooseNextMove(GridCellState[] playerGrid, List<int> remainingShipSizes, SingleTurnReport lastMoveReport, out ShotType shotType)
    {
        if (!lastMoveReport.FirstTurn)
            ProcessAttackOutcomes(remainingShipSizes, lastMoveReport);

        shotType = ShotType.Single;

        var attackGridPosition = _gameDifficulty switch
        {
            GameDifficulty.Easy => GenerateEasyMove(playerGrid, out shotType),
            GameDifficulty.Medium => GenerateMediumMove(playerGrid, out shotType),
            GameDifficulty.Hard => GenerateHardMove(playerGrid, out shotType),
            GameDifficulty.AI => GenerateAIMove(playerGrid, remainingShipSizes, out shotType),
            _ => 0
        };

        RemoveAllTargetedCells(attackGridPosition, shotType);
        _completedShots.Add((attackGridPosition, shotType));

        if (shotType == ShotType.AirstrikeDownRight || shotType == ShotType.AirstrikeUpRight)
            _airstrikeHitCount = 0;
        if (shotType == ShotType.Bombardment)
            _bombardmentHitCount = 0;

        return attackGridPosition;
    }

    /// <summary>
    /// Generates a randomly generated easy difficulty move for the computer player to select. It uses 
    /// information from the player grid and returns a grid position and shot type. It uses random number 
    /// generation to mimic uncertainty in the shot selection.
    /// </summary>
    /// <param name="playerGrid">A <see cref="GridCellState"/> array representing the player board.</param>
    /// <param name="shotType">The selected shot type, used for creating the <see cref="SingleTurnReport"/>.</param>
    /// <returns>An integer value representing the selected grid position.</returns>
    internal int GenerateEasyMove(GridCellState[] playerGrid, out ShotType shotType)
    {
        var gridPosition = GenerateRandomTargetPhaseShot(playerGrid, 2, out shotType); //50% chance of selecting a target strategy

        //gridPosition == -1 if no hits are available to target.
        if (gridPosition >= 0 && gridPosition < 100)
            return gridPosition;

        shotType = GenerateRandomShotType(3); //Adds additional 67% of total random numbers that return single shot

        gridPosition = _randomShotPicker.GenerateRandomShot(ref shotType, 2); //Uses spacing of two for diagonal lines strategy

        return gridPosition;
    }

    /// <summary>
    /// Generates a randomly generated medium computer move following a hunt and target strategy with a 
    /// variable diagonal lines strategy in the hunt phase. Randomly selects an available square next to an 
    /// unsunk hit if possible. Otherwise randomly selects a cell on the diagonal lines with the widest 
    /// possible spacing (using the maximum ship size).
    /// </summary>
    /// <param name="playerGrid">A <see cref="GridCellState"/> array representing the player board.</param>
    /// <param name="shotType">The selected shot type, used for creating the <see cref="SingleTurnReport"/>.</param>
    /// <returns>An integer value representing the selected grid position.</returns>
    internal int GenerateMediumMove(GridCellState[] playerGrid, out ShotType shotType)
    {
        var gridPosition = GenerateRandomTargetPhaseShot(playerGrid, 1, out shotType); //100% chance of selecting a target strategy

        if (gridPosition >= 0 && gridPosition < 100)
            return gridPosition;

        shotType = SelectMultipleTargetShotType();

        //Generates a random shot with the widest possible spacing given the size of ships remaining.
        //Strategy aims to catch the largest ship first as quickly as possible.
        gridPosition = _randomShotPicker.GenerateRandomShot(ref shotType, _maximumShipSize);

        return gridPosition;
    }

    /// <summary>
    /// Selects a target phase shot based on hit positions if they exist. Otherwise, uses the probability 
    /// density map to select a hunt phase shot.
    /// </summary>
    /// <param name="playerGrid">A <see cref="GridCellState"/> array representing the player board.</param>
    /// <param name="shotType">The selected shot type, used for creating the <see cref="SingleTurnReport"/>.</param>
    /// <returns>An integer value representing the selected grid position.</returns>
    internal int GenerateHardMove(GridCellState[] playerGrid, out ShotType shotType)
    {
        var targetPhaseShot = SelectTargetPhaseShot(playerGrid, out shotType);

        //targestPhaseShot == -1 if no hits available to target.
        if (targetPhaseShot >= 0)
            return targetPhaseShot;

        var huntPhaseShot = SelectHuntPhaseShot(out shotType);

        return huntPhaseShot;
    }

    internal int GenerateAIMove(GridCellState[] playerGrid, List<int> remainingShipSizes, out ShotType shotType)
    {
        var gameStateDTO = new GameStateDTO
        {
            Grid = playerGrid
            .Select(state => (int)state)
            .ToArray(),
            RemainingShipSizes = remainingShipSizes.ToArray(),
            ShipsCanTouch = _shipsCanTouch,
            AirstrikeAllowed = _airstrikeAllowed,
            BombardmentAllowed = _bombardmentAllowed,
            AirstrikeHitCount = _airstrikeHitCount,
            BombardmentHitCount = _bombardmentHitCount,
            AirstrikeAvailable = AirstrikeActivated,
            BombardmentAvailable = BombardmentActivated
        };

        return _aiModelService.SelectNextShot(gameStateDTO, out shotType);
    }

    #endregion //Shot Selection Methods

    #region Shot Selection Helper Methods

    /// <summary>
    /// Selects a target phase shot that hits the most hit adjacent cells. If multiple shots hit the same 
    /// number of hit adjacent cells, takes the best total probability score.
    /// </summary>
    /// <param name="playerGrid">A <see cref="GridCellState"/> array representing the player board.</param>
    /// <param name="shotType">The selected shot type, used for creating the <see cref="SingleTurnReport"/>.</param>
    /// <returns></returns>
    private int SelectTargetPhaseShot(GridCellState[] playerGrid, out ShotType shotType)
    {
        var hitsList = GenerateHitsList(playerGrid);
        shotType = ShotType.Single;

        if (hitsList.Count == 0)
            return -1;

        var shotsToEvaluate = new List<(int gridPosition, bool isRow)>();

        var targetShot = SelectTargetPhaseShotFromLineOfHits(hitsList, out bool isRow);

        if (targetShot == -1)
            shotsToEvaluate = SelectBestHitAdjacentTargetShots(hitsList);
        else
            shotsToEvaluate.Add((targetShot, isRow));

        if (shotsToEvaluate.Count == 0)
            return -1;

        var hitAdjacentCells = hitsList
            .SelectMany(hit => GenerateValidHitAdjacentShots(hit))
            .Where(shot => shot.gridPosition >= 0 && _availablePositions.Contains(shot.gridPosition))
            .Select(shot => shot.gridPosition)
            .Distinct()
            .ToList();

        var scoredShots = shotsToEvaluate
            .SelectMany(shot => EvaluateShotSelectionOptions(shot.gridPosition, shot.isRow, hitAdjacentCells))
            .OrderByDescending(shot => shot.totalHitAdjacent)
            .ThenBy(shot => shot.totalProbabilityScore);

        if (!scoredShots.Any())
            return -1;

        var bestShot = scoredShots.First();

        shotType = bestShot.shotType;
        return bestShot.position;
    }

    /// <summary>
    /// Selects highest probability score positions and evaluates the impace of possible attacks on those 
    /// positions to find the position and shot type with the highest total probability score.
    /// </summary>
    /// <param name="shotType">The shot type for the attack with the highest total probability score.</param>
    /// <returns>An integer value representing the position to attack in the hunt phase.</returns>
    internal int SelectHuntPhaseShot(out ShotType shotType)
    {
        shotType = ShotType.Single;

        var bestProbabilityTargets = SelectBestProbabilityTargets()
            .Where(position => _availablePositions.Contains(position));

        if (!bestProbabilityTargets.Any())
            return -1;

        var shotOptions = bestProbabilityTargets
            .SelectMany(position => EvaluateShotSelectionOptions(position, true, []))
            .OrderByDescending(position => position.totalProbabilityScore);

        if (!shotOptions.Any())
            return -1;

        var bestShot = shotOptions.First();

        shotType = bestShot.shotType;
        return bestShot.position;
    }

    /// <summary>
    /// Selects indexes for the highest probability scores from the probability density map. The list is 
    /// limited to five scores to enable further evaluation of possible shots without excessive processing. 
    /// This is used to help select the best targets for the hunt phase based on their probability score.
    /// </summary>
    /// <returns>A list of integer values representing grid positions to target in the hunt phase.</returns>
    private List<int> SelectBestProbabilityTargets()
    {
        var positiveAvailableProbabilities = _probabilityDensityMap
            .Select((score, index) => (Score: score, Index: index))
            .Where(value => value.Score > 0 && _availablePositions.Contains(value.Index));

        return positiveAvailableProbabilities
            .OrderByDescending(value => value.Score)
            .Select(value => value.Index)
            .Take(Math.Min(5, positiveAvailableProbabilities.Count()))
            .ToList();
    }

    /// <summary>
    /// Generates a list of hits on the <paramref name="playerGrid"> excluding sunk ships. Used for selecting 
    /// shots during the target phase.
    /// </summary>
    /// <param name="playerGrid">A <see cref="GridCellState"/> array representing the player board.</param>
    /// <returns>A list of integer values repesenting the positions of any hits on the player board.</returns>
    private static List<int> GenerateHitsList(GridCellState[] playerGrid)
    {
        return playerGrid
            .Select((state, index) => new { state, index })
            .Where(cell => cell.state == GridCellState.Hit)
            .Select(cell => cell.index)
            .ToList();
    }

    /// <summary>
    /// Identifies rows and columns of hits that could be part of the same ship based on finding lists 
    /// of consecutive hits. Used to selected target phase shots by completing lines of hits to find ships.
    /// </summary>
    /// <param name="hits">Integer values representing the positions of any hits on the player board.</param>
    /// <returns>A list of integer lists representing groups of consecutive positions on the player board.</returns>
    private static List<List<int>> IdentifyLinesOfHits(IEnumerable<int> hits)
    {
        var hitsHashSet = hits.ToHashSet();

        var linesOfHits = new List<List<int>>();

        for (int i = 0; i < 10; i++)
        {
            var rows = FindConsecutiveHits(hitsHashSet, i, true);

            if (rows.Count > 0)
                linesOfHits.AddRange(rows);
        }

        for (int i = 0; i < 10; i++)
        {
            var columns = FindConsecutiveHits(hitsHashSet, i, false);

            if (columns.Count > 0)
                linesOfHits.AddRange(columns);
        }

        return linesOfHits;
    }

    /// <summary>
    /// Checks the rows and columns for any consecutive hits to enable targeted shot selection in the 
    /// target phase. Use to find lines of hits to continue to find complete ships in the target phase.
    /// </summary>
    /// <param name="hits">A hash set of integer values representing the grid positions of hits on the 
    /// player board.</param>
    /// <param name="fixedIndex">The integer value of the row or column index that is fixed during this 
    /// loop.</param>
    /// <param name="isRow">A boolean value indicating whether a row or column is being searched.</param>
    /// <returns>A list of integer value lists, each representing a line of consecutive hits.</returns>
    private static List<List<int>> FindConsecutiveHits(HashSet<int> hits, int fixedIndex, bool isRow)
    {
        var lines = new List<List<int>>();
        var possibleLine = new List<int>();

        for (int j = 0; j < 9; j++)
        {
            int currentPosition = isRow ? 10 * fixedIndex + j : 10 * j + fixedIndex;
            int nextPosition = isRow ? currentPosition + 1 : currentPosition + 10;

            if (hits.Contains(currentPosition) && hits.Contains(nextPosition))
            {
                possibleLine.Add(currentPosition);

                if (j == 8) //Deals with the end of the row/column.
                {
                    possibleLine.Add(nextPosition);

                    if (possibleLine.Count > 1)
                        lines.Add([.. possibleLine]);

                    possibleLine.Clear();
                }

                continue;
            }

            if (hits.Contains(currentPosition) && !hits.Contains(nextPosition))
            {
                possibleLine.Add(currentPosition);

                if (possibleLine.Count > 1)
                    lines.Add([.. possibleLine]);

                possibleLine.Clear();
                continue;
            }
        }

        return lines;
    }

    /// <summary>
    /// Selects a grid position to attack in the target phase if a line of hits exists after previous 
    /// attacks. Checks whether the line can be continued by targeting either end or returns a random choice 
    /// if both are possible.
    /// </summary>
    /// <param name="hits">A list of integer values representing the grid positions of hits on the 
    /// player board.</param>
    /// <param name="isRow">A boolean value indicating whether the line of hits forms a row or column.</param>
    /// <returns>An integer value repesenting the grid position of the selected shot. Returns -1 if no 
    /// valid shot can be found.</returns>
    /// <remarks>The calling method will need to deal with the failure to find a valid shot if this method 
    /// returns -1.</remarks>
    private int SelectTargetPhaseShotFromLineOfHits(List<int> hits, out bool isRow)
    {
        isRow = true;

        var linesOfHits = IdentifyLinesOfHits(hits);

        if (linesOfHits.Count == 0)
            return -1;

        var bestLine = linesOfHits
            .OrderByDescending(line => line.Count)
            .First();

        if (bestLine.Count < 2)
            return -1;

        int firstElement = bestLine.First();
        int lastElement = bestLine.Last();

        if (firstElement % 10 == lastElement % 10)
            isRow = false;

        int offset = isRow ? 1 : 10;

        bool beforeValid = IsValidTarget(firstElement, isRow, true);
        bool afterValid = IsValidTarget(lastElement, isRow, false);

        if (!beforeValid && !afterValid)
            return -1;

        if (beforeValid && !afterValid)
            return firstElement - offset;

        if (!beforeValid && afterValid)
            return lastElement + offset;

        var selectElementBefore = RandomProvider.Instance.Next(2) == 0;

        return selectElementBefore ? firstElement - offset : lastElement + offset;

    }

    /// <summary>
    /// Checks that a cell before of after a line of hits is available and in bounds.
    /// </summary>
    /// <param name="gridPosition">An integer value representing the grid position of a hit.</param>
    /// <param name="isRow">A boolean value indicating whether the cell to be tested forms a row with 
    /// the hit.</param>
    /// <param name="isBefore">A boolean value indicating whether the cell to be test comes before the 
    /// hit in the grid.</param>
    /// <returns>A boolean value indicating whether the tested cell is a valid target.</returns>
    private bool IsValidTarget(int gridPosition, bool isRow, bool isBefore)
    {
        int offset = isRow ? 1 : 10;
        int testPosition = isBefore ? gridPosition - offset : gridPosition + offset;

        if (!_availablePositions.Contains(testPosition))
            return false;

        if (isRow && gridPosition % 10 == (isBefore ? 0 : 9))
            return false;

        if (!isRow && (isBefore ? gridPosition < 10 : gridPosition >= 90))
            return false;

        return true;
    }

    /// <summary>
    /// Selects up to five grid positions adjacent to a hit with the best probability score on the probability 
    /// density map. Used to evaluate possible shots adjacent to any hits during the target phase of the 
    /// game to enable the computer player to find full ships more effectively.
    /// </summary>
    /// <param name="hitsList">A list of integer values representing the grid positions of any hits 
    /// on the player board.</param>
    /// <returns>A list of tuples describing the integer value for the grid position and the boolean 
    /// value indicating whether the cell forms a row with the hit.</returns>
    private List<(int gridPosition, bool isRow)> SelectBestHitAdjacentTargetShots(List<int> hitsList)
    {
        var hitAdjacentShots = hitsList
            .SelectMany(hit => GenerateValidHitAdjacentShots(hit))
            .Where(shot => shot.gridPosition >= 0 && _availablePositions.Contains(shot.gridPosition))
            .Distinct()
            .ToList();

        if (hitAdjacentShots.Count == 0)
            return hitAdjacentShots;

        var returnLength = Math.Min(5, hitsList.Count);

        return hitAdjacentShots
            .OrderByDescending(shot => _probabilityDensityMap[shot.gridPosition])
            .Take(returnLength)
            .ToList();
    }

    /// <summary>
    /// Generates a distinct list of cells next to a hit that remain in bounds and are still available.
    /// </summary>
    /// <param name="gridPosition">An integer value repsenting the position of a hit.</param>
    /// <returns>A selection of tuples describing the grid position of each hit adjacent cell and a 
    /// boolean value indicating whether it forms a row with the hit.</returns>
    private IEnumerable<(int gridPosition, bool isRow)> GenerateValidHitAdjacentShots(int gridPosition)
    {
        int[] offsets = [1, -1, 10, -10];

        return offsets
            .Select(offset => GenerateValidHitAdjacentShot(gridPosition, offset))
            .Where(shot => _availablePositions.Contains(shot.gridPosition))
            .Distinct();
    }

    /// <summary>
    /// Generates a valid shot adjacent to a hit based on the offset from the hit. This determines whether 
    /// the shot forms a row or column with the hit.
    /// </summary>
    /// <param name="gridPosition">An integer value representing the grid position.</param>
    /// <param name="offset">An integer value representing the offset from the hit.</param>
    /// <returns>A tuple describing the grid position of the shot and its offset from the hit.</returns>
    private static (int gridPosition, bool isRow) GenerateValidHitAdjacentShot(int gridPosition, int offset)
    {
        bool isRow = (Math.Abs(offset) == 1);

        bool isValid = isRow
            ? (offset > 0 && gridPosition % 10 < 9) || (offset < 0 && gridPosition % 10 > 0)
            : (offset > 0 && gridPosition < 90) || (offset < 0 && gridPosition >= 10);

        return isValid ? (gridPosition + offset, isRow) : (-1, isRow);
    }

    /// <summary>
    /// Generates information for possible shots including a targeted grid position based on whether or 
    /// not the shot is in a row or column with adjacent cells (particularly for targeting hit adjacent 
    /// cells). Used to select the best shot option when targeting a particular cell.
    /// </summary>
    /// <param name="gridPosition">An integer value representing the targeted posittion.</param>
    /// <param name="isRow">A boolean value indicating whether the grid position forms a row or column with 
    /// any adjacent hits it is targeting.</param>
    /// <param name="hitAdjacentCells">A list of integer values representing the position of any hit 
    /// adjacent cells on the player board.</param>
    /// <returns>A list of tuples containing key information about the attack: the key grid position, the 
    /// type of shot, the total number of hit adjacent cells targeted and the total probability score 
    /// for the shot.</returns>
    private List<(int position, ShotType shotType, int totalHitAdjacent, int totalProbabilityScore)>
        EvaluateShotSelectionOptions(int gridPosition, bool isRow, List<int> hitAdjacentCells)
    {
        var scoredShotOptions = new List<(int position, ShotType shotType, int totalHitAdjacent, int totalProbabilityScore)>
        {
            (gridPosition, ShotType.Single, 1, _probabilityDensityMap[gridPosition])
        };

        if (AirstrikeActivated)
            scoredShotOptions.AddRange(GenerateAirstrikeOptions(gridPosition, hitAdjacentCells));

        if (BombardmentActivated)
            scoredShotOptions.AddRange(GenerateBombardmentOptions(gridPosition, isRow, hitAdjacentCells));

        int returnSize = Math.Min(5, scoredShotOptions.Count);

        return scoredShotOptions
            .OrderByDescending(shot => shot.totalHitAdjacent)
            .ThenBy(shot => shot.totalProbabilityScore)
            .Take(returnSize)
            .ToList();
    }

    /// <summary>
    /// Generates information for possible airstrike shots including a targeted grid position. Used to 
    /// evaluate the best shot option targeting a particular cell. Includes both types of airstrike shot.
    /// </summary>
    /// <param name="gridPosition">An integer value representing the targeted posittion.</param>
    /// <param name="hitAdjacentCells">A list of integer values representing the position of any hit 
    /// adjacent cells on the player board.</param>
    /// <returns>A list of tuples containing key information about the attack: the key grid position, the 
    /// type of shot, the total number of hit adjacent cells targeted and the total probability score 
    /// for the shot.</returns>
    private List<(int position, ShotType shotType, int totalHitAdjacent, int totalProbabilityScore)>
        GenerateAirstrikeOptions(int gridPosition, List<int> hitAdjacentCells)
    {
        var airstrikeOptions = new List<(int position, ShotType shotType, int totalHitAdjacent, int totalProbabilityScore)>();

        var offsetsUpRight = new int[] { 0, 9, 18 }
            .Select(offset => gridPosition + offset)
            .Where(position => _availablePositions.Contains(position))
            .Where(position => position % 10 < 8 && position >= 20)
            .ToList();
        var offsetsDownRight = new int[] { -22, -11, 0 }
            .Select(offset => gridPosition + offset)
            .Where(position => _availablePositions.Contains(position))
            .Where(position => position % 10 < 8 && position < 80)
            .ToList();

        if (offsetsUpRight.Count == 0 && offsetsDownRight.Count == 0)
            return [];

        var deltasUpRight = _shotDeltas.GetValueOrDefault(ShotType.AirstrikeUpRight, [0]);
        var deltasDownRight = _shotDeltas.GetValueOrDefault(ShotType.AirstrikeDownRight, [0]);

        var shotPositionsUpRight = offsetsUpRight
            .Select(position => deltasUpRight.Select(d => position + d).ToList())
            .Where(list => !list.Except(_availablePositions).Any())
            .ToList();
        var shotPositionsDownRight = offsetsDownRight
            .Select(position => deltasDownRight.Select(d => position + d).ToList())
            .Where(list => !list.Except(_availablePositions).Any())
            .ToList();

        airstrikeOptions.AddRange(GenerateShotOptionEntries(shotPositionsUpRight, ShotType.AirstrikeUpRight, hitAdjacentCells));
        airstrikeOptions.AddRange(GenerateShotOptionEntries(shotPositionsDownRight, ShotType.AirstrikeDownRight, hitAdjacentCells));

        return airstrikeOptions;
    }

    /// <summary>
    /// Generates information for possible bombardment shots including a targeted grid position based on 
    /// whether or not the bombardment shot is in a row or column with adjacent cells (particularly for 
    /// targeting hit adjacent cells). Used to evaluate the best shot option targeting a particular cell.
    /// </summary>
    /// <param name="gridPosition">An integer value representing the targeted posittion.</param>
    /// <param name="isRow">A boolean value indicating whether the grid position forms a row or column with 
    /// any adjacent hits it is targeting.</param>
    /// <param name="hitAdjacentCells">A list of integer values representing the position of any hit 
    /// adjacent cells on the player board.</param>
    /// <returns>A list of tuples containing key information about the attack: the key grid position, the 
    /// type of shot, the total number of hit adjacent cells targeted and the total probability score 
    /// for the shot.</returns>
    private List<(int position, ShotType shotType, int totalHitAdjacent, int totalProbabilityScore)>
        GenerateBombardmentOptions(int gridPosition, bool isRow, List<int> hitAdjacentCells)
    {
        var bombardmentOptions = new List<(int position, ShotType shotType, int totalHitAdjacent, int totalProbabilityScore)>();

        //If the line is a row, the bombardment should try the row above and below to fit around the hit.
        var step = isRow ? 10 : 1;

        var offsets = new int[] { -step, 0, step }
            .Select(offset => gridPosition + offset)
            .Where(position => _availablePositions.Contains(position))
            .Where(position => position % 10 < 9 && position % 10 > 0 && position >= 10 && position < 90)
            .ToList();

        var deltas = _shotDeltas.GetValueOrDefault(ShotType.Bombardment, [0]);

        var shotPositions = offsets
            .Select(position => deltas.Select(d => position + d).ToList())
            .Where(list => !list.Except(_availablePositions).Any())
            .ToList();

        return GenerateShotOptionEntries(shotPositions, ShotType.Bombardment, hitAdjacentCells);
    }

    /// <summary>
    /// Creates a list of possible shots with relevant scores: the target position, the shot type, the total 
    /// number of hit adjacent cells hit by the shot and the total probability score for the cells hit. This 
    /// is used to help select the best shot to play, including multiple target shots, in both the target 
    /// and hunt phases.
    /// </summary>
    /// <param name="positions">A list containing lists of integers representing the grid positions hit 
    /// by that particular shot.</param>
    /// <param name="shotType">The type of shot represented by each of the lists of integers in the 
    /// <paramref name="positions"/> parameter.</param>
    /// <param name="hitAdjacentCells">A list of integers representing the cells adjacent to a hit 
    /// on the player board.</param>
    /// <returns>A list of tuples containing the key position, the shot type, the total hit adjacent cells 
    /// targeted and the total probability score for each shot evaluated.</returns>
    private List<(int position, ShotType shotType, int totalHitAdjacent, int totalProbabilityScore)>
        GenerateShotOptionEntries(List<List<int>> positions, ShotType shotType, List<int> hitAdjacentCells)
    {
        var scoredOptions = new List<(int position, ShotType shotType, int totalHitAdjacent, int totalProbabilityScore)>();


        foreach (var list in positions)
        {
            int totalHitAdjacent = list.Intersect(hitAdjacentCells).Count();
            int totalProbabilityScore = list.Select(position => _probabilityDensityMap[position]).Sum();

            scoredOptions.Add((list.First(), shotType, totalHitAdjacent, totalProbabilityScore));
        }

        return scoredOptions;
    }

    private void RemoveAllTargetedCells(int gridPosition, ShotType shotType)
    {
        var deltas = _shotDeltas.GetValueOrDefault(shotType, [0]);

        foreach (var delta in deltas)
            if (!_availablePositions.Remove(gridPosition + delta))
                throw new InvalidOperationException($"Targeted position {gridPosition + delta} could not be removed from _availablePositions.");
    }

    #endregion //Shot Selection Helper Methods

    #region Random Shot Helper Methods
    /// <summary>
    /// Checks for hit ships that are not yet sunk and plays a random shot adjacent to the hit. If there is 
    /// not a possible shot, returns -1. Contains a <see cref="RandomProvider"/> instance to ensure that 
    /// the shot is only selected 50% of the time. Updates the <see cref="RandomShotPicker"/> to ensure 
    /// that it can still provide fully validated random shots.
    /// </summary>
    /// <param name="playerGrid">A <see cref="GridCellState"/> array representing the state of the 
    /// player game board.</param>
    /// <param name="targetProbabilityWeighting">An integer representing a weighting against following 
    /// a target strategy if a hit is found. 2 would give a 50% chance. 10 would give a 10% chance.</param>
    /// <param name="shotType">A <see cref="ShotType"/> value representing the generated type of shot. This 
    /// is returned using the out keyword to enable easy use in the move generation methods.</param>
    /// <returns>An integer value representing the grid position of the valid target phase move. 
    /// Returns -1 if no target phase shot can be created.</returns>
    private int GenerateRandomTargetPhaseShot(GridCellState[] playerGrid, int targetProbabilityWeighting, out ShotType shotType)
    {
        var gridPosition = Array.FindIndex(playerGrid, state => state == GridCellState.Hit);

        if (gridPosition != -1 && RandomProvider.Instance.Next(targetProbabilityWeighting) == 0) //Adds randomness into the decision to adopt a target approach
        {
            int row = gridPosition / 10;
            int column = gridPosition % 10;

            shotType = ShotType.Single;

            var possibleShots = new List<int>();

            if (column > 0)
                possibleShots.Add(gridPosition - 1);
            if (column < 9)
                possibleShots.Add(gridPosition + 1);
            if (row > 0)
                possibleShots.Add(gridPosition - 10);
            if (row < 9)
                possibleShots.Add(gridPosition + 10);

            possibleShots = possibleShots.Where(shot => _availablePositions.Contains(shot)).ToList();

            if (possibleShots.Count == 0)
                return -1;

            var index = RandomProvider.Instance.Next(possibleShots.Count);
            gridPosition = possibleShots[index];

            _randomShotPicker.UpdateShotPickerLists(gridPosition, shotType);
            return gridPosition;
        }

        shotType = ShotType.Single;
        return -1;
    }

    /// <summary>
    /// Selects a random shot type from the available shot types to enable less predictable computer shot 
    /// selection. It includes the option for weighting towards single shot selection.
    /// </summary>
    /// <param name="weightingFactor">An integer representing a weighting towards the single shot type. 
    /// A weighting of 1 gives each shot an equal chance. A weighting of 10 leads to an additional 90% 
    /// of the options being single shot.</param>
    /// <returns>A randomized and available <see cref="ShotType"/> for the computer to play.</returns>
    private ShotType GenerateRandomShotType(int weightingFactor)
    {
        List<ShotType> possibleShotTypes = [ShotType.Single];

        if (AirstrikeActivated)
            possibleShotTypes.AddRange([ShotType.AirstrikeDownRight, ShotType.AirstrikeUpRight]);
        if (BombardmentActivated)
            possibleShotTypes.Add(ShotType.Bombardment);

        var index = RandomProvider.Instance.Next(possibleShotTypes.Count * weightingFactor);

        if (index >= possibleShotTypes.Count)
            index = 0;

        return possibleShotTypes[index];
    }

    /// <summary>
    /// Checks which multiple target shot types are available and selects one to play with equal probability 
    /// of bombardment or airstrike. Used to add a random selection element into the computer player choices.
    /// </summary>
    /// <returns>A <see cref="ShotType"/> value for the selected shot. If no multiple target shots are 
    /// available, returns a single shot.</returns>
    private ShotType SelectMultipleTargetShotType()
    {
        List<ShotType> possibleShotTypes = (BombardmentActivated) ? [ShotType.Bombardment] : [];

        if (AirstrikeActivated)
            possibleShotTypes.AddRange([ShotType.AirstrikeDownRight, ShotType.AirstrikeUpRight]);
        if (BombardmentActivated)
            possibleShotTypes.Add(ShotType.Bombardment);

        var length = possibleShotTypes.Count;

        if (length == 0)
            return ShotType.Single;

        var index = RandomProvider.Instance.Next(length);

        return possibleShotTypes[index];
    }

    #endregion //Random Shot Helper Methods

    #region Probability Density Methods

    /// <summary>
    /// Updates stored values based on the <see cref="SingleTurnReport"/> containing information from the 
    /// last computer move, an updated player grid and a list of the remaining ship sizes. Ensures that the 
    /// probability density map is kept up to date.
    /// </summary>
    private void ProcessAttackOutcomes(List<int> remainingShipSizes, SingleTurnReport lastMoveReport)
    {
        _maximumShipSize = remainingShipSizes.Max();
        _airstrikeHitCount += lastMoveReport.PositionsHit?.Count ?? 0;
        _bombardmentHitCount += lastMoveReport.PositionsHit?.Count ?? 0;
        UpdateProbabilityDensityMap(lastMoveReport);
    }

    /// <summary>
    /// Creates a new integer array with all values set to 100 to represent an equal chance of each grid 
    /// position being hit at the start of the game. Balances the greater number of possible ship placements 
    /// in the center of the grid against the human strategy of placing pieces in less likely positions.
    /// </summary>
    /// <returns>An integer array representing the starting probability values for each grid position.</returns>
    private static int[] SetUpProbabilityDensityMap()
    {
        var newMap = new int[100];

        for (int i = 0; i < 100; i++)
            newMap[i] = 100;

        return newMap;
    }

    /// <summary>
    /// Updates the probability density map based on information from the last turn. Uses private methods to 
    /// implement probability value changes near hit or missed positions and around sunken ships. Ensures 
    /// that the probability value for hit, miss and sunk positions is set to zero to avoid these being 
    /// chosen again.
    /// </summary>
    /// <param name="playerBoard">A <see cref="GridCellState"/> array containing information about hits, 
    /// misses and sinkings on the player's board.</param>
    /// <param name="lastMoveReport">A <see cref="SingleTurnReport"/> containing relevant information 
    /// from the last computer shot.</param>
    internal void UpdateProbabilityDensityMap(SingleTurnReport lastMoveReport)
    {
        List<int> hits = lastMoveReport.PositionsHit?.ToList() ?? [];
        List<int> misses = lastMoveReport.PositionsMissed?.ToList() ?? [];
        List<(int, bool, ShipType)> sinkings = lastMoveReport.ShipsSunk?.ToList() ?? [];

        foreach (var position in hits)
            AdjustMapOnHit(position);

        foreach (var position in misses)
            AdjustMapOnMiss(position);

        foreach (var ship in sinkings)
            AdjustMapOnSinking(ship);

        //Ensure that hit and miss positions are set to 0 probability
        //Sunk positions are set in AdjustMapOnSinking
        foreach (var position in hits)
            _probabilityDensityMap[position] = 0;

        foreach (var position in misses)
            _probabilityDensityMap[position] = 0;
    }

    /// <summary>
    /// Adjusts the probability of affected grid positions around a hit. Increases the probability value 
    /// to reflect a higher chance of a ship being near a hit.
    /// </summary>
    /// <param name="gridPosition">An integer representing the grid position of the hit.</param>
    private void AdjustMapOnHit(int gridPosition)
    {
        foreach (var direction in _directions)
            AdjustProbability(gridPosition, direction, _maximumShipSize, 5);
    }

    /// <summary>
    /// Asjusts the probability of affected grid positions around a missed shot. Reduces the probability 
    /// value to reflect a lower chance of a ship being near a miss.
    /// </summary>
    /// <param name="gridPosition">An integer representing the grid position of the miss.</param>
    internal void AdjustMapOnMiss(int gridPosition)
    {
        foreach (var direction in _directions)
            AdjustProbability(gridPosition, direction, _maximumShipSize, -5);
    }

    /// <summary>
    /// Adjusts the probability of affected grid positions around a sunk ship to update the probability map.
    /// </summary>
    /// <param name="shipInformation">Contains an integer value representing the starting grid position for 
    /// the sunk ship, a boolean value indicating whwther the ship was horizontal and the type of ship. 
    /// Enables all of the grid positions containing the sunken ship to be located.</param>
    internal void AdjustMapOnSinking((int gridPosition, bool isHorizontal, ShipType shipType) shipInformation)
    {
        var sunkShipPositions = new List<int>();
        var size = _shipSizes[shipInformation.shipType];

        for (int i = 0; i < size; i++)
        {
            var newPosition = (shipInformation.isHorizontal)
                 ? shipInformation.gridPosition + i
                 : shipInformation.gridPosition + 10 * i;

            sunkShipPositions.Add(newPosition);
        }

        foreach (var position in sunkShipPositions)
        {
            foreach (var direction in _directions)
                AdjustProbability(position, direction, 3, -3);
        }

        if (!_shipsCanTouch)
        {
            foreach (var position in sunkShipPositions)
            {
                foreach (var direction in _directions)
                    SetProbability(position, direction, 2, 0);
            }
        }

        foreach (var position in sunkShipPositions)
            _probabilityDensityMap[position] = 0;
    }

    /// <summary>
    /// Adjusts the probability of the selected grid positions by the given amount.
    /// </summary>
    /// <param name="startPosition">An integer representing the starting position.</param>
    /// <param name="direction">An integer representing the change in grid position for each step.</param>
    /// <param name="limit">An integer representing how many grid positions should be set.</param>
    /// <param name="probabilityChange">An integer representing the selected change in each probability.</param>
    private void AdjustProbability(int startPosition, int direction, int limit, int probabilityChange)
    {
        for (int i = 1; i < limit; i++)
        {
            int newPosition = startPosition + i * direction;

            if (newPosition < 0 || newPosition > 99)
                return;

            if (Math.Abs(direction) == 1 && startPosition / 10 != newPosition / 10)
                return;

            var adjustment = (probabilityChange > 0) ? probabilityChange - i : probabilityChange + i;

            if (_probabilityDensityMap[newPosition] > 0)
                _probabilityDensityMap[newPosition] += adjustment;
        }
    }

    /// <summary>
    /// Sets the probability of selected grid position to a set value.
    /// </summary>
    /// <param name="startPosition">An integer representing the starting position.</param>
    /// <param name="direction">An integer representing the change in grid position for each step.</param>
    /// <param name="limit">An integer representing how many grid positions should be set.</param>
    /// <param name="newValue">An integer representing the new value for the grid positions.</param>
    private void SetProbability(int startPosition, int direction, int limit, int newValue)
    {
        for (int i = 1; i < limit; i++)
        {
            int newPosition = startPosition + i * direction;

            if (newPosition < 0 || newPosition > 99)
                return;

            if (Math.Abs(direction) == 1 && startPosition / 10 != newPosition / 10)
                return;

            if (_probabilityDensityMap[newPosition] > 0)
                _probabilityDensityMap[newPosition] = newValue;
        }
    }
    #endregion //Probability Density Methods

    public ComputerPlayerDTO GetDTO()
    {
        var computerPlayerDTO = new ComputerPlayerDTO()
        {
            RandomShotPickerDTO = _randomShotPicker.GetDTO(),
            GameDifficulty = _gameDifficulty,
            ShipsCanTouch = _shipsCanTouch,
            ProbabilityDensityMap = _probabilityDensityMap,
            Directions = _directions,
            AvailablePositions = _availablePositions,
            AirstrikeAllowed = _airstrikeAllowed,
            BombardmentAllowed = _bombardmentAllowed,
            AirstrikeHitCount = _airstrikeHitCount,
            BombardmentHitCount = _bombardmentHitCount,
            MaximumShipSize = _maximumShipSize,
            CompletedShots = _completedShots
        };

        return computerPlayerDTO;
    }
}
