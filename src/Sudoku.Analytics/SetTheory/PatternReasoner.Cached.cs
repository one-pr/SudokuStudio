namespace Sudoku.SetTheory;

public partial class PatternReasoner
{
	/// <summary>
	/// Represents equivalent implementation of the parent type, but an extra parameter <c>permutations</c> is required.
	/// </summary>
	public static class Cached
	{
		/// <inheritdoc cref="PatternReasoner.GetEliminationRank(in Logic, Candidate)"/>
		public static int GetEliminationRank(in Logic logic, Candidate candidate, ReadOnlySpan<Permutation> permutations)
		{
			ref readonly var links = ref logic.Links;
			var (maxOccupied, minOccupied) = (0, links.Count);
			foreach (var permutation in permutations)
			{
				var occupied = permutation.LightupLinks.Length;
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

		/// <inheritdoc cref="PatternReasoner.GetRank0Links(in Logic)"/>
		public static Rank GetRank(in Logic logic, ReadOnlySpan<Conclusion> conclusions, ReadOnlySpan<Permutation> permutations, out FrozenDictionary<Conclusion, Logic> sublogics)
		{
			var resultViews = new Dictionary<Conclusion, Logic>();
			if (logic.Truths.Count == logic.Links.Count)
			{
				// Optimization: If n(truths) == n(links), just return 0.
				// Collect sublogics.
				foreach (var conclusion in conclusions)
				{
					resultViews.Add(conclusion, logic);
				}

				sublogics = resultViews.ToFrozenDictionary();
				return 0;
			}

			// Create a minimal logic lookup table as cache.
			var cachedMinimalLogics = new Dictionary<List<Space>, Logic>(
				EqualityComparer<List<Space>>.Create(
					static (left, right) => left!.AsSpan().SequenceEqual(right!.AsSpan()),
					static obj =>
					{
						var result = new HashCode();
						foreach (var member in obj)
						{
							result.Add(member);
						}
						return result.ToHashCode();
					}
				)
			);
			var rankList = new SortedSet<int>();
			foreach (var elimination in from conclusion in conclusions where conclusion.ConclusionType == Elimination select conclusion)
			{
				// Find for lightup links.
				var lightupLinks = new List<Space>(4);
				foreach (var link in elimination.Candidate.Spaces)
				{
					if (logic.Links.Contains(link))
					{
						lightupLinks.Add(link);
					}
				}

				// Read cache. If minimal pattern is found in cached field,
				// we can directly return the value and its corresponding rank.
				if (cachedMinimalLogics.TryGetValue(lightupLinks, out var minimalCached))
				{
					resultViews.Add(elimination, minimalCached);
					rankList.Add(minimalCached.Links.Count - minimalCached.Truths.Count);
					continue;
				}

				// Otherwise, a conclusion with different origin should be checked.
				var minimal = TrimExcessLinks(logic, [elimination], permutations);
				resultViews.Add(elimination, minimal);
				rankList.Add(minimal.Links.Count - minimal.Truths.Count);

				// Cache eliminations.
				cachedMinimalLogics.Add(lightupLinks, minimal);
			}

			sublogics = resultViews.ToFrozenDictionary();
			return rankList.Count == 1 ? rankList.First() : (int[])[.. rankList];
		}

		/// <inheritdoc cref="PatternReasoner.GetAssignmentsCount(in Logic)"/>
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

		/// <inheritdoc cref="PatternReasoner.GetConclusions(in Logic)"/>
		public static ReadOnlySpan<Conclusion> GetConclusions(in Logic logic, ReadOnlySpan<Permutation> permutations, bool checkingLinks)
		{
			ref readonly var grid = ref logic.Grid;
			ref readonly var links = ref logic.Links;
			var candidatesMap = grid.CandidatesMap;

			// Construct a link lookup. This lookup table records candidates covered in the grid.
			// We'll use this variable in checking light-up links for each permutation, in order to find strict conclusions
			// when option 'checkingLinks' is on.
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
						// Traverse all light-up links.
						// Find whether the current conclusion to check is covered by any light-up links.
						var anyLinksIncludesConclusion = false;
						foreach (var link in permutation.LightupLinks)
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

		/// <inheritdoc cref="PatternReasoner.GetRank0Links(in Logic)"/>
		public static SpaceSet GetRank0Links(in Logic logic, ReadOnlySpan<Permutation> permutations)
		{
			var result = logic.Links;
			foreach (var permutation in permutations)
			{
				if (!(result &= [.. permutation.LightupLinks.Span]))
				{
					break;
				}
			}
			return result;
		}

		/// <inheritdoc cref="PatternReasoner.GetRank0Eliminations(in Logic)"/>
		public static CandidateMap GetRank0Eliminations(in Logic logic, ReadOnlySpan<Conclusion> conclusions, ReadOnlySpan<Permutation> permutations)
		{
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

		/// <inheritdoc cref="PatternReasoner.GetMinimalTruths(in Logic, Candidate)"/>
		public static SpaceSet GetMinimalTruths(in Logic logic, Candidate elimination, ReadOnlySpan<Conclusion> conclusions, ReadOnlySpan<Permutation> permutations)
		{
			ref readonly var truths = ref logic.Truths;
			if (truths.Count <= 2)
			{
				// There's nothing to do - A minimum pattern is an X-Wing which cannot reduce.
				return truths;
			}

			var allEliminations = CandidateMap.Empty;
			foreach (var (type, candidate) in conclusions)
			{
				if (type == Elimination)
				{
					allEliminations += candidate;
				}
			}
			if (GetRank0Eliminations(logic, conclusions, permutations) == allEliminations)
			{
				// All candidates are rank-0 eliminations.
				return truths;
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

			var truthsArray = truths.ToArray();
			var tempFoundTruthCombinations = default(Space[]);
			bool found;
			do
			{
				found = false;

				// Iterate on each combination of truths.
				var targetTruths = tempFoundTruthCombinations ?? truthsArray;
				foreach (var truthCombination in targetTruths & targetTruths.Length - 1)
				{
					var sublogic = new Logic([.. truthCombination], logic.Links, logic.Grid);
					var sublogicConclusions = GetConclusions(sublogic, GetPermutations(sublogic), true);
					if (sublogicConclusions.Contains(new(Elimination, elimination)))
					{
						// If the pattern (with lower-sized truths) can delete such candidate,
						// we should continue to check, until the candidate cannot be eliminiated.
						tempFoundTruthCombinations = truthCombination;
						found = true;
						break;
					}
				}
			} while (found);

			// Otherwise, we cannot find any subpatterns that can remove that candidate - all truths are necessary.
			return tempFoundTruthCombinations?.AsSpaceSet() ?? truths;
		}

		/// <inheritdoc cref="PatternReasoner.GetMinimalPattern(in Logic, Candidate)"/>
		public static Logic GetMinimalPattern(in Logic logic, Candidate elimination, ReadOnlySpan<Conclusion> conclusions, ReadOnlySpan<Permutation> permutations)
		{
			var sublogic = new Logic(GetMinimalTruths(logic, elimination, conclusions, permutations), logic.Links, logic.Grid);
			return TrimExcessLinks(sublogic, [new(Elimination, elimination)], sublogic == logic ? permutations : GetPermutations(sublogic));
		}

		/// <inheritdoc cref="PatternReasoner.TrimExcessLinks(in Logic)"/>
		public static Logic TrimExcessLinks(in Logic logic, ConclusionSet conclusions, ReadOnlySpan<Permutation> permutations)
		{
			// Optimization: If n(truths) == n(links), we cannot remove any possible links because it is already a minimal case.
			if (logic.Truths.Count == logic.Links.Count)
			{
				return logic;
			}

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
