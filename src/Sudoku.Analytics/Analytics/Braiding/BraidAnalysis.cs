namespace Sudoku.Analytics.Braiding;

/// <summary>
/// Provides an entry to analyze braiding of a chute in a pattern.
/// </summary>
public static class BraidAnalysis
{
	/// <summary>
	/// Indicates top-3 cells defined in the specified chute, sequence index and type.
	/// </summary>
	private static readonly CellMap[] TopThreeCellsMap;

	/// <summary>
	/// Represents the map of all rotation patterns, grouped by sequence index (0..3) and type.
	/// </summary>
	private static readonly FrozenDictionary<Strand, ChuteStrandMap> StrandsMap;


	/// <include file='../../global-doc-comments.xml' path='g/static-constructor' />
	static BraidAnalysis()
	{
		var strandsMap = new Dictionary<Strand, ChuteStrandMap>();
		TopThreeCellsMap = new CellMap[Chute.MaxChuteIndex * 3];

		// Iterate on each chute.
		foreach (var (chuteIndex, _, housesMask) in Chute.Chutes)
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
				var globalIndex = ProjectGlobalIndex(chuteIndex, sequenceIndex);
				TopThreeCellsMap[globalIndex] = cells1;

				// Then do rotate-shifting with N or Z mode.
				foreach (var mode in (StrandType.Downside, StrandType.Upside))
				{
					// Get the second segment.
					ref readonly var cellsFromHouse2 = ref HousesMap[
						(sequenceIndex, mode) switch
						{
							(0, StrandType.Downside) => house2,
							(0, _) => house3,
							(1, StrandType.Downside) => house3,
							(1, _) => house1,
							(2, StrandType.Downside) => house1,
							_ => house2
						}
					];
					var cells2 = cellsFromHouse2[3..6];
					var otherCells2 = cellsFromHouse2 & ~cells2;

					// Get the third segment.
					ref readonly var cellsFromHouse3 = ref HousesMap[
						(sequenceIndex, mode) switch
						{
							(0, StrandType.Downside) => house3,
							(0, _) => house2,
							(1, StrandType.Downside) => house1,
							(1, _) => house3,
							(2, StrandType.Downside) => house2,
							_ => house1
						}
					];
					var cells3 = cellsFromHouse3[6..];
					var otherCells3 = cellsFromHouse3 & ~cells3;

					// Merge them up.
					var otherCellsFromChute = otherCells1 | otherCells2 | otherCells3;

					// Add value into the dictionary.
					strandsMap.Add(new(chuteIndex, sequenceIndex, mode), new([cells1, cells2, cells3], otherCellsFromChute));
				}
			}
		}

		StrandsMap = strandsMap.ToFrozenDictionary();
	}


	/// <summary>
	/// Projects global index from chute index (0..6) and sequence index (0..3).
	/// </summary>
	/// <param name="chuteIndex">The chute index.</param>
	/// <param name="sequenceIndex">The sequence index.</param>
	/// <returns>The global index.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int ProjectGlobalIndex(int chuteIndex, int sequenceIndex)
		=> (chuteIndex / 3 * 3 + chuteIndex % 3) * 3 + sequenceIndex;

	/// <summary>
	/// Get cells at the specified chute, sequence index and type.
	/// </summary>
	/// <param name="chuteIndex">The chute index (0..6).</param>
	/// <param name="sequenceIndex">The sequence index (0..3).</param>
	/// <param name="type">The type.</param>
	/// <returns>The map of the strand.</returns>
	public static ref readonly ChuteStrandMap GetCellsAt(int chuteIndex, Digit sequenceIndex, StrandType type)
		=> ref GetCellsAt(new(chuteIndex, sequenceIndex, type));

	/// <summary>
	/// Get cells at the specified strand.
	/// </summary>
	/// <param name="label">The label of strand.</param>
	/// <returns>The map of the strand.</returns>
	public static ref readonly ChuteStrandMap GetCellsAt(Strand label) => ref StrandsMap[label];

	/// <summary>
	/// Gets the pattern type of three digits in the specified chute.
	/// </summary>
	/// <param name="solutionGrid">The solution to a certain grid.</param>
	/// <param name="chuteIndex">The chute (0..6).</param>
	/// <param name="sequenceIndex">The sequence index (0..3).</param>
	/// <returns>The first three digits from the segment, specified as <paramref name="sequenceIndex"/>.</returns>
	/// <exception cref="ArgumentException">Throws when the argument must be solved.</exception>
	public static BraidType GetPattern(in Grid solutionGrid, int chuteIndex, int sequenceIndex)
	{
		ArgumentException.Assert(solutionGrid.IsSolved);

		var globalIndex = ProjectGlobalIndex(chuteIndex, sequenceIndex);
		ref readonly var topThreeCells = ref TopThreeCellsMap[globalIndex];
		var valuesMap = solutionGrid.ValuesMap;

		var result = new List<StrandType>(3);

		// Iterate on each cell.
		foreach (var cell in topThreeCells)
		{
			var digit = solutionGrid.GetDigit(cell);

			// Check for two types of rotation.
			foreach (var type in (StrandType.Downside, StrandType.Upside))
			{
				var strand = new Strand(chuteIndex, sequenceIndex, type);
				ref readonly var cells = ref StrandsMap[strand].Included;
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

	/// <summary>
	/// Maps all digits in the specified grid that can be categorized as N or Z mode.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <returns>A dictionary of strands and the digits that can be categorized as this strand.</returns>
	public static FrozenDictionary<Strand, Mask> MapStrands(in Grid grid)
	{
		var result = new Dictionary<Strand, Mask>();
		var digitsMap = grid.DigitsMap;
		foreach (ref readonly var strand in Strand.Strands)
		{
			var ((chuteIndex, sequenceIndex, _), mask) = (strand, (Mask)0);
			var includedSegments = StrandsMap[strand].IncludedSegments;

			// Iterate on each digit appeared in this group of cells.
			var globalIndex = ProjectGlobalIndex(chuteIndex, sequenceIndex);
			foreach (var digit in grid[TopThreeCellsMap[globalIndex], true])
			{
				var allSegmentsSatisfied = true;
				foreach (ref readonly var segmentCells in includedSegments)
				{
					if (!(digitsMap[digit] & segmentCells))
					{
						allSegmentsSatisfied = false;
						break;
					}
				}
				if (allSegmentsSatisfied)
				{
					mask |= (Mask)(1 << digit);
				}
			}

			// Add the target mask into dictionary.
			result.Add(strand, mask);
		}
		return result.ToFrozenDictionary();
	}
}
