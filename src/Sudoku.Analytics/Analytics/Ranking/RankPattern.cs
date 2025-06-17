namespace Sudoku.Analytics.Ranking;

/// <summary>
/// Represents an object that can calculate rank-related information via the specified data.
/// </summary>
/// <param name="grid">The grid.</param>
/// <param name="truths">The truths.</param>
/// <param name="links">The links.</param>
[TypeImpl(TypeImplFlags.EqualityOperators)]
public readonly ref partial struct RankPattern(in Grid grid, in SpaceSet truths, in SpaceSet links) : IEquatable<RankPattern>
{
	/// <summary>
	/// Indicates the grid.
	/// </summary>
	public readonly ref readonly Grid Grid = ref grid;

	/// <summary>
	/// Indicates the truths.
	/// </summary>
	public readonly ref readonly SpaceSet Truths = ref truths;

	/// <summary>
	/// Indicates the links.
	/// </summary>
	public readonly ref readonly SpaceSet Links = ref links;

	/// <summary>
	/// Represents candidates.
	/// </summary>
	private readonly CandidateMap _candidates = BuildCandidates(grid, truths, links);


	/// <summary>
	/// Indicates all cells used.
	/// </summary>
	public CellMap Cells => _candidates.Cells;

	/// <summary>
	/// Indicates the candidates.
	/// </summary>
	[UnscopedRef]
	public ref readonly CandidateMap Candidates => ref _candidates;


	/// <summary>
	/// Indicates whether the current pattern is stable rank-0 pattern, i.e. all links are rank-0 links.
	/// </summary>
	public bool GetIsRank0Pattern() => GetRank0Links() == Links;

	/// <inheritdoc/>
	[Obsolete($"This method always return false. Ref structs cannot be boxed so argument '{nameof(obj)}' must be a different instance.", false)]
	public override bool Equals(object? obj) => false;

	/// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
	public bool Equals(in RankPattern other) => Grid == other.Grid && Truths == other.Truths && Links == other.Links;

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(Grid, Truths, Links);

	/// <summary>
	/// Indicates the rank of the current pattern. If the pattern is unstable
	/// (sometimes assignments certain times of digits in the pattern but sometimes not),
	/// this property will return <see langword="null"/>.
	/// </summary>
	public int? GetRank() => GetRankCore(GetAssignmentCombinations());

	/// <inheritdoc cref="object.ToString"/>
	public override string ToString() => $"T{Truths.Count} = {Truths}, L{Links.Count} = {Links}";

	/// <summary>
	/// Gets the full string of the current pattern, including its details (rank, eliminations and so on).
	/// </summary>
	/// <returns>The string.</returns>
	public unsafe string ToFullString()
	{
		var combinations = GetAssignmentCombinations();
		return string.Format(
			SR.Get("RankInfo"),
			Grid.ToString("@:"),
			ToString(),
			combinations.Length,
			GetRankCore(combinations)?.ToString() ?? SR.Get("UnstableRank"),
			GetEliminationsCore(combinations).ToString(),
			GetEliminationZoneCore(combinations, true).ToString(),
			GetRank0LinksCore(combinations).ToString(),
			SR.Get(GetIsRank0PatternCore(combinations) ? "IsRank0Pattern" : "IsNotRank0Pattern")
		);
	}

	/// <summary>
	/// Indicates eliminations can be found in the current pattern.
	/// </summary>
	/// <remarks>
	/// In theory, eliminations may not require any links. All conclusions come from valid combinations of truths,
	/// keeping one valid digit filling into each truth, and find intersections of eliminations can be found from all cases.
	/// </remarks>
	public unsafe CandidateMap GetEliminations() => GetEliminationsCore(GetAssignmentCombinations());

	/// <summary>
	/// Returns a list of <see cref="Candidate"/> group that describes the valid assignments.
	/// </summary>
	/// <returns>Valid assignments.</returns>
	public ReadOnlySpan<ReadOnlyMemory<Candidate>> GetAssignmentCombinations()
	{
		var result = new List<ReadOnlyMemory<Candidate>>();

		// Create a queue to record all possible cases, in BFS way.
		var queue = new LinkedList<CombinationQueueNode>();
		queue.AddLast(new CombinationQueueNode([], [.. SpanEnumerable.Range(Truths.Count)]));

#if DEBUG
		// Provides a way to view max capacity while queuing.
		var max = 0;
#endif
		// Iterate the whole queue until the queue becomes empty.
		while (queue.Count != 0)
		{
#if DEBUG
			if (queue.Count >= max)
			{
				max = queue.Count;
			}
#endif

			// Dequeue a node.
			var (currentState, remainingTruths) = queue.RemoveFirstNode();

			// Check whether the node has already finished.
			if (remainingTruths.Length == 0)
			{
				// Verify links.
				var flag = true;
				foreach (var link in Links)
				{
					if (!link.IsSatisfied(currentState, false))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					result.Add(currentState.ToArray());
				}
				continue;
			}

			// Heuristic searching:
			// We should firstly check for truths with less number of remaining positions.
			// However, if we have found at least one house having no valid positions to be filled,
			// because we must select a candidate to be filled, so it cause a conflict,
			// meaning the current combination is invalid.
			var tempProjectedValues = new List<(int, CandidateMap Remaining)>();
			var tempGrid = Grid;
			foreach (var state in currentState)
			{
				tempGrid.SetDigit(state / 9, state % 9);
			}
			foreach (var index in remainingTruths)
			{
				tempProjectedValues.AddRef((index, Truths[index].GetAvailableRange(tempGrid)));
			}
			var sorted =
				from x in tempProjectedValues.AsSpan()
				orderby x.Remaining.Count
				select x into x
				select x;

			// Check whether the collection is valid.
			if (sorted.Length == 0 || sorted.Any(static value => value.Remaining.Count == 0))
			{
				continue;
			}

			// Valid. Now add children nodes.
			var (selectedIndex, candidates) = sorted[0];
			var newRemainingTruths = new List<int>();
			foreach (var truthIndex in remainingTruths)
			{
				if (truthIndex != selectedIndex)
				{
					newRemainingTruths.Add(truthIndex);
				}
			}
			foreach (var candidate in candidates)
			{
				var nextState = currentState + candidate;
				if (Links.TrueForAll(link => link.IsSatisfied(nextState, false)))
				{
					// Check whether the remaining truths, preventing truth overlapped cases.
					var overlapped = new List<int>();
					foreach (var truthIndex in newRemainingTruths)
					{
						var overlappingFlag = false;
						foreach (var assigned in nextState)
						{
							if (Truths[truthIndex].ContainsAssignment(assigned))
							{
								overlappingFlag = true;
								break;
							}
						}
						if (overlappingFlag)
						{
							overlapped.Add(truthIndex);
						}
					}
					foreach (var truthIndex in overlapped)
					{
						newRemainingTruths.Remove(truthIndex);
					}

					queue.AddLast(new CombinationQueueNode(nextState, [.. newRemainingTruths]));
				}
			}
		}

		return result.AsSpan();
	}

	/// <summary>
	/// Try to find all rank-0 links. A rank-0 link is a link that will become truth
	/// because all valid combinations lead to a same result that the link must hold one correct digit.
	/// </summary>
	/// <returns>A list of links that are determined as rank-0 links.</returns>
	public SpaceSet GetRank0Links() => GetRank0LinksCore(GetAssignmentCombinations());

	/// <summary>
	/// Find elimination zones, indicating a list of candidates that can be eliminated,
	/// no matter whether they exist or not.
	/// </summary>
	/// <param name="linksOnly">
	/// Indicates whether the elimination zone only checks for links.
	/// If <see langword="true"/>, candidates can only be created from links.
	/// </param>
	/// <returns>A list of candidates.</returns>
	public CandidateMap GetEliminationZone(bool linksOnly) => GetEliminationZoneCore(GetAssignmentCombinations(), linksOnly);

	/// <inheritdoc/>
	bool IEquatable<RankPattern>.Equals(RankPattern other) => Equals(other);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool GetIsRank0PatternCore(ReadOnlySpan<ReadOnlyMemory<Candidate>> combinations)
		=> GetRank0LinksCore(combinations) == Links;

	private int? GetRankCore(ReadOnlySpan<ReadOnlyMemory<Candidate>> combinations)
	{
		var factAssignmentCountValues = new HashSet<int>();
		foreach (var l in from assignment in combinations select assignment.Length)
		{
			factAssignmentCountValues.Add(l);
			if (factAssignmentCountValues.Count >= 2)
			{
				// Invalid, fast fail.
				return null;
			}
		}
		return factAssignmentCountValues.Count == 1 ? Links.Count - factAssignmentCountValues.First() : null;
	}

	private unsafe CandidateMap GetEliminationsCore(
		ReadOnlySpan<ReadOnlyMemory<Candidate>> combinations,
		delegate*<ref readonly Grid, Cell, Digit, Mask> otherDigitCalculator = null,
		delegate*<ref readonly Grid, Cell, Digit, CellMap> otherCellsCalculator = null
	)
	{
		if (otherDigitCalculator == null)
		{
			otherDigitCalculator = &otherDigitsCalc;
		}
		if (otherCellsCalculator == null)
		{
			otherCellsCalculator = &otherCellsCalc;
		}

		var result = CandidateMap.Empty;
		var i = 0;
		foreach (var assignmentGroup in combinations)
		{
			var current = CandidateMap.Empty;
			foreach (var assignment in assignmentGroup)
			{
				var cell = assignment / 9;
				var digit = assignment % 9;
				foreach (var otherDigit in otherDigitCalculator(in Grid, cell, digit))
				{
					current.Add(cell * 9 + otherDigit);
				}
				foreach (var otherCell in otherCellsCalculator(in Grid, cell, digit))
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

		// Remove candidates from truths.
		foreach (var truth in Truths)
		{
			result &= ~truth.GetRange();
		}

		return result;


		static Mask otherDigitsCalc(ref readonly Grid grid, Cell cell, Digit digit)
			=> (Mask)(grid.GetCandidates(cell) & ~(1 << digit));

		static CellMap otherCellsCalc(ref readonly Grid grid, Cell cell, Digit digit)
			=> PeersMap[cell] & grid.CandidatesMap[digit];
	}

	private SpaceSet GetRank0LinksCore(ReadOnlySpan<ReadOnlyMemory<Candidate>> combinations)
	{
		var result = SpaceSet.Empty;
		var links = Links;

		var i = 0;
		foreach (var assignmentGroup in combinations)
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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe CandidateMap GetEliminationZoneCore(ReadOnlySpan<ReadOnlyMemory<Candidate>> combinations, bool linksOnly)
	{
		var result = GetEliminationsCore(combinations, &otherDigitsCalc, &otherCellsCalc);
		if (linksOnly)
		{
			var candidatesSet = CandidateMap.Empty;
			foreach (var link in Links)
			{
				candidatesSet |= link.GetRange();
			}
			result &= candidatesSet;
		}
		return result;


		static Mask otherDigitsCalc(ref readonly Grid grid, Cell cell, Digit digit) => grid.GetCandidates(cell);

		static CellMap otherCellsCalc(ref readonly Grid grid, Cell cell, Digit digit) => PeersMap[cell] & grid.EmptyCells;
	}


	/// <summary>
	/// Build candidates.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="truths">The truths.</param>
	/// <param name="links">The links.</param>
	private static CandidateMap BuildCandidates(in Grid grid, in SpaceSet truths, in SpaceSet links)
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
