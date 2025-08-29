namespace Sudoku.Algorithms.Ranking;

public partial struct RankPattern
{
	/// <summary>
	/// Indicates whether the current pattern is stable rank-0 pattern, i.e. all links are rank-0 links.
	/// </summary>
	public bool GetIsRank0Pattern() => GetRank0Links().Count == Truths.Count;

	/// <summary>
	/// Indicates the rank of the current pattern.
	/// </summary>
	public int GetRank() => GetRankCore(GetAssignmentCombinations());

	/// <summary>
	/// Try to find all rank-0 links. A rank-0 link is a link that will become truth
	/// because all valid combinations lead to a same result that the link must hold one correct digit.
	/// </summary>
	/// <returns>A list of links that are determined as rank-0 links.</returns>
	public SpaceSet GetRank0Links() => GetRank0LinksCore(GetAssignmentCombinations());

	/// <summary>
	/// Gets ranks on eliminations.
	/// </summary>
	/// <returns>A lookup table that displays each candidate and its elimination rank.</returns>
	/// <exception cref="InvalidOperationException">Throws when <see cref="Links"/> is not specified.</exception>
	public FrozenDictionary<Candidate, int> GetEliminationRanks() => GetEliminationRanksCore(GetAssignmentCombinations());

	/// <summary>
	/// Determine whether the pattern is rank-0 pattern via cached combinations.
	/// </summary>
	/// <param name="combinations">The combinations.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	private bool GetIsRank0PatternCore(ReadOnlySpan<ReadOnlyMemory<Candidate>> combinations)
		=> GetRank0LinksCore(combinations) == Links;

	/// <summary>
	/// Calculate rank value via cached combinations.
	/// </summary>
	/// <param name="combinations">The combinations.</param>
	/// <returns>The rank value.</returns>
	private int GetRankCore(ReadOnlySpan<ReadOnlyMemory<Candidate>> combinations)
	{
		var factAssignmentCountValues = new SortedSet<int>();
		factAssignmentCountValues.AddRange(from assignment in combinations select assignment.Length);

		return factAssignmentCountValues.Max - factAssignmentCountValues.Min;
	}

	/// <summary>
	/// Gets rank-0 links via cached combinations.
	/// </summary>
	/// <param name="combinations">The combinations.</param>
	/// <returns>Rank-0 links.</returns>
	private SpaceSet GetRank0LinksCore(ReadOnlySpan<ReadOnlyMemory<Candidate>> combinations)
	{
		var result = SpaceSet.Empty;
		var links = Links;

		var i = 0;
		foreach (var assignmentGroup in combinations)
		{
			var lightUpLinks = SpaceSet.Empty;
			foreach (var assignment in assignmentGroup)
			{
				foreach (var set in links)
				{
					if (set.Contains(assignment))
					{
						lightUpLinks.Add(set);
					}
				}
			}

			if (i++ == 0)
			{
				result |= lightUpLinks;
			}
			else
			{
				result &= lightUpLinks;
			}
		}

		return result;
	}

	/// <summary>
	/// Gets ranks on eliminations.
	/// </summary>
	/// <param name="combinations">The combinations.</param>
	/// <returns>A lookup table that displays each candidate and its elimination rank.</returns>
	/// <exception cref="InvalidOperationException">Throws when <see cref="Links"/> is not specified.</exception>
	public unsafe FrozenDictionary<Candidate, int> GetEliminationRanksCore(ReadOnlySpan<ReadOnlyMemory<Candidate>> combinations)
	{
		if (!Links)
		{
			throw new InvalidOperationException(SR.ExceptionMessage("RequireLinksOnCheckingEliminationRank"));
		}

		var result = new List<(Candidate Elimination, int Rank)>();
		foreach (var elimination in GetEliminationsCore(combinations))
		{
			// Find for all possible links.
			var occupiedLinks = SpaceSet.Empty;
			foreach (var link in Links)
			{
				if (link.GetAvailableRange(Grid).Contains(elimination))
				{
					occupiedLinks.Add(link);
				}
			}

			// Enumerate all combinations, to find for filling states on links.
			var occupiedStates = new SortedSet<int>();
			foreach (var combination in combinations)
			{
				var linksCount = 0;
				var map = combination.Span.AsCandidateMap();
				foreach (var link in occupiedLinks)
				{
					if (link.GetAvailableRange(Grid) & map)
					{
						linksCount++;
					}
				}

				// Handle cannibalism cases.
				// If the elimination is inside a truth, we should remove the number of truths.
				var occupiedTruthsCount = 0;
				foreach (var truth in Truths)
				{
					if (truth.GetAvailableRange(Grid).Contains(elimination))
					{
						occupiedTruthsCount++;
					}
				}

				// Record the value.
				occupiedStates.Add(linksCount - occupiedTruthsCount);
			}

			// Store the states.
			result.Add((elimination, occupiedStates.Max - occupiedStates.Min));
		}

		// It seems that a FrozenDictionary<int, int> may be optimized that will cause values are ordered by keys,
		// which is out of control for me to construct a dictionary that should order by its value.
		//result.Sort(static (left, right) => left.Rank.CompareTo(right.Rank) is var r1 and not 0 ? r1 : left.Elimination.CompareTo(right.Elimination));

		// Return the value.
		return result.ToFrozenDictionary(static pair => pair.Elimination, static pair => pair.Rank);
	}
}
