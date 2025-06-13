namespace Sudoku.Analytics.Ranking;

/// <summary>
/// Represents an object that can calculate rank-related information via the specified data.
/// </summary>
/// <param name="grid">The grid.</param>
/// <param name="rankSets">The rank sets.</param>
public sealed class RankPattern(in Grid grid, params RankSetCollection rankSets)
{
	/// <summary>
	/// Represents candidates.
	/// </summary>
	private readonly CandidateMap _candidates = BuildCandidates(grid, rankSets);

	/// <summary>
	/// The backing grid.
	/// </summary>
	private readonly Grid _grid = grid;


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

			if (factAssignmentCountValues.Count == 1)
			{
				return RankSets.Links.Count - factAssignmentCountValues.First();
			}
			return null;
		}
	}

	/// <summary>
	/// Indicates all cells used.
	/// </summary>
	public CellMap Cells => _candidates.Cells;

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
			var fullMap = (Candidate[][])[.. from set in RankSets.Truths select set.GetAvailableRange(_grid).ToArray()];
			var combinations = fullMap.GetExtractedCombinations();
			foreach (var combination in combinations)
			{
				var candidates = combination.AsCandidateMap();

				// Check satisfiability.
				var areAllRankSetsSatisfied = true;
				foreach (var rankSet in RankSets)
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
	public RankSetCollection RankSets { get; } = rankSets;


	/// <inheritdoc/>
	public override string ToString()
	{
		var truths = RankSets.Truths;
		var links = RankSets.Links;
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
