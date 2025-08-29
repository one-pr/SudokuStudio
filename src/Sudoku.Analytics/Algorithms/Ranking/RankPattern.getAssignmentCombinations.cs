namespace Sudoku.Algorithms.Ranking;

public partial struct RankPattern
{
	/// <summary>
	/// Returns a list of <see cref="Candidate"/> group that describes the valid assignments.
	/// </summary>
	/// <returns>Valid assignments.</returns>
	public ReadOnlySpan<ReadOnlyMemory<Candidate>> GetAssignmentCombinations() => GetAssignmentCombinationsCore(out _);

	/// <summary>
	/// Gets assignment combinations with full links.
	/// </summary>
	/// <param name="links">The full links.</param>
	/// <returns>Assignment combinations.</returns>
	private ReadOnlySpan<ReadOnlyMemory<Candidate>> GetAssignmentCombinationsCore(out SpaceSet links)
	{
		(links, var result) = (SpaceSet.Empty, new List<ReadOnlyMemory<Candidate>>());

		// Create a queue to record all possible cases, in BFS way.
		var queue = new LinkedList<CombinationQueueNode>();
		queue.AddLast(new CombinationQueueNode(-1, [.. SpanEnumerable.Range(Truths.Count)], null));

		// Iterate the whole queue until the queue becomes empty.
		while (queue.Count != 0)
		{
			// Dequeue a node.
			// There're the following values can be used:
			//   * currentState: The current candidates applied. To combine all of them it'll be the current assignment combination.
			//   * remainingTruths: The remaining truth, as corresponding indices of truth space set.
			//   * parent: The parent node.
			var currentNode = queue.RemoveFirstNode();
			var (_, remainingTruths, parent) = currentNode;
			var currentState = currentNode.State;

			// Check whether the node has already finished.
			if (remainingTruths.Length == 0)
			{
				result.Add(currentState.ToArray());
				links |= currentNode.GetProducedLinks(Grid, Truths);
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
			var (selectedIndex, remainingCandidates) = sorted[0];
			var newRemainingTruths = new List<int>();
			foreach (var truthIndex in remainingTruths)
			{
				if (truthIndex != selectedIndex)
				{
					newRemainingTruths.Add(truthIndex);
				}
			}
			foreach (var remainingCandidate in remainingCandidates)
			{
				var nextState = currentState + remainingCandidate;

				// Check whether the remaining truths, preventing truth overlapped cases.
				var overlapped = new List<int>();
				foreach (var truthIndex in newRemainingTruths)
				{
					var overlappingFlag = false;
					foreach (var assigned in nextState)
					{
						if (Truths[truthIndex].Contains(assigned))
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

				queue.AddLast(new CombinationQueueNode(remainingCandidate, [.. newRemainingTruths], currentNode));

				// Backtrack: Revert operation on removing on overlapped truths.
				foreach (var truthIndex in overlapped)
				{
					newRemainingTruths.Add(truthIndex);
				}
			}
		}

		return result.AsSpan();
	}
}

/// <summary>
/// Represents a queue node that is used in calculating assignment combinations.
/// </summary>
/// <param name="Set">Indicates the set (assignment).</param>
/// <param name="RemainingTruthIndices">Indicates the remaining truth indices.</param>
/// <param name="Parent">Indicates the parent node.</param>
file sealed record CombinationQueueNode(Candidate Set, int[] RemainingTruthIndices, CombinationQueueNode? Parent)
{
	/// <summary>
	/// Indicates the states of node (assignments in this branch).
	/// </summary>
	public CandidateMap State
	{
		get
		{
			var result = CandidateMap.Empty;
			for (var node = this; node.Parent is not null; node = node.Parent)
			{
				result.Add(node.Set);
			}
			return result;
		}
	}


	/// <summary>
	/// Find for produced links on this branch of nodes in the grid.
	/// </summary>
	/// <param name="grid">The original grid.</param>
	/// <param name="truths">Indicates the truths.</param>
	/// <returns>The spaces of the links.</returns>
	public SpaceSet GetProducedLinks(in Grid grid, in SpaceSet truths)
	{
		// Calculate candidates appeared in truths, and group each candidate by its containing truth.
		var pattern = CandidateMap.Empty;
		var truthLookup = new Dictionary<Candidate, SpaceSet>();
		foreach (var truth in truths)
		{
			foreach (var candidate in truth.GetAvailableRange(grid))
			{
				pattern.Add(candidate);
				if (!truthLookup.TryAdd(candidate, [truth]))
				{
					truthLookup[candidate].Add(truth);
				}
			}
		}

		// Iterate on the nodes chain, to get all assignment and effected candidates (removed from truths).
		var linkLookup = new Dictionary<Space, CandidateMap>();
		for (var (node, previousGrid) = (this, grid); node.Parent is not null; node = node.Parent)
		{
			var assignment = node.Set;
			var assignmentCell = assignment / 9;
			var assignmentDigit = assignment % 9;
			ref readonly var assignmentTruths = ref truthLookup.GetValueRef(assignment);

			// Apply the node.
			var tempGrid = previousGrid;
			tempGrid.SetDigit(assignmentCell, assignmentDigit);

			// Calculate removed candidates.
			foreach (var removedCandidate in pattern)
			{
				if (removedCandidate == assignment)
				{
					// Skip for equality case.
					continue;
				}

				if (previousGrid.Exists(removedCandidate) is false || tempGrid.Exists(removedCandidate) is true)
				{
					// The current grid must be removed, it checks 2 places (original grid and new produced grid).
					// If new grid doesn't contain that candidate, but original grid contains,
					// it will be a valid removed candidate.
					continue;
				}

				// This candidate is removed.
				// Check whether it is on the same truth as the assignment.
				ref readonly var removedCandidateContainingTruths = ref truthLookup.GetValueRef(removedCandidate);
				if (assignmentTruths & removedCandidateContainingTruths)
				{
					// There're several truths held by both the assignment and the removed candidate.
					continue;
				}

				// Otherwise, the removed candidate belongs to a truth that is not related to assignment.
				// We should collect it and calculate space relation between those two candidates
				// (assignment and this removed candidate).

				// Check cell.
				var removedCell = removedCandidate / 9;
				var removedDigit = removedCandidate % 9;
				if (removedCell == assignmentCell)
				{
					// The link is a cell link.
					var cellLink = Space.RowColumn(removedCell / 9, removedCell % 9);
					if (!truths.Contains(cellLink) && !linkLookup.TryAdd(cellLink, removedCandidate.AsCandidateMap()))
					{
						linkLookup[cellLink].Add(removedCandidate);
					}
					continue;
				}

				// Check validity on same house.
				if (!PeersMap[removedCell].Contains(assignmentCell) || removedDigit != assignmentDigit)
				{
					continue;
				}

				// Check row or column.
				var pairMap = assignmentCell.AsCellMap() + removedCell;
				var line = pairMap.SharedLine;
				var isLineSatisfied = line != FallbackConstants.@int;
				if (isLineSatisfied)
				{
					// The link is a line link (row or column link).
					var lineLink = line switch
					{
						>= 9 and < 18 => Space.RowDigit(line - 9, removedDigit),
						>= 18 => Space.ColumnDigit(line - 18, removedDigit)
					};
					if (!truths.Contains(lineLink) && !linkLookup.TryAdd(lineLink, removedCandidate.AsCandidateMap()))
					{
						linkLookup[lineLink].Add(removedCandidate);
					}
				}

				// Check block.
				if (pairMap.SharedBlock is var block and not FallbackConstants.@int && !isLineSatisfied)
				{
					// The link is a block link.
					// However, we should ignore the case when it can also be treated as a row / column link,
					// in order to keep space relations unique, unified and "greedy".
					var blockLink = Space.BlockDigit(block, removedDigit);
					if (!truths.Contains(blockLink) && !linkLookup.TryAdd(blockLink, removedCandidate.AsCandidateMap()))
					{
						linkLookup[blockLink].Add(removedCandidate);
					}
				}
			}
		}

		return [.. linkLookup.Keys];
	}
}
