namespace Sudoku.SetTheory;

/// <summary>
/// Provides a list of members to get pattern detailed information that should be inferred.
/// </summary>
public static class PatternReasoner
{
	/// <summary>
	/// Try to find all possible permutations.
	/// </summary>
	/// <param name="pattern">The pattern.</param>
	/// <returns>The permutations.</returns>
	public static ReadOnlySpan<Permutation> GetPermutations(in Pattern pattern)
		=> SetSolver.Solve(pattern.Grid, pattern.Truths, pattern.Links);

	/// <summary>
	/// Try to find all conclusions.
	/// </summary>
	/// <param name="pattern">The pattern.</param>
	/// <returns>All conclusions.</returns>
	public static ReadOnlySpan<Conclusion> GetConclusions(in Pattern pattern)
	{
		var result = ConclusionSet.Empty;
		ref readonly var grid = ref pattern.Grid;
		var candidatesMap = grid.CandidatesMap;
		var i = 0;
		foreach (var permutation in GetPermutations(pattern))
		{
			var tempConclusions = ConclusionSet.Empty;
			foreach (var candidate in permutation)
			{
				var cell = candidate / 9;
				var digit = candidate % 9;
				foreach (var c in PeersMap[cell] & candidatesMap[digit])
				{
					tempConclusions.Add(new(Elimination, c, digit));
				}
				foreach (var d in (Mask)(grid.GetCandidates(cell) & ~(1 << digit)))
				{
					tempConclusions.Add(new(Elimination, cell, d));
				}
				tempConclusions.Add(new(Assignment, cell, digit));
			}

			if (i++ == 0)
			{
				result |= tempConclusions;
			}
			else
			{
				result &= tempConclusions;
			}
		}
		return result.AsSpan();
	}

	/// <summary>
	/// Gets all rank-0 links.
	/// </summary>
	/// <param name="pattern">The pattern.</param>
	/// <returns>All rank-0 links.</returns>
	public static SpaceSet GetRank0Links(in Pattern pattern)
	{
		var permutations = GetPermutations(pattern);
		var result = pattern.Links;
		foreach (var permutation in permutations)
		{
			var lightUpLinks = SpaceSet.Empty;
			foreach (var candidate in permutation)
			{
				var cell = candidate / 9;
				var digit = candidate % 9;
				lightUpLinks.Add(Space.RowColumn(cell / 9, cell % 9));
				lightUpLinks.Add(Space.BlockDigit(cell >> HouseType.Block, digit));
				lightUpLinks.Add(Space.RowDigit((cell >> HouseType.Row) - 9, digit));
				lightUpLinks.Add(Space.ColumnDigit((cell >> HouseType.Column) - 18, digit));
			}
			result &= lightUpLinks;
		}
		return result;
	}
}
