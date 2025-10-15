namespace Sudoku.SetTheory;

/// <summary>
/// Provides a list of members to get pattern detailed information that should be inferred.
/// </summary>
public static class PatternReasoner
{
	/// <summary>
	/// Gets rank of specified elimination. The rank of elimination is defined as <c>n(links) - n(lightup_links)</c>.
	/// </summary>
	/// <param name="pattern">The pattern.</param>
	/// <param name="candidate">The candidate.</param>
	/// <returns>The rank of elimination. -1 will be returned if candidate is not an eliminiation.</returns>
	public static int GetEliminationRank(in Pattern pattern, Candidate candidate)
	{
		ref readonly var links = ref pattern.Links;
		var (maxOccupied, minOccupied) = (0, links.Count);
		foreach (var permutation in GetPermutations(pattern))
		{
			var lightupLinks = SpaceSet.Empty;
			foreach (var assigned in permutation)
			{
				var cell = assigned / 9;
				var digit = assigned % 9;
				var cellLink = Space.RowColumn(cell / 9, cell % 9);
				if (links.Contains(cellLink))
				{
					lightupLinks += cellLink;
				}

				var blockLink = Space.BlockDigit(cell >> HouseType.Block, digit);
				if (links.Contains(blockLink))
				{
					lightupLinks += blockLink;
				}

				var rowLink = Space.RowDigit((cell >> HouseType.Row) - 9, digit);
				if (links.Contains(rowLink))
				{
					lightupLinks += rowLink;
				}

				var columnLink = Space.ColumnDigit((cell >> HouseType.Column) - 18, digit);
				if (links.Contains(columnLink))
				{
					lightupLinks += columnLink;
				}
			}

			var occupied = lightupLinks.Count;
			if (occupied >= maxOccupied)
			{
				maxOccupied = occupied;
			}
			if (occupied <= minOccupied)
			{
				minOccupied = occupied;
			}
		}
		return maxOccupied - minOccupied;
	}

	/// <summary>
	/// <para>
	/// Gets the number of assigned candidates that can make a pattern satisfied with all truths and links.
	/// </para>
	/// <para>
	/// Please note that the return value may not be a stable number
	/// because sometimes the pattern may not be stable always.
	/// For example, if a pattern produces multiple eliminations from different sub-patterns,
	/// the result may uses different number of assignments to satisfy all sets (truths and links).
	/// Please check type <see cref="AssignmentCountRange"/> to learn more details of result.
	/// </para>
	/// </summary>
	/// <param name="pattern">The pattern.</param>
	/// <returns>The permutation count value.</returns>
	/// <seealso cref="AssignmentCountRange"/>
	public static AssignmentCountRange GetAssignmentsCount(in Pattern pattern)
	{
		var (min, max) = (int.MaxValue, int.MinValue);
		foreach (var permutation in GetPermutations(pattern))
		{
			var count = permutation.Assignments.Length;
			if (count <= min)
			{
				min = count;
			}
			if (count >= max)
			{
				max = count;
			}
		}
		return (min, max) is ( >= 0, >= 0) ? new(min, max) : new();
	}

	/// <summary>
	/// Try to find all possible permutations.
	/// </summary>
	/// <param name="pattern">The pattern.</param>
	/// <returns>The permutations.</returns>
	public static ReadOnlySpan<Permutation> GetPermutations(in Pattern pattern) => SetSolver.Solve(pattern);

	/// <summary>
	/// Try to find all conclusions.
	/// </summary>
	/// <param name="pattern">The pattern.</param>
	/// <returns>All conclusions.</returns>
	public static ReadOnlySpan<Conclusion> GetConclusions(in Pattern pattern)
	{
		ref readonly var grid = ref pattern.Grid;
		ref readonly var fullMap = ref pattern.FullMap;
		var candidatesMap = grid.CandidatesMap;

		var result = ConclusionSet.Empty;
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
					if (fullMap.Contains(c * 9 + digit))
					{
						tempConclusions.Add(new(Elimination, c, digit));
					}
				}
				foreach (var d in (Mask)(grid.GetCandidates(cell) & ~(1 << digit)))
				{
					if (fullMap.Contains(cell * 9 + d))
					{
						tempConclusions.Add(new(Elimination, cell, d));
					}
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

	/// <summary>
	/// Gets all rank-0 eliminations.
	/// </summary>
	/// <param name="pattern">The pattern.</param>
	/// <returns>All rank-0 eliminations.</returns>
	public static CandidateMap GetRank0Eliminations(in Pattern pattern)
	{
		var conclusions = GetConclusions(pattern);
		var result = CandidateMap.Empty;
		foreach (var (type, candidate) in conclusions)
		{
			if (type != Elimination)
			{
				continue;
			}

			// TODO: construct a cache. Unnecessary to calculate permutations twice.
			foreach (var link in GetRank0Links(pattern))
			{
				if (link.Contains(candidate))
				{
					result.Add(candidate);
				}
			}
		}
		return result;
	}
}
