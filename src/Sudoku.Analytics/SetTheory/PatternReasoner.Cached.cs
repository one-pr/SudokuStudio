namespace Sudoku.SetTheory;

public partial class PatternReasoner
{
	/// <summary>
	/// Represents equivalent implementation of the parent type, but an extra parameter <c>permutations</c> is required.
	/// </summary>
	private static class Cached
	{
		public static int GetEliminationRank(in Pattern pattern, Candidate candidate, ReadOnlySpan<Permutation> permutations)
		{
			ref readonly var links = ref pattern.Links;
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

		public static AssignmentCountRange GetAssignmentsCount(in Pattern pattern, ReadOnlySpan<Permutation> permutations)
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

		public static ReadOnlySpan<Conclusion> GetConclusions(
			in Pattern pattern,
			ReadOnlySpan<Permutation> permutations,
			bool checkingLinks
		)
		{
			ref readonly var grid = ref pattern.Grid;
			ref readonly var fullMap = ref pattern.FullMap;
			var candidatesMap = grid.CandidatesMap;

			var result = ConclusionSet.Empty;
			var i = 0;
			foreach (var permutation in permutations)
			{
				var tempConclusions = ConclusionSet.Empty;
				foreach (var candidate in permutation)
				{
					var cell = candidate / 9;
					var digit = candidate % 9;
					if (checkingLinks)
					{
						// We should check whether a conclusion is on a link or not.
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
					else
					{
						// If candidates exists, we can eliminate or set it without checking whether it is on a link or not.
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

		public static SpaceSet GetRank0Links(in Pattern pattern, ReadOnlySpan<Permutation> permutations)
		{
			var result = pattern.Links;
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

		public static CandidateMap GetRank0Eliminations(in Pattern pattern, ReadOnlySpan<Permutation> permutations)
		{
			var conclusions = GetConclusions(pattern, permutations, true);
			var result = CandidateMap.Empty;
			foreach (var (type, candidate) in conclusions)
			{
				if (type != Elimination)
				{
					continue;
				}

				foreach (var link in GetRank0Links(pattern, permutations))
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
}
