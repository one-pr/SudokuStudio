namespace Sudoku.Analytics.Braiding;

using BraidingCellsInfo = (CellMap Containing, CellMap NotContaining);
using BraidingCellsKey = (int ChuteIndex, Digit SequenceIndex, RotationType Type);

/// <summary>
/// Provides an entry to analyze braiding of a chute in a pattern.
/// </summary>
public static class BraidAnalysis
{
	/// <summary>
	/// Represents the map of all rotation patterns, grouped by sequence index (0..3) and type.
	/// </summary>
	private static readonly FrozenDictionary<BraidingCellsKey, BraidingCellsInfo> RotationMap;

	/// <summary>
	/// Indicates top-3 cells defined in the specified chute, sequence index and type.
	/// </summary>
	private static readonly FrozenDictionary<(int ChuteIndex, Digit SequenceIndex), CellMap> TopThreeCellsMap;


	/// <include file='../../global-doc-comments.xml' path='g/static-constructor' />
	static BraidAnalysis()
	{
		var rotationMap = new Dictionary<BraidingCellsKey, BraidingCellsInfo>();
		var topThreeCellsMap = new Dictionary<(int ChuteIndex, Digit SequenceIndex), CellMap>();

		// Iterate on each chute.
		foreach (var (index, _, housesMask) in Chute.Chutes)
		{
			// Get three houses of the chute.
			var house1 = BitOperations.TrailingZeroCount(housesMask);
			var house2 = housesMask.GetNextSet(house1);
			var house3 = housesMask.GetNextSet(house2);

			// Starts with the specified segment.
			for (var sequenceIndex = 0; sequenceIndex < 3; sequenceIndex++)
			{
				// Try to get the first 3 cells from the top-left segment.
				ref readonly var cellsFromHouse1 = ref HousesMap[sequenceIndex switch { 0 => house1, 1 => house2, _ => house3 }];
				var cells1 = cellsFromHouse1[..3];
				var otherCells1 = cellsFromHouse1 & ~cells1;
				topThreeCellsMap.Add((index, sequenceIndex), cells1);

				// Then do rotate-shifting with N or Z mode.
				foreach (var mode in (RotationType.Downside, RotationType.Upside))
				{
					// Get the second segment.
					ref readonly var cellsFromHouse2 = ref HousesMap[
						(sequenceIndex, mode) switch
						{
							(0, RotationType.Downside) => house2,
							(0, _) => house3,
							(1, RotationType.Downside) => house3,
							(1, _) => house1,
							(2, RotationType.Downside) => house1,
							_ => house2
						}
					];
					var cells2 = cellsFromHouse2[3..6];
					var otherCells2 = cellsFromHouse2 & ~cells2;

					// Get the third segment.
					ref readonly var cellsFromHouse3 = ref HousesMap[
						(sequenceIndex, mode) switch
						{
							(0, RotationType.Downside) => house3,
							(0, _) => house2,
							(1, RotationType.Downside) => house1,
							(1, _) => house3,
							(2, RotationType.Downside) => house2,
							_ => house1
						}
					];
					var cells3 = cellsFromHouse3[6..];
					var otherCells3 = cellsFromHouse3 & ~cells3;

					// Merge them up.
					var cellsFromChute = cells1 | cells2 | cells3;
					var otherCellsFromChute = otherCells1 | otherCells2 | otherCells3;

					// Add value into the dictionary.
					var tuple = (index, sequenceIndex, mode);
					rotationMap.Add(tuple, (cellsFromChute, otherCellsFromChute));
				}
			}
		}

		RotationMap = rotationMap.ToFrozenDictionary();
		TopThreeCellsMap = topThreeCellsMap.ToFrozenDictionary();
	}


	/// <summary>
	/// Get cells at the specified chute, sequence index and type.
	/// </summary>
	/// <param name="chuteIndex">The chute index (0..6).</param>
	/// <param name="sequenceIndex">The sequence index (0..3).</param>
	/// <param name="type">The type.</param>
	/// <returns>A pair of cells indicating the result.</returns>
	public static ref readonly BraidingCellsInfo GetCellsAt(int chuteIndex, Digit sequenceIndex, RotationType type)
		=> ref RotationMap[(chuteIndex, sequenceIndex, type)];

	/// <summary>
	/// Gets the pattern type of three digits in the specified chute.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="chuteIndex">The chute (0..6).</param>
	/// <param name="sequenceIndex">The sequence index (0..3).</param>
	/// <returns>The first three digits from the segment, specified as <paramref name="sequenceIndex"/>.</returns>
	/// <exception cref="ArgumentException">Throws when the argument must be solved.</exception>
	public static BraidType GetPattern(in Grid grid, int chuteIndex, int sequenceIndex)
	{
		ArgumentException.Assert(grid.IsSolved);

		ref readonly var topThreeCells = ref TopThreeCellsMap[(chuteIndex, sequenceIndex)];
		var valuesMap = grid.ValuesMap;

		var result = new List<RotationType>(3);

		// Iterate on each cell.
		foreach (var cell in topThreeCells)
		{
			var digit = grid.GetDigit(cell);

			// Check for two types of rotation.
			foreach (var type in (RotationType.Downside, RotationType.Upside))
			{
				var tuple = (chuteIndex, sequenceIndex, type);
				ref readonly var cells = ref RotationMap[tuple].Containing;
				if ((valuesMap[digit] & cells).Count == 3)
				{
					// Valid.
					result.Add(type);
					break;
				}
			}
		}
		return BraidType.Create(result[0], result[1], result[2]);
	}
}
