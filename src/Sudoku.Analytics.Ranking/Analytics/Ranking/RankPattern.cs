namespace Sudoku.Analytics.Ranking;

/// <summary>
/// Represents an object that can calculate rank-related information via the specified data.
/// </summary>
/// <param name="grid">The grid.</param>
/// <param name="sets">The rank sets.</param>
public sealed class RankPattern(in Grid grid, params RankSetCollection sets)
{
	/// <summary>
	/// Represents candidates.
	/// </summary>
	private readonly CandidateMap _candidates = BuildCandidates(grid, sets);

	/// <summary>
	/// The backing grid.
	/// </summary>
	private readonly Grid _grid = grid;


	/// <summary>
	/// Indicates whether the current pattern is stable rank-0 pattern, i.e. all links are rank-0 sets.
	/// </summary>
	public bool IsRank0Pattern => Rank0Sets == Sets.Links;

	/// <summary>
	/// Indicates the rank of the current pattern. If the pattern is unstable
	/// (sometimes assignments certain times of digits in the pattern but sometimes not),
	/// this property will return <see langword="null"/>.
	/// </summary>
	public int? Rank
	{
		get
		{
			var factAssignmentCountValues = new HashSet<int>();
			foreach (var l in from assignment in Assignments select assignment.Length)
			{
				factAssignmentCountValues.Add(l);
			}
			return factAssignmentCountValues.Count == 1 ? Sets.Links.Count - factAssignmentCountValues.First() : null;
		}
	}

	/// <summary>
	/// Indicates all cells used.
	/// </summary>
	public CellMap Cells => _candidates.Cells;

	/// <summary>
	/// Indicates eliminations can be found in the current pattern.
	/// </summary>
	public CandidateMap Eliminations
	{
		get
		{
			var result = CandidateMap.Empty;
			var i = 0;
			var candidatesMap = _grid.CandidatesMap;
			foreach (var assignmentGroup in Assignments)
			{
				var current = CandidateMap.Empty;
				foreach (var assignment in assignmentGroup)
				{
					var cell = assignment / 9;
					var digit = assignment % 9;
					foreach (var otherDigit in (Mask)(_grid.GetCandidates(cell) & ~(1 << digit)))
					{
						current.Add(cell * 9 + otherDigit);
					}
					foreach (var otherCell in PeersMap[cell] & candidatesMap[digit])
					{
						current.Add(otherCell * 9 + digit);
					}
				}

				if (i++ == 0)
				{
					result |= current;
				}
				else
				{
					result &= current;
				}
			}
			return result;
		}
	}

	/// <summary>
	/// Indicates the candidates.
	/// </summary>
	public ref readonly CandidateMap Candidates => ref _candidates;

	/// <summary>
	/// Indicates the grid.
	/// </summary>
	public ref readonly Grid Grid => ref _grid;

	/// <summary>
	/// Returns a list of <see cref="Candidate"/> group that describes the valid assignments.
	/// </summary>
	/// <returns>Valid assignments.</returns>
	public ReadOnlySpan<ReadOnlyMemory<Candidate>> Assignments
	{
		get
		{
			var result = new List<ReadOnlyMemory<Candidate>>();

			var assignments = CandidateMap.Empty;
			var fullMap = (Candidate[][])[.. from set in Sets.Truths select set.GetAvailableRange(_grid).ToArray()];
			var combinations = fullMap.GetExtractedCombinations();
			foreach (var combination in combinations)
			{
				var candidates = combination.AsCandidateMap();

				// Check satisfiability.
				var areAllRankSetsSatisfied = true;
				foreach (var rankSet in Sets)
				{
					if (!rankSet.IsSatisfied(candidates))
					{
						areAllRankSetsSatisfied = false;
						break;
					}
				}
				if (!areAllRankSetsSatisfied)
				{
					continue;
				}

				// The combination is okay to be added.
				result.Add(combination);
			}

			return result.AsSpan();
		}
	}

	/// <summary>
	/// Indicates the rank sets.
	/// </summary>
	public RankSetCollection Sets { get; } = sets;

	/// <summary>
	/// Try to find all rank-0 sets.
	/// </summary>
	public RankSetCollection Rank0Sets
	{
		get
		{
			RankSetCollection result = [.. Sets.Links];
			var links = result.Clone();

			var i = 0;
			foreach (var assignmentGroup in Assignments)
			{
				var lightUpLinks = new RankSetCollection();
				foreach (var assignment in assignmentGroup)
				{
					foreach (var set in links)
					{
						if (set.ContainsAssignment(assignment))
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
	}


	/// <inheritdoc/>
	public override string ToString()
	{
		var truths = Sets.Truths;
		var links = Sets.Links;
		var truthsString = string.Join(' ', from truth in truths select truth.ToString());
		var linksString = string.Join(' ', from link in links select link.ToString());
		return $"T{truths.Count} = {truthsString}, L{links.Count} = {linksString}";
	}


	/// <summary>
	/// Build candidates.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="rankSets">The rank sets.</param>
	private static CandidateMap BuildCandidates(in Grid grid, RankSetCollection rankSets)
	{
		var result = CandidateMap.Empty;

		var candidatesMap = grid.CandidatesMap;
		foreach (var rankSet in rankSets.EnumerateTruths())
		{
			switch (rankSet)
			{
				case CellTruth { Cell: var cell }:
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						result.Add(cell * 9 + digit);
					}
					break;
				}
				case HouseTruth { House: var house, Digit: var digit }:
				{
					foreach (var cell in HousesMap[house] & candidatesMap[digit])
					{
						result.Add(cell * 9 + digit);
					}
					break;
				}
			}
		}

		return result;
	}
}
