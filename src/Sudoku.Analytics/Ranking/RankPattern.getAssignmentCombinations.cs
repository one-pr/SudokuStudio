namespace Sudoku.Ranking;

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
