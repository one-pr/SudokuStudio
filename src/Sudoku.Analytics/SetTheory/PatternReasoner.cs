namespace Sudoku.SetTheory;

/// <summary>
/// Provides a list of members to get pattern detailed information that should be inferred.
/// </summary>
public static partial class PatternReasoner
{
	/// <summary>
	/// Gets rank of specified elimination. The rank of elimination is defined as <c>n(links) - n(lightup_links)</c>.
	/// </summary>
	/// <param name="logic">The pattern.</param>
	/// <param name="candidate">The candidate.</param>
	/// <returns>The rank of elimination. -1 will be returned if candidate is not an eliminiation.</returns>
	public static int GetEliminationRank(in Logic logic, Candidate candidate)
		=> Cached.GetEliminationRank(logic, candidate, GetPermutations(logic));

	/// <summary>
	/// Gets the rank of the pattern. If the pattern is not minimal, it may contains multiple ranks,
	/// corresponding to different subpatterns eliminates for different candidates.
	/// </summary>
	/// <param name="logic">The logic.</param>
	/// <returns>A <see cref="Rank"/> instance representing the result.</returns>
	public static Rank GetRank(in Logic logic)
	{
		var rankList = new SortedSet<int>();
		foreach (var elimination in
			from conclusion in GetConclusions(logic)
			where conclusion.ConclusionType == Elimination
			select conclusion)
		{
			var minimalPattern = GetMinimalPattern(logic, elimination.Candidate);
			rankList.Add(minimalPattern.Links.Count - minimalPattern.Truths.Count);
		}
		return rankList.Count == 1 ? rankList.First() : (int[])[.. rankList];
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
	/// <param name="logic">The pattern.</param>
	/// <returns>The permutation count value.</returns>
	/// <seealso cref="AssignmentCountRange"/>
	public static AssignmentCountRange GetAssignmentsCount(in Logic logic)
		=> Cached.GetAssignmentsCount(logic, GetPermutations(logic));

	/// <summary>
	/// Try to find all possible permutations.
	/// </summary>
	/// <param name="logic">The pattern.</param>
	/// <returns>The permutations.</returns>
	public static ReadOnlySpan<Permutation> GetPermutations(in Logic logic) => SetTheorySolver.Solve(logic);

	/// <summary>
	/// Try to find all conclusions.
	/// </summary>
	/// <param name="logic">The pattern.</param>
	/// <returns>All conclusions.</returns>
	public static ReadOnlySpan<Conclusion> GetConclusions(in Logic logic)
		=> Cached.GetConclusions(logic, GetPermutations(logic), true);

	/// <summary>
	/// Try to find all conclusions, without checking <see cref="Logic.Links"/>.
	/// </summary>
	/// <param name="logic">The pattern.</param>
	/// <returns>All conclusions.</returns>
	public static ReadOnlySpan<Conclusion> GetConclusionsWithoutCheckingLinks(in Logic logic)
		=> Cached.GetConclusions(logic, GetPermutations(logic), false);

	/// <summary>
	/// Gets all rank-0 links.
	/// </summary>
	/// <param name="logic">The pattern.</param>
	/// <returns>All rank-0 links.</returns>
	public static SpaceSet GetRank0Links(in Logic logic) => Cached.GetRank0Links(logic, GetPermutations(logic));

	/// <summary>
	/// Gets all rank-0 eliminations.
	/// </summary>
	/// <param name="logic">The pattern.</param>
	/// <returns>All rank-0 eliminations.</returns>
	public static CandidateMap GetRank0Eliminations(in Logic logic) => Cached.GetRank0Eliminations(logic, GetPermutations(logic));

	/// <summary>
	/// Finds for minimal truths that covers the candidate as elimination.
	/// </summary>
	/// <param name="logic">The pattern.</param>
	/// <param name="elimination">The elimination.</param>
	/// <returns>The minimal truths.</returns>
	public static SpaceSet GetMinimalTruths(in Logic logic, Candidate elimination)
		=> Cached.GetMinimalTruths(logic, elimination, GetPermutations(logic));

	/// <summary>
	/// Finds for minimal <see cref="Logic"/> instance that can eliminate the specified elimination.
	/// </summary>
	/// <param name="logic">The pattern.</param>
	/// <param name="elimination">The elimination.</param>
	/// <returns>A minimal <see cref="Logic"/> instance that can remove the specified elimination.</returns>
	public static Logic GetMinimalPattern(in Logic logic, Candidate elimination)
	{
		var permutations = GetPermutations(logic);
		var truths = Cached.GetMinimalTruths(logic, elimination, permutations);
		var subpattern = new Logic(truths, logic.Links, logic.Grid);
		return Cached.TrimExcessLinks(subpattern, [new(Elimination, elimination)], GetPermutations(subpattern));
	}

	/// <summary>
	/// Trims for all excess links, and return a new <see cref="Logic"/> instance.
	/// </summary>
	/// <param name="logic">The original pattern.</param>
	/// <returns>A new <see cref="Logic"/> instance.</returns>
	public static Logic TrimExcessLinks(in Logic logic)
	{
		var permutations = GetPermutations(logic);
		return Cached.TrimExcessLinks(logic, Cached.GetConclusions(logic, permutations, true).AsSet(), permutations);
	}
}
