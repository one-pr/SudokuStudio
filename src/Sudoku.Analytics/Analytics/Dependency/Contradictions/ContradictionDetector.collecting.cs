namespace Sudoku.Analytics.Dependency.Contradictions;

public partial class ContradictionDetector
{
	/// <summary>
	/// Find for hidden singles, and collect them into <paramref name="result"/>.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="result">The result.</param>
	private static void FindHiddenSingle(in Grid grid, HashSet<(DependencyAssignment, DependencyNodeType)> result)
	{
		// Iterate on each digit.
		for (var digit = 0; digit < 9; digit++)
		{
			// Iterate on each house.
			for (var house = 0; house < 27; house++)
			{
				var count = 0;
				var firstFoundCell = -1;
				foreach (var cell in HousesMap[house])
				{
					var cellState = grid.GetState(cell);
					if (grid.GetDigit(cell) == digit)
					{
						goto NextHouse;
					}
					if (grid.Exists(cell, digit) is true)
					{
						firstFoundCell = cell;
						if (++count >= 2)
						{
							break;
						}
					}
				}

				if (count == 1)
				{
					result.Add(
						(
							new(firstFoundCell * 9 + digit),
							house switch
							{
								< 9 => DependencyNodeType.Block,
								< 18 => DependencyNodeType.Row,
								_ => DependencyNodeType.Column
							}
						)
					);
				}

			NextHouse:;
			}
		}
	}

	/// <summary>
	/// Find for naked singles, and collect them into <paramref name="result"/>.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="result">The result.</param>
	private static void FindNakedSingle(in Grid grid, HashSet<(DependencyAssignment, DependencyNodeType)> result)
	{
		foreach (var cell in grid.EmptyCells)
		{
			var mask = grid.GetCandidates(cell);
			if (BitOperations.IsPow2((uint)mask))
			{
				result.Add((new(cell * 9 + BitOperations.Log2((uint)mask)), DependencyNodeType.Cell));
			}
		}
	}

	/// <summary>
	/// Find for locked candidates, and collect them into <paramref name="result"/>.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="result">The result.</param>
	private static void FindLockedCandidates(in Grid grid, HashSet<(DependencyAssignment, DependencyNodeType)> result)
	{
		var emptyCells = grid.EmptyCells;
		var candidatesMap = grid.CandidatesMap;
		foreach (var ((baseSet, coverSet), (a, b, c, _)) in Miniline.Map)
		{
			// Check whether the locked candidates pattern can be formed or not.
			if (!LockedCandidates.IsLockedCandidates(grid, a, b, c, emptyCells, out var digitsMaskFormingLockedCandidates))
			{
				continue;
			}

			// Now iterate on the mask to get all digits.
			foreach (var digit in digitsMaskFormingLockedCandidates)
			{
				ref readonly var map = ref candidatesMap[digit];

				// Check whether the digit contains any eliminations.
				var intersection = c & map;
				if (intersection.Count >= 2)
				{
					// a & map => Cells in lines are not empty => pointing eliminations.
					result.Add(
						(
							new(digit, intersection),
							a & map
								? DependencyNodeType.Block
								: baseSet < 18 ? DependencyNodeType.Row : DependencyNodeType.Column
						)
					);
				}
			}
		}
	}

	/// <summary>
	/// An unsafe entry to method <c>GetHeaderBits</c> defined in type <see cref="Grid"/>.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="cell">The cell.</param>
	/// <returns>The mask of mask bits, unshifted.</returns>
	[UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetHeaderBits")]
	private static extern Mask GetHeaderBits(ref readonly Grid grid, Cell cell);
}
