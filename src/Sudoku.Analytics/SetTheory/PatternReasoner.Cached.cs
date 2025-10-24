namespace Sudoku.SetTheory;

public partial class PatternReasoner
{
	/// <summary>
	/// Represents equivalent implementation of the parent type, but an extra parameter <c>permutations</c> is required.
	/// </summary>
	private static class Cached
	{
		public static int GetEliminationRank(in Logic logic, Candidate candidate, ReadOnlySpan<Permutation> permutations)
		{
			ref readonly var links = ref logic.Links;
			var (maxOccupied, minOccupied) = (0, links.Count);
			foreach (var permutation in permutations)
			{
				var lightupLinks = SpaceSet.Empty;
				foreach (var assigned in permutation)
				{
					foreach (var link in assigned.Spaces)
					{
						if (links.Contains(link))
						{
							lightupLinks += link;
						}
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

		public static AssignmentCountRange GetAssignmentsCount(in Logic logic, ReadOnlySpan<Permutation> permutations)
		{
			var (min, max) = (int.MaxValue, int.MinValue);
			foreach (var permutation in permutations)
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

		public static ReadOnlySpan<Conclusion> GetConclusions(in Logic logic, ReadOnlySpan<Permutation> permutations, bool checkingLinks)
		{
			ref readonly var grid = ref logic.Grid;
			ref readonly var links = ref logic.Links;
			var candidatesMap = grid.CandidatesMap;

			// Construct a link lookup. This lookup table records candidates covered in the grid.
			// We'll use this variable in checking light-up links for each permutation, in order to find strict conclusions
			// when option 'checkingLinks' is on.
			var linksLookup = checkingLinks ? new Dictionary<Space, CandidateMap>() : null;
			if (checkingLinks)
			{
				foreach (var link in links)
				{
					linksLookup!.Add(link, link.GetAvailableRange(grid));
				}
			}

			var result = ConclusionSet.Empty;
			var i = 0;
			foreach (var permutation in permutations)
			{
				var tempConclusions = ConclusionSet.Empty;

				// Links are ignored to be checked.
				foreach (var candidate in permutation)
				{
					var cell = candidate / 9;
					var digit = candidate % 9;

					// If candidates exists, we can eliminate or set it without checking whether it is on a link or not.
					foreach (var c in Peer.PeersMap[cell] & candidatesMap[digit])
					{
						tempConclusions.Add(new(Elimination, c, digit));
					}
					foreach (var d in (Mask)(grid.GetCandidates(cell) & ~(1 << digit)))
					{
						tempConclusions.Add(new(Elimination, cell, d));
					}
					tempConclusions.Add(new(Assignment, cell, digit));
				}

				if (checkingLinks)
				{
					// We should find for all light-up links, and find eliminations.
					// If the elimination doesn't include in every links, we should remove it.
					foreach (var conclusion in tempConclusions.ToArray())
					{
						// Construct light-up links.
						var lightupLinks = SpaceSet.Empty;
						foreach (var candidate in permutation)
						{
							foreach (var link in links)
							{
								if (linksLookup![link].Contains(candidate))
								{
									lightupLinks += link;
								}
							}
						}

						// Traverse all light-up links. Find whether the current conclusion to check is covered by any light-up links.
						var anyLinksIncludesConclusion = false;
						foreach (var link in lightupLinks)
						{
							if (link.Contains(conclusion.Candidate))
							{
								anyLinksIncludesConclusion = true;
								break;
							}
						}
						if (!anyLinksIncludesConclusion)
						{
							tempConclusions -= conclusion;
						}
					}
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

		public static SpaceSet GetRank0Links(in Logic logic, ReadOnlySpan<Permutation> permutations)
		{
			var result = logic.Links;
			foreach (var permutation in permutations)
			{
				var lightUpLinks = SpaceSet.Empty;
				foreach (var assigned in permutation)
				{
					lightUpLinks.AddRange(assigned.Spaces);
				}
				result &= lightUpLinks;
			}
			return result;
		}

		public static CandidateMap GetRank0Eliminations(in Logic logic, ReadOnlySpan<Permutation> permutations)
		{
			var conclusions = GetConclusions(logic, permutations, true);
			var result = CandidateMap.Empty;
			foreach (var (type, candidate) in conclusions)
			{
				if (type != Elimination)
				{
					continue;
				}

				foreach (var link in GetRank0Links(logic, permutations))
				{
					if (link.Contains(candidate))
					{
						result.Add(candidate);
					}
				}
			}
			return result;
		}

		public static SpaceSet GetMinimalTruths(in Logic logic, Candidate elimination, ReadOnlySpan<Permutation> permutations)
		{
			ref readonly var truthsRef = ref logic.Truths;
			if (truthsRef.Count <= 2)
			{
				// There's nothing to do - A minimum pattern is an X-Wing which cannot reduce.
				return truthsRef;
			}

			var allEliminations = CandidateMap.Empty;
			foreach (var (type, candidate) in GetConclusions(logic, permutations, true))
			{
				if (type == Elimination)
				{
					allEliminations += candidate;
				}
			}
			if (GetRank0Eliminations(logic, permutations) == allEliminations)
			{
				// All candidates are rank-0 eliminations.
				return truthsRef;
			}
			if (!allEliminations.Contains(elimination))
			{
				// Invalid case - the original pattern cannot remove such candidate.
				var candidateString = new RxCyConverter().CandidateConverter(elimination.AsCandidateMap());
				throw new ArgumentException(
					$"The original pattern cannot remove the specified candidate '{candidateString}'.",
					nameof(elimination)
				);
			}

			var truths = truthsRef.ToArray();

			// Otherwise, we should iterate on each combination of truths to get eliminations.
			for (var truthsSize = 2; truthsSize <= truths.Length - 1; truthsSize++)
			{
				// Iterate on each combination of truths.
				foreach (var truthCombination in truths & truthsSize)
				{
					var sublogicTruths = truthCombination.AsSpaceSet();
					var sublogic = new Logic(sublogicTruths, logic.Links, logic.Grid);
					var sublogicConclusions = GetConclusions(sublogic, GetPermutations(sublogic), true);
					if (sublogicConclusions.Contains(new(Elimination, elimination)))
					{
						// If the pattern can delete such candidate, we'll know that the pattern has a minimal size of truths.
						// All patterns greater than this (with larger size of truths) is redundant.
						return sublogicTruths;
					}
				}
			}

			// Otherwise, we cannot find any subpatterns that can remove that candidate - all truths are necessary.
			return truthsRef;
		}

		public static Logic TrimExcessLinks(in Logic logic, ConclusionSet conclusions, ReadOnlySpan<Permutation> permutations)
		{
			// Just remove the link and find conclusions.
			// If we can find all possible conclusions of the pattern if removed,
			// we can know that the link having been removed is redundant.
			var result = logic;
			foreach (var link in logic.Links)
			{
				var sublogic = new Logic(logic.Truths, logic.Links - link, logic.Grid);
				if ((GetConclusions(sublogic, GetPermutations(sublogic), true).AsSet() & conclusions) == conclusions)
				{
					// The link can be removed because we find a subpattern with removed link state
					// can eliminate all conclusions specified.
					result.RemoveLink(link);
				}
			}
			return result;
		}
	}
}
