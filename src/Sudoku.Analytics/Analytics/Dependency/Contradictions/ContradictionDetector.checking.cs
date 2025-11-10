#undef NAKED_SINGLE_FIRST
#undef MINIMUM_REMAINING_VALUES
#undef MINIMUM_REMAINING_VALUES_USING_DESCENDING_ORDER
#if !MINIMUM_REMAINING_VALUES && MINIMUM_REMAINING_VALUES_USING_DESCENDING_ORDER
#undef MINIMUM_REMAINING_VALUES_USING_DESCENDING_ORDER
#endif

namespace Sudoku.Analytics.Dependency.Contradictions;

public partial class ContradictionDetector
{
	/// <summary>
	/// Represents equality comparer for typed assignment pair.
	/// </summary>
	private static readonly EqualityComparer<(DependencyAssignment Assignment, DependencyNodeType Type)> AssignmentComparer =
		EqualityComparer<(DependencyAssignment, DependencyNodeType)>.Create(
			static (l, r) => l.Item1 == r.Item1,
			static obj => obj.Item1.GetHashCode()
		);

	/// <summary>
	/// Indicates empty instance of <see cref="DependencyAssignment"/> sequence.
	/// </summary>
	private static readonly ReadOnlyMemory<DependencyAssignment> EmptyAssignment = ReadOnlyMemory<DependencyAssignment>.Empty;


	/// <summary>
	/// Do a fast check, to determine whether the candidate can be a valid elimination or not.
	/// </summary>
	/// <param name="grid">The grid to be checked.</param>
	/// <param name="cell">The start cell to check.</param>
	/// <param name="digit">The digit to check.</param>
	/// <param name="includesGroupedNodes">Indicates whether the searching method will includes grouped nodes.</param>
	/// <returns>A <see cref="bool"/> result indicating whether the grid can lead a conflict or not.</returns>
	private static bool DoFastTryAndError(in Grid grid, Cell cell, Digit digit, bool includesGroupedNodes)
	{
		// Fast check. Just assign it and find for conclusions.
		var firstAssignment = new DependencyAssignment(cell * 9 + digit);
		var tempGrid = grid;
		Update(ref tempGrid, firstAssignment, out _);

		bool isChanged;
		do
		{
			isChanged = false;

			if (TryFindContradiction(tempGrid, out _))
			{
				return true;
			}

			// Find for the next conclusion.
			var collector = new HashSet<(DependencyAssignment Assignment, DependencyNodeType)>(AssignmentComparer);

			// Collect for valid next steps.
#if NAKED_SINGLE_FIRST
			FindNakedSingle(tempGrid, collector);
			FindHiddenSingle(tempGrid, collector);
#else
			FindHiddenSingle(tempGrid, collector);
			FindNakedSingle(tempGrid, collector);
#endif
			if (includesGroupedNodes)
			{
				FindLockedCandidates(tempGrid, collector);
			}

			if (collector.Count != 0)
			{
				isChanged = true;
				foreach (var pair in collector)
				{
					Update(ref tempGrid, pair.Assignment, out _);
				}
			}
		} while (isChanged);

		// No contradictions found.
		return false;
	}

	/// <summary>
	/// Checks for the grid, to determine whether the grid can cause a house that cannot fill a digit,
	/// or a cell has no valid candidates, if the specified cell is filled with the specified digit.
	/// </summary>
	/// <param name="grid">The grid to be checked.</param>
	/// <param name="cell">The start cell to check.</param>
	/// <param name="digit">The digit to check.</param>
	/// <param name="includesGroupedNodes">Indicates whether the searching method will includes grouped nodes.</param>
	/// <param name="lastNode">Indicates the node branch that causes invalid states.</param>
	/// <param name="emptySpace">The space that is going to be empty.</param>
	/// <returns>A <see cref="bool"/> result indicating whether the grid can lead a conflict or not.</returns>
	private static bool LeadsToEmpty(
		in Grid grid,
		Cell cell,
		Digit digit,
		bool includesGroupedNodes,
		[NotNullWhen(true)] out DependencyNode? lastNode,
		out Space emptySpace
	)
	{
		// Create a queue and enqueue root node.
		var queue = new Queue<DependencyNode>();

		// Define the root supposing node and enqueue it.
		var firstAssignment = new DependencyAssignment(cell * 9 + digit);
		var firstAssignmentGrid = grid;
		Update(ref firstAssignmentGrid, firstAssignment, out _);
		queue.Enqueue(
			new(
				DependencyNodeType.Supposing,
				firstAssignmentGrid,
				firstAssignment,
				EmptyAssignment,
				new(DependencyNodeType.Root, grid, default, EmptyAssignment, null)
			)
		);

		// Performs a BFS to iterate.
		while (queue.Count != 0)
		{
			// Dequeue a node.
			var node = queue.Dequeue();
			ref readonly var tempGrid = ref node.Grid;

			// Check any contradiction.
			if (TryFindContradiction(tempGrid, out emptySpace))
			{
				// Any contradiction found.
				lastNode = node;
				return true;
			}

			// Find for the next conclusion.
			var collector = new HashSet<(DependencyAssignment Assignment, DependencyNodeType Type)>(AssignmentComparer);

			// Collect for valid next steps.
#if NAKED_SINGLE_FIRST
			FindNakedSingle(tempGrid, collector);
			FindHiddenSingle(tempGrid, collector);
#else
			FindHiddenSingle(tempGrid, collector);
			FindNakedSingle(tempGrid, collector);
#endif
			if (includesGroupedNodes)
			{
				FindLockedCandidates(tempGrid, collector);
			}

			if (collector.Count == 0)
			{
				// No conclusions found - no contradiction and no conclusions. Discard this branch.
				continue;
			}

			// Define a sibling of assignments (they are of a same depth).
			var collectedSiblings = from a in collector.ToArray() select a.Assignment;

#if MINIMUM_REMAINING_VALUES
			// Iterate on collected assignments, and store them into a temporary collection,
			// in order to perform MRV strategy of the number of candidates.
			var mrvNodes = new SortedList<int, List<DependencyNode>>(
				collector.Count
#if MINIMUM_REMAINING_VALUES_USING_DESCENDING_ORDER
				,
				Comparer<int>.Create(static (left, right) => right - left)
#endif
			);
#endif
			foreach (var (assignment, type) in collector)
			{
				// Branch pruning: we should add the node into all parent nodes, in order to avoid searching them twice.
				// Check whether the current ancestor node can directly connect to the current assignment.
				// If so, we can prune the branch due to <c>A -> B</c> rather than <c>A -> C -> B</c>.
				var anyAncestorNodeIncludesThisAssignment = false;
				foreach (var ancestor in node.EnumerateAncestors())
				{
					if (ancestor.SiblingAssignments.Span.Contains(assignment))
					{
						// This assignment must be handled in ancestor nodes.
						anyAncestorNodeIncludesThisAssignment = true;
						break;
					}
				}
				if (anyAncestorNodeIncludesThisAssignment)
				{
					continue;
				}

				// Add this node into list.
				var tempGridUpdated = tempGrid;
				Update(ref tempGridUpdated, assignment, out var removedCandidates);
				var nextNode = new DependencyNode(type, tempGridUpdated, assignment, collectedSiblings, node);

				// Add it into sorted list.
#if MINIMUM_REMAINING_VALUES
				if (!mrvNodes.TryAdd(removedCandidates.Length, [nextNode]))
				{
					mrvNodes[removedCandidates.Length].Add(nextNode);
				}
#else
				queue.Enqueue(nextNode);
#endif
			}

#if MINIMUM_REMAINING_VALUES
			// Enqueue all nodes.
			foreach (var nodes in mrvNodes.Values)
			{
				nodes.ForEach(queue.Enqueue);
			}
#endif
		}

		lastNode = null;
		emptySpace = default;
		return false;
	}

	/// <summary>
	/// Try to find contradiction of the specified, searching for a space (house or cell) has no valid digit to be filled.
	/// If found, the space will be recorded in argument <paramref name="result"/>.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="result">The result space causing an empty.</param>
	/// <returns>A <see cref="bool"/> result indicating whether there's any contradiction.</returns>
	private static bool TryFindContradiction(in Grid grid, out Space result)
	{
		// Check cell.
		foreach (var cell in grid.EmptyCells)
		{
			if (grid.GetCandidates(cell) == 0)
			{
				result = Space.RowColumn(cell / 9, cell % 9);
				return true;
			}
		}

		// Check house.
		for (var digit = 0; digit < 9; digit++)
		{
			for (var house = 0; house < 27; house++)
			{
				var count = 0;
				foreach (var cell in HousesMap[house])
				{
					var cellState = grid.GetState(cell);
					if (grid.GetDigit(cell) == digit)
					{
						goto NextHouse;
					}
					if (grid.Exists(cell, digit) is true)
					{
						count++;
					}
				}
				if (count == 0)
				{
					result = house switch
					{
						< 9 => Space.BlockDigit(house, digit),
						< 18 => Space.RowDigit(house - 9, digit),
						_ => Space.ColumnDigit(house - 18, digit)
					};
					return true;
				}

			NextHouse:;
			}
		}

		result = default;
		return false;
	}

	/// <summary>
	/// Create a new grid that applies the specified assignment.
	/// </summary>
	/// <param name="grid">The grid to be updated.</param>
	/// <param name="assignment">The assignment.</param>
	/// <param name="removedCandidates">Removed candidates.</param>
	private static void Update(scoped ref Grid grid, DependencyAssignment assignment, out ReadOnlySpan<Candidate> removedCandidates)
	{
		var r = new HashSet<Candidate>();
		_ = assignment is (var digit, { PeerIntersection: var peerCells } cells);
		foreach (var peerCell in peerCells)
		{
			ref var mask = ref grid[peerCell];
			if (MaskToCellState(mask) == CellState.Empty && (mask >> digit & 1) != 0)
			{
				mask &= (Mask)~(1 << digit);
				r.Add(peerCell * 9 + digit);
			}
		}

		// Set digit for the current cell.
		if (cells is [var cell])
		{
			foreach (var d in (Mask)(grid.GetCandidates(cell) & ~(1 << digit)))
			{
				r.Add(cell * 9 + d);
			}

			// Updates mask of the target cell.
			grid[cell] = (Mask)(GetHeaderBits(in grid, cell) | Grid.ModifiableMask | 1 << digit);
		}

		removedCandidates = r.ToArray();
	}
}
