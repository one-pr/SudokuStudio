namespace Sudoku.Theories.SetTheory;

/// <summary>
/// Provides a list of members to get pattern detailed information that should be inferred.
/// </summary>
public static partial class LogicReasoner
{
	/// <summary>
	/// <para>
	/// Gets the rank of the pattern. If the pattern is not minimal, it may contains multiple ranks,
	/// corresponding to different subpatterns eliminates for different candidates.
	/// </para>
	/// <para>
	/// This method also returns corresponding sublogic to each elimination.
	/// </para>
	/// </summary>
	/// <param name="logic">The logic.</param>
	/// <param name="sublogics">Represents sublogic views for each eliminations.</param>
	/// <returns>A <see cref="Rank"/> instance representing the result.</returns>
	public static Rank GetRank(ref readonly Logic logic, out FrozenDictionary<Conclusion, Logic> sublogics)
	{
		var permutations = GetPermutations(in logic);
		return PermRequired.GetRank(
			in logic,
			PermRequired.GetConclusions(in logic, permutations, true, false),
			permutations,
			out sublogics
		);
	}

	/// <summary>
	/// <para>
	/// Gets the number of assigned candidates that can make a pattern satisfied with all truths and links.
	/// </para>
	/// <para>
	/// <para>
	/// Please note that there's a possible case that cause a pattern
	/// that can accommodate different number of assigned values.
	/// For example, if a pattern produces multiple eliminations from different sub-patterns,
	/// the result may uses different number of assignments to satisfy all sets (truths and links).
	/// </para>
	/// </para>
	/// </summary>
	/// <param name="logic">The pattern.</param>
	/// <returns>The permutation count value.</returns>
	public static AssignedCount GetAssignedCount(ref readonly Logic logic)
		=> PermRequired.GetAssignedCount(in logic, GetPermutations(in logic));

	/// <summary>
	/// Try to find all possible permutations.
	/// </summary>
	/// <param name="logic">The pattern.</param>
	/// <returns>The permutations.</returns>
	public static ReadOnlySpan<Permutation> GetPermutations(ref readonly Logic logic)
	{
		var permutationsRaw = SetTheorySolver.Solve(in logic);
		var linksLookup = logic.LinksLightupLookup;
		var result = new List<Permutation>(permutationsRaw.Length);
		foreach (var permutation in permutationsRaw)
		{
			var lightupLinks = new List<Space>();
			foreach (var candidate in permutation)
			{
				foreach (var link in logic.Links)
				{
					if (linksLookup![link].Contains(candidate))
					{
						lightupLinks.Add(link);
					}
				}
			}
			result.Add(new(permutation, lightupLinks.AsMemory()));
		}
		return result.AsSpan();
	}

	/// <summary>
	/// Try to find all conclusions.
	/// </summary>
	/// <param name="logic">The pattern.</param>
	/// <returns>All conclusions.</returns>
	public static ConclusionSet GetConclusions(ref readonly Logic logic)
		=> PermRequired.GetConclusions(in logic, GetPermutations(in logic), true, false);

	/// <summary>
	/// Try to find all conclusions, without checking <see cref="Logic.Links"/>.
	/// </summary>
	/// <param name="logic">The pattern.</param>
	/// <returns>All conclusions.</returns>
	public static ConclusionSet GetConclusionsWithoutCheckingLinks(ref readonly Logic logic)
		=> PermRequired.GetConclusions(in logic, GetPermutations(in logic), false, false);

	/// <summary>
	/// Try to find all possible conclusions of the patter in theory,
	/// no matter whether the candidate exists in the grid or not.
	/// </summary>
	/// <param name="logic">The pattern.</param>
	/// <returns>All conclusions, ignoring existence of conclusions.</returns>
	public static ConclusionSet GetConclusionZone(ref readonly Logic logic)
		=> PermRequired.GetConclusions(in logic, GetPermutations(in logic), true, true);

	/// <summary>
	/// Gets all rank-0 links.
	/// </summary>
	/// <param name="logic">The pattern.</param>
	/// <returns>All rank-0 links.</returns>
	public static SpaceSet GetRank0Links(ref readonly Logic logic)
		=> PermRequired.GetRank0Links(in logic, GetPermutations(in logic));

	/// <summary>
	/// Gets all rank-0 eliminations.
	/// </summary>
	/// <param name="logic">The pattern.</param>
	/// <returns>All rank-0 eliminations.</returns>
	public static CandidateMap GetRank0Eliminations(ref readonly Logic logic)
	{
		var permutations = GetPermutations(in logic);
		return PermRequired.GetRank0Eliminations(
			in logic,
			PermRequired.GetConclusions(in logic, permutations, true, false),
			permutations
		);
	}

	/// <summary>
	/// Finds for minimal truths that covers the candidate as elimination.
	/// </summary>
	/// <param name="logic">The pattern.</param>
	/// <param name="elimination">The elimination.</param>
	/// <returns>The minimal truths.</returns>
	public static SpaceSet GetMinimalTruths(ref readonly Logic logic, Candidate elimination)
	{
		var permutations = GetPermutations(in logic);
		return PermRequired.GetMinimalTruths(
			in logic,
			elimination,
			PermRequired.GetConclusions(in logic, permutations, true, false),
			permutations
		);
	}

	/// <summary>
	/// Finds for minimal <see cref="Logic"/> instance that can eliminate the specified elimination.
	/// </summary>
	/// <param name="logic">The pattern.</param>
	/// <param name="elimination">The elimination.</param>
	/// <returns>A minimal <see cref="Logic"/> instance that can remove the specified elimination.</returns>
	public static Logic GetMinimalPattern(ref readonly Logic logic, Candidate elimination)
	{
		var permutations = GetPermutations(in logic);
		return PermRequired.GetMinimalPattern(
			in logic,
			elimination,
			PermRequired.GetConclusions(in logic, permutations, true, false),
			permutations
		);
	}

	/// <summary>
	/// Trims for all excess links, and return a new <see cref="Logic"/> instance.
	/// </summary>
	/// <param name="logic">The original pattern.</param>
	/// <returns>A new <see cref="Logic"/> instance.</returns>
	public static Logic TrimExcessLinks(ref readonly Logic logic)
	{
		var permutations = GetPermutations(in logic);
		return PermRequired.TrimExcessLinks(
			in logic,
			PermRequired.GetConclusions(in logic, permutations, true, false),
			permutations
		);
	}

	/// <summary>
	/// Transforms truths into links if such truths can be links.
	/// </summary>
	/// <param name="logic">The original pattern.</param>
	/// <returns>A new <see cref="Logic"/> instance.</returns>
	public static Logic ConvertTruthsToLinks(ref readonly Logic logic)
	{
		var permutations = GetPermutations(in logic);
		return PermRequired.ConvertTruthsToLinks(
			in logic,
			PermRequired.GetConclusions(in logic, permutations, true, false),
			permutations
		);
	}
}
