#undef SKIP_ZERO_RANK_FULL_REDUNDANCY_CHECK

namespace Sudoku.Ranking;

public partial struct RankPattern
{
	/// <summary>
	/// Correct links that will cover all candidates from the truths and eliminations,
	/// if links are unavailable, invalid or unknown.
	/// </summary>
	/// <returns>The spaces that represent links.</returns>
	/// <remarks>
	/// <include file="../../../../global-doc-comments.xml" path="/g/developer-notes"/>
	/// <para>
	/// Because the algorithm design differs from that of the XSudo software,
	/// we need special handling to support this feature.
	/// </para>
	/// <para>
	/// First, we must identify all possible assignment combinations in the entire sequence,
	/// then determine how many times values are filled, and compute the rank of pattern.
	/// Thus, the rank of the pattern falls into two cases: rank equals 0, or rank is a positive integer
	/// (Note: a valid rank of pattern cannot be negative).
	/// </para>
	/// <para>
	/// At the same time, when obtaining assignment combinations, we also record the exclusion effects generated
	/// by those assignments, and convert these into links.
	/// When the rank is 0, all computed links are treated as valid and can be returned directly.
	/// But when the rank is non-zero, we must test each link individually.
	/// </para>
	/// <para>
	/// We iterate over every link. For each link, we perform the following test:
	/// find all candidates under this link, and pair them in every possible two-candidate combination
	/// (i.e., the total number of pairs equals <c>C(n, 2)</c>, where <c>n</c> is the total number of candidates).
	/// </para>
	/// <para>
	/// By definition of a link, no pair of candidates within it can both be true simultaneously.
	/// Therefore, for each such candidate pair, we temporarily force both to be true, and exclude all truths
	/// that contain either of these two candidates as true simultaneously, as well as exclude the link itself.
	/// We then treat the remaining truths and links as a subpattern,
	/// compute all assignment combinations for this subpattern, and use them to determine its elimination results.
	/// </para>
	/// <para>
	/// Because the subpattern has fewer truths, it may produce more elimination combinations,
	/// and the total eliminations may even exceed those of the original pattern.
	/// </para>
	/// <para>
	/// Since eliminations may increase, we check whether the set of eliminations for the subpattern is equal to or fully contains
	/// the original pattern's elimination set.
	/// If it does, this indicates that under the violation of the link rule for this candidate pair,
	/// we can still achieve those eliminations - at least for this pair.
	/// </para>
	/// <para>
	/// We must repeat this check for all candidate pairs: if every pair yields eliminations equal to
	/// or containing the original set, then the link is indeed redundant and can be removed.
	/// Conversely, if there is at least one pair for which the eliminations are neither equal to
	/// nor a superset of the original eliminations, the link is "worthy" and cannot be removed.
	/// </para>
	/// <para>
	/// Apply this algorithm to check all links once, identify and remove all redundant ones;
	/// the remaining sets are the correct links.
	/// </para>
	/// </remarks>
	public unsafe SpaceSet CorrectLinks()
	{
		// Find for all links, passing eliminations.
		var combinations = GetAssignmentCombinationsCore(out var fullLinks);

#if SKIP_ZERO_RANK_FULL_REDUNDANCY_CHECK
		// Check whether the pattern is rank-0. If so, all links can be used as elimination one,
		// meaning there's no redundant links. Just treat them as result.
		if (GetRank0LinksCore(combinations) is var rank0Links && rank0Links.Count == Truths.Count)
		{
			return rank0Links;
		}
#endif

		// Otherwise, non-rank-0 pattern.
		// We should do a full check on compatibility of pairs on links, to know which are necessary and which are not.

		// Get eliminations as original one.
		// If there's no valid eliminations, we cannot know any extra information,
		// even validity of pattern (truths selection, etc.).
		var originalEliminations = GetEliminationsCore(combinations);

		// Create a dictionary to record distribution of each candidate and its containing truth.
		var truthDistributionLookup = CreateTruthsDistributionDictionary();

		// Iterate on each link, to check whether it is redundant or not.
		var result = SpaceSet.Empty;
		foreach (var link in fullLinks)
		{
			var isLinkRedundant = true;

			var combinationCandidates = _candidates & link.GetAvailableRange(Grid);
			foreach (ref readonly var combination in combinationCandidates & 2)
			{
				// Special check on space relations between two assignments.
				// Sometimes the pairs of combination can be appeared in a line, which would be already checked by line links.
				// We should ignore the pairs.
				if (link.House is < 9 and not -1
					&& BitOperations.IsPow2(combination.Digits)
					&& combination.Cells is { Count: 2, SharedLine: not FallbackConstants.@int } pairCells)
				{
					// Skip for the pair.
					continue;
				}

				var setsIntersected = SpaceSet.Empty;
				var setsUnioned = SpaceSet.Empty;
				var isFirstAssigned = true;
				foreach (var assigned in combination)
				{
					ref readonly var set = ref truthDistributionLookup.GetValueRef(assigned);
					setsUnioned |= set;

					if (isFirstAssigned)
					{
						isFirstAssigned = false;
						setsIntersected |= set;
						continue;
					}
					setsIntersected &= set;
				}

				// Check whether they are in a same truth.
				if (setsIntersected)
				{
					// This combination disobeys the rule of truth, invalid.
					// Skip for the current combination.
					continue;
				}

				// Suppose the link is gone and make an assumption that do both candidates are true.
				// Here we should do a trick: forcely assign two candidates to the grid, regardless of conflict of grid.
				// This will also work for both digits in a same cell.
				// We know that two candidates will also clear digit appearances from peer cells,
				// which is by design of type 'Grid', so we can perform the applying rules
				// to clear irrelevant candidates.
				// Although the grid becomes invalid, we know that this type won't check validity of the grid.
				var subgrid = Grid;
				foreach (var assigned in combination)
				{
					subgrid.SetDigit(assigned / 9, assigned % 9);
				}

				// Create a pattern.
				// Note that link can be empty because here we don't use any links as necessary data -
				// we just want to get all assignment combinations of the subpattern mentioned above,
				// whose relied mechanism doesn't use any links (link-free).
				var subpatternTruths = Truths & ~setsUnioned;
				var subpattern = new RankPattern(in subgrid, in subpatternTruths, in SpaceSet.Empty);
				var subpatternCombinations = subpattern.GetAssignmentCombinations();
				if (subpatternCombinations.Length == 0)
				{
					// There's no valid combinations in this subpattern, meaning the pair cannot be compatible.
					continue;
				}

				var subpatternEliminations = subpattern.GetEliminationsCore(subpatternCombinations);

				// If there's a link from candidate to elimination,
				// we will get an information "those two cannot be both true".
				// Therefore, safely add elimination from original grid into the elimination set.
				foreach (var assignmentToCheck in combination)
				{
					var cell = assignmentToCheck / 9;
					var digit = assignmentToCheck % 9;

					// Check whether the candidate can see at least one elimination.
					var assignmentToCheckPeerCandidates = CandidateMap.Empty;
					foreach (var c in Grid.CandidatesMap[digit] & PeersMap[cell])
					{
						assignmentToCheckPeerCandidates.Add(c * 9 + digit);
						if (!_candidates.Contains(c * 9 + digit))
						{
							subpatternEliminations.Add(c * 9 + digit);
						}
					}
					foreach (var d in Grid.GetCandidates(cell))
					{
						assignmentToCheckPeerCandidates.Add(cell * 9 + d);
						if (!_candidates.Contains(cell * 9 + d))
						{
							subpatternEliminations.Add(cell * 9 + d);
						}
					}

					foreach (var eliminationToCheck in assignmentToCheckPeerCandidates & originalEliminations)
					{
						var tempMap = link.GetAvailableRange(Grid);
						if (tempMap.Contains(assignmentToCheck) && tempMap.Contains(eliminationToCheck))
						{
							// They are in a same link, but the link is supposed to be disappeared.
							// So we cannot link they up and remove the elimination.
							subpatternEliminations.Remove(eliminationToCheck);
						}

						// Here is a rescue:
						// The assignment and elimination can see each other, but they are in a same line and block,
						// just like two cells in a locked candidates pattern.
						// Even if block link can remove it, we can also remove that candidate because it can be treated
						// as a row and column link.
						var rescueFlag = false;
						foreach (var nestedAssignedCell in combination)
						{
							var canAssignedCellSeeElimination = link.House is <= 9 and not -1
								&& (nestedAssignedCell / 9, nestedAssignedCell % 9) is var (assignedCell, assignedDigit)
								&& (eliminationToCheck / 9, eliminationToCheck % 9) is var (eliminatedCell, eliminatedDigit)
								&& assignedDigit == eliminatedDigit
								&& (assignedCell.AsCellMap() + eliminatedCell).SharedLine != FallbackConstants.@int;
							if (canAssignedCellSeeElimination)
							{
								rescueFlag = true;
								break;
							}
						}
						if (rescueFlag)
						{
							subpatternEliminations.Add(eliminationToCheck);
						}
					}
				}

				if ((subpatternEliminations & originalEliminations) != originalEliminations)
				{
					// If we removed the link and can get a same elimination set or a subset,
					// we can know that the link is redundant.
					isLinkRedundant = false;
					goto CheckRedudancy;
				}
			}
		CheckRedudancy:
			if (!isLinkRedundant)
			{
				// Otherwise, the link is required.
				result.Add(link);
			}
		}

		// Return necessary links.
		return result;
	}

	/// <summary>
	/// Creates a dictionary as lookup table on all pattern candidates and their containing truths.
	/// </summary>
	/// <returns>A dictionary that records each candidate appeared in pattern and its containing truths.</returns>
	private Dictionary<Candidate, SpaceSet> CreateTruthsDistributionDictionary()
	{
		var result = new Dictionary<Candidate, SpaceSet>();
		foreach (var truth in Truths)
		{
			foreach (var candidate in truth.GetAvailableRange(Grid))
			{
				if (!result.TryAdd(candidate, [truth]))
				{
					var original = result[candidate];
					original.Add(truth);
					result[candidate] = original;
				}
			}
		}
		return result;
	}
}
