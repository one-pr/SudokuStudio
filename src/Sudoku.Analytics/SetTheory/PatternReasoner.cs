namespace Sudoku.SetTheory;

/// <summary>
/// Provides a list of members to get pattern detailed information that should be inferred.
/// </summary>
public static partial class PatternReasoner
{
	/// <summary>
	/// Gets rank of specified elimination. The rank of elimination is defined as <c>n(links) - n(lightup_links)</c>.
	/// </summary>
	/// <param name="pattern">The pattern.</param>
	/// <param name="candidate">The candidate.</param>
	/// <returns>The rank of elimination. -1 will be returned if candidate is not an eliminiation.</returns>
	public static int GetEliminationRank(in Pattern pattern, Candidate candidate)
		=> Cached.GetEliminationRank(pattern, candidate, GetPermutations(pattern));

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
		=> Cached.GetAssignmentsCount(pattern, GetPermutations(pattern));

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
		=> Cached.GetConclusions(pattern, GetPermutations(pattern), true);

	/// <summary>
	/// Try to find all conclusions, without checking <see cref="Pattern.Links"/>.
	/// </summary>
	/// <param name="pattern">The pattern.</param>
	/// <returns>All conclusions.</returns>
	public static ReadOnlySpan<Conclusion> GetConclusionsWithoutCheckingLinks(in Pattern pattern)
		=> Cached.GetConclusions(pattern, GetPermutations(pattern), false);

	/// <summary>
	/// Gets all rank-0 links.
	/// </summary>
	/// <param name="pattern">The pattern.</param>
	/// <returns>All rank-0 links.</returns>
	public static SpaceSet GetRank0Links(in Pattern pattern) => Cached.GetRank0Links(pattern, GetPermutations(pattern));

	/// <summary>
	/// Gets all rank-0 eliminations.
	/// </summary>
	/// <param name="pattern">The pattern.</param>
	/// <returns>All rank-0 eliminations.</returns>
	public static CandidateMap GetRank0Eliminations(in Pattern pattern)
		=> Cached.GetRank0Eliminations(pattern, GetPermutations(pattern));
}
