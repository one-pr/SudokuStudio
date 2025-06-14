namespace Sudoku.Analytics.Ranking;

/// <summary>
/// Represents an object that can calculate rank-related information via the specified data.
/// </summary>
/// <param name="grid">The grid.</param>
/// <param name="truths">The truths.</param>
/// <param name="links">The links.</param>
[TypeImpl(TypeImplFlags.Object_Equals | TypeImplFlags.Object_GetHashCode | TypeImplFlags.Equatable | TypeImplFlags.EqualityOperators)]
public sealed partial class RankPattern(in Grid grid, SpaceSet truths, SpaceSet links) :
	IEquatable<RankPattern>,
	IEqualityOperators<RankPattern, RankPattern, bool>
{
	/// <summary>
	/// Represents candidates.
	/// </summary>
	private readonly CandidateMap _candidates = BuildCandidates(grid, truths, links);

	/// <summary>
	/// The backing grid.
	/// </summary>
	[EquatableMember]
	[HashCodeMember]
	private readonly Grid _grid = grid;


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
	/// Indicates the truths.
	/// </summary>
	[EquatableMember]
	[HashCodeMember]
	public SpaceSet Truths { get; } = truths;

	/// <summary>
	/// Indicates the links.
	/// </summary>
	[EquatableMember]
	[HashCodeMember]
	public SpaceSet Links { get; } = links;


	/// <summary>
	/// Indicates whether the current pattern is stable rank-0 pattern, i.e. all links are rank-0 sets.
	/// </summary>
	public bool GetIsRank0Pattern() => GetRank0Sets() == Links;

	/// <summary>
	/// Indicates the rank of the current pattern. If the pattern is unstable
	/// (sometimes assignments certain times of digits in the pattern but sometimes not),
	/// this property will return <see langword="null"/>.
	/// </summary>
	public int? GetRank()
	{
		var factAssignmentCountValues = new HashSet<int>();
		foreach (var l in from assignment in GetAssignmentCombinations() select assignment.Length)
		{
			factAssignmentCountValues.Add(l);
		}
		return factAssignmentCountValues.Count == 1 ? Links.Count - factAssignmentCountValues.First() : null;
	}

	/// <inheritdoc/>
	public override string ToString() => $"T{Truths.Count} = {Truths}, L{Links.Count} = {Links}";

	/// <summary>
	/// Gets the full string of the current pattern, including its details (rank, eliminations and so on).
	/// </summary>
	/// <returns>The string.</returns>
	public string ToFullString()
		=> string.Format(
			SR.Get("RankInfo"),
			_grid.ToString("@:"),
			ToString(),
			GetAssignmentCombinations().Length,
			GetRank()?.ToString() ?? SR.Get("UnstableRank"),
			GetEliminations().ToString(),
			GetRank0Sets().ToString(),
			SR.Get(GetIsRank0Pattern() ? "IsRank0Pattern" : "IsNotRank0Pattern")
		);

	/// <summary>
	/// Indicates eliminations can be found in the current pattern.
	/// </summary>
	public CandidateMap GetEliminations()
	{
		var result = CandidateMap.Empty;
		var i = 0;
		var candidatesMap = _grid.CandidatesMap;
		foreach (var assignmentGroup in GetAssignmentCombinations())
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

	/// <summary>
	/// Returns a list of <see cref="Candidate"/> group that describes the valid assignments.
	/// </summary>
	/// <returns>Valid assignments.</returns>
	public ReadOnlySpan<ReadOnlyMemory<Candidate>> GetAssignmentCombinations()
	{
		var result = new List<ReadOnlyMemory<Candidate>>();

		var assignments = CandidateMap.Empty;
		var fullMap = from set in Truths.ToArray() select set.GetAvailableRange(_grid).ToArray();
		var combinations = fullMap.GetExtractedCombinations();
		foreach (var combination in combinations)
		{
			var candidates = combination.AsCandidateMap();

			// Check satisfiability.
			var areAllRankSetsSatisfied = true;
			foreach (var (rankSets, isTruth) in ((Truths, true), (Links, false)))
			{
				foreach (var rankSet in rankSets)
				{
					if (!rankSet.IsSatisfied(candidates, isTruth))
					{
						areAllRankSetsSatisfied = false;
						goto CheckFlag;
					}
				}
			}
		CheckFlag:
			if (!areAllRankSetsSatisfied)
			{
				continue;
			}

			// The combination is okay to be added.
			result.Add(combination);
		}

		return result.AsSpan();
	}

	/// <summary>
	/// Try to find all rank-0 sets.
	/// </summary>
	public SpaceSet GetRank0Sets()
	{
		var result = Links;
		var links = result;

		var i = 0;
		foreach (var assignmentGroup in GetAssignmentCombinations())
		{
			var lightUpLinks = SpaceSet.Empty;
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


	/// <summary>
	/// Build candidates.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="truths">The truths.</param>
	/// <param name="links">The links.</param>
	private static CandidateMap BuildCandidates(in Grid grid, SpaceSet truths, SpaceSet links)
	{
		var result = CandidateMap.Empty;

		var candidatesMap = grid.CandidatesMap;
		foreach (var truth in truths)
		{
			switch (truth)
			{
				case { IsCellRelated: true, Cell: var cell }:
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						result.Add(cell * 9 + digit);
					}
					break;
				}
				case { IsHouseRelated: true, House: var house, Digit: var digit }:
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
