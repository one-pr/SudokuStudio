namespace Sudoku.Analytics.Dependency.Contradictions;

/// <summary>
/// Represents a type that can find for conflict that will be used in complex patterns.
/// </summary>
public static partial class ContradictionDetector
{
	/// <summary>
	/// The global method to check for complex contradiction.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="includesGroupedNodes">Indicates whether grouped nodes should also be checked.</param>
	/// <returns>A list of found conflict.</returns>
	public static ReadOnlySpan<Cause> Check(in Grid grid, bool includesGroupedNodes)
	{
		var result = new List<Cause>();
		if (!grid.IsValid)
		{
			return result.AsSpan();
		}

		var solution = grid.SolutionGrid;

		// Iterate on each empty cell.
		foreach (var cell in grid.EmptyCells)
		{
			// Iterate on each wrong digit.
			foreach (var digit in (Mask)(grid.GetCandidates(cell) & ~(1 << solution.GetDigit(cell))))
			{
				// Do a fast check, to know whether the candidate is worth to be an elimination or not.
				if (DoFastTryAndError(grid, cell, digit, includesGroupedNodes)
					&& LeadsToEmpty(grid, cell, digit, includesGroupedNodes, out var lastNode, out var cause))
				{
					result.Add(new(cell * 9 + digit, lastNode, cause));
				}
			}
		}

		// All candidates are checked.
		return result.AsSpan();
	}
}
