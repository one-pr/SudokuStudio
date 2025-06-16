namespace Sudoku.Analytics.Ranking;

/// <summary>
/// Represents an object that can calculate rank-related information via the specified data.
/// </summary>
/// <param name="grid">The grid.</param>
/// <param name="truths">The truths.</param>
/// <param name="links">The links.</param>
[TypeImpl(TypeImplFlags.Object_Equals | TypeImplFlags.EqualityOperators)]
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
	/// Indicates whether the current pattern is stable rank-0 pattern, i.e. all links are rank-0 sets.
	/// </summary>
	public bool GetIsRank0Pattern() => GetRank0Sets() == Links;

	/// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
	public bool Equals(in RankPattern other) => Grid == other.Grid && Truths == other.Truths && Links == other.Links;

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(Grid, Truths, Links);

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
			Grid.ToString("@:"),
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
		var candidatesMap = Grid.CandidatesMap;
		foreach (var assignmentGroup in GetAssignmentCombinations())
		{
			var current = CandidateMap.Empty;
			foreach (var assignment in assignmentGroup)
			{
				var cell = assignment / 9;
				var digit = assignment % 9;
				foreach (var otherDigit in (Mask)(Grid.GetCandidates(cell) & ~(1 << digit)))
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

		// Create a queue to record all possible cases, in BFS way.
		var queue = new LinkedList<(CandidateMap State, int[] RemainingTruthIndices)>();
		queue.AddLast(([], [.. SpanEnumerable.Range(Truths.Count)]));

		// Iterate the whole queue until the queue becomes empty.

#if DEBUG
		// Provides a way to view max capacity while queuing.
		var max = 0;
#endif
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
				if (Links.All(link => link.IsSatisfied(nextState, false)))
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

					queue.AddLast((nextState, [.. newRemainingTruths]));
				}
			}
		}

		return result.AsSpan();
	}

	/// <summary>
	/// Try to find all rank-0 sets.
	/// </summary>
	public SpaceSet GetRank0Sets()
	{
		var result = SpaceSet.Empty;
		var links = Links;

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

	/// <inheritdoc/>
	bool IEquatable<RankPattern>.Equals(RankPattern other) => Equals(other);


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
