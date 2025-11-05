#define REPORT_ERROR_IF_ALL_PRUNING_SYMBOLS_ARE_DISABLED
#undef NAKED_SINGLE_FIRST
#define NODE_PRUNING
#if !NODE_PRUNING
#if REPORT_ERROR_IF_ALL_PRUNING_SYMBOLS_ARE_DISABLED
#error It is strongly recommended to enable compilation symbols related with pruning; otherwise it will cause large memory analysis and has no optimizaiton.
#else
#warning It is strongly recommended to enable compilation symbols related with pruning; otherwise it will cause large memory analysis and has no optimizaiton.
#endif
#endif

namespace Sudoku.Analytics.Dependency.Contradictions;

/// <summary>
/// Represents a type that can find for conflict that will be used in complex patterns.
/// </summary>
public static class ContradictionDetector
{
	/// <summary>
	/// Represents equality comparer for typed assignment pair.
	/// </summary>
	private static readonly EqualityComparer<(AssignmentInfo Assignment, DependencyNodeType Type)> EqualityComparer =
		EqualityComparer<(AssignmentInfo, DependencyNodeType)>.Create(
			static (l, r) => l.Item1 == r.Item1,
			static obj => obj.Item1.GetHashCode()
		);


	/// <summary>
	/// The global method to check for complex contradiction.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="includesGroupedNodes">Indicates whether grouped nodes should also be checked.</param>
	/// <returns>A list of found conflict.</returns>
	public static ReadOnlySpan<Cause> Check(in Grid grid, bool includesGroupedNodes)
	{
		var result = new List<Cause>();
		if (!grid.IsValid)
		{
			return result.AsSpan();
		}

		var solution = grid.SolutionGrid;

		// Iterate on each empty cell.
		foreach (var cell in grid.EmptyCells)
		{
			// Iterate on each wrong digit.
			foreach (var digit in (Mask)(grid.GetCandidates(cell) & ~(1 << solution.GetDigit(cell))))
			{
				if (LeadsToEmpty(grid, cell, digit, includesGroupedNodes, out var lastNode, out var cause))
				{
					result.Add(new(cell * 9 + digit, lastNode, cause));
				}
			}
		}

		// All candidates are checked.
		return result.AsSpan();
	}

	/// <summary>
	/// Checks for the grid, to determine whether the grid can cause a house that cannot fill a digit,
	/// or a cell has no valid candidates, if the specified cell is filled with the specified digit.
	/// </summary>
	/// <param name="playground">The grid to be checked.</param>
	/// <param name="cell">The start cell to check.</param>
	/// <param name="digit">The digit to check.</param>
	/// <param name="includesGroupedNodes">Indicates whether the searching method will includes grouped nodes.</param>
	/// <param name="lastNode">Indicates the node branch that causes invalid states.</param>
	/// <param name="emptySpace">The space that is going to be empty.</param>
	/// <returns>A <see cref="bool"/> result indicating whether the grid can lead a conflict or not.</returns>
	/// <remarks>
	/// <para>
	/// We know that the assignments may form a complex graph, for example:
	/// <code>
	///      A
	///     / \
	///    B   C
	///   / \   \
	///  D   E  (F)
	///  |
	/// (F)
	/// </code>
	/// Here, we can store connections like <c>C -> F</c> into the map,
	/// in order not to traverse other links to F (like <c>B -> D -> F</c>).
	/// </para>
	/// <para>
	/// However, here may cause a potential problem: we can only know the assignment relation, but ignoring grid states.
	/// For example, if we have <c>A -> C</c>, this map will ignore all connections from <c>A</c> to <c>C</c>,
	/// no matter how a grid is from.
	/// </para>
	/// <para>
	/// The correct implementation in fact is to store branches that each candidate can reach, and their updated grids;
	/// or store the whole branch. Only for the whole branch can be pruned.
	/// </para>
	/// </remarks>
	private static bool LeadsToEmpty(
		in Grid playground,
		Cell cell,
		Digit digit,
		bool includesGroupedNodes,
		[NotNullWhen(true)] out DependencyNode? lastNode,
		out Space emptySpace
	)
	{
		// Create a queue and enqueue root node.
		var queue = new Queue<DependencyNode>();

#if NODE_PRUNING
		// Defines a map of assignment relations that will deduplicate assignments by connections.
		var assignmentMap = new Dictionary<AssignmentInfo, HashSet<AssignmentInfo>>();
#endif

		var firstAssignment = new AssignmentInfo(cell * 9 + digit);
		queue.Enqueue(
			new(
				DependencyNodeType.Supposing,
				GetUpdatedGrid(/*copies*/ playground, in firstAssignment, out _),
				firstAssignment,
				new(DependencyNodeType.Root, playground, null, null)
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
			var collector = new HashSet<(AssignmentInfo, DependencyNodeType)>(EqualityComparer);

			// Collect for valid next steps.
#if NAKED_SINGLE_FIRST
			FindNakedSingle(tempGrid, node, collector);
			FindHiddenSingle(tempGrid, node, collector);
#else
			FindHiddenSingle(tempGrid, node, collector);
			FindNakedSingle(tempGrid, node, collector);
#endif
			if (includesGroupedNodes)
			{
				FindLockedCandidates(tempGrid, node, collector);
			}

			foreach (var (assignment, type) in collector)
			{
				// Determine whether the next node is worth to be added.
				var parentAssignment = node.Assignment!.Value;
#if NODE_PRUNING
				if (assignmentMap.TryAdd(assignment, [parentAssignment]) || assignmentMap[assignment].Add(parentAssignment))
#endif
				{
					queue.Enqueue(new(type, GetUpdatedGrid(/*copies*/ tempGrid, in assignment, out _), assignment, node));
				}
			}
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
	/// <param name="original">The original grid.</param>
	/// <param name="assignment">The assignment.</param>
	/// <param name="removedCandidates">Removed candidates.</param>
	/// <returns>The grid updated.</returns>
	private static Grid GetUpdatedGrid(/*copies*/ Grid original, ref readonly AssignmentInfo assignment, out ReadOnlySpan<Candidate> removedCandidates)
	{
		var r = new HashSet<Candidate>();
		_ = assignment is (var digit, { PeerIntersection: var peerCells } cells);
		foreach (var peerCell in peerCells)
		{
			ref var mask = ref original[peerCell];
			if (MaskToCellState(mask) == CellState.Empty && (mask >> digit & 1) != 0)
			{
				mask &= (Mask)~(1 << digit);
				r.Add(peerCell * 9 + digit);
			}
		}

		// Set digit for the current cell.
		if (cells is [var cell])
		{
			foreach (var d in (Mask)(original.GetCandidates(cell) & ~(1 << digit)))
			{
				r.Add(cell * 9 + d);
			}

			// Updates mask of the target cell.
			original[cell] = (Mask)(GetHeaderBits(in original, cell) | Grid.ModifiableMask | 1 << digit);
		}

		removedCandidates = r.ToArray();
		return original;
	}

	/// <summary>
	/// Find for locked candidates, and collect them into <paramref name="result"/>.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="node">The node.</param>
	/// <param name="result">The result.</param>
	private static void FindLockedCandidates(in Grid grid, DependencyNode node, HashSet<(AssignmentInfo, DependencyNodeType)> result)
	{
		var emptyCells = grid.EmptyCells;
		var candidatesMap = grid.CandidatesMap;
		foreach (var ((baseSet, coverSet), (a, b, c, _)) in Miniline.Map)
		{
			// Check whether the locked candidates pattern can be formed or not.
			if (!LockedCandidates.IsLockedCandidates(grid, a, b, c, emptyCells, out var digitsMaskFormingLockedCandidates))
			{
				continue;
			}

			// Now iterate on the mask to get all digits.
			foreach (var digit in digitsMaskFormingLockedCandidates)
			{
				ref readonly var map = ref candidatesMap[digit];

				// Check whether the digit contains any eliminations.
				var intersection = c & map;
				if (intersection.Count >= 2)
				{
					// a & map => Cells in lines are not empty => pointing eliminations.
					result.Add(
						(
							new(digit, intersection),
							a & map
								? DependencyNodeType.Block
								: baseSet < 18 ? DependencyNodeType.Row : DependencyNodeType.Column
						)
					);
				}
			}
		}
	}

	/// <summary>
	/// Find for naked singles, and collect them into <paramref name="result"/>.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="node">The node.</param>
	/// <param name="result">The result.</param>
	private static void FindNakedSingle(in Grid grid, DependencyNode node, HashSet<(AssignmentInfo, DependencyNodeType)> result)
	{
		foreach (var cell in grid.EmptyCells)
		{
			var mask = grid.GetCandidates(cell);
			if (BitOperations.IsPow2((uint)mask))
			{
				result.Add((new(cell * 9 + BitOperations.Log2((uint)mask)), DependencyNodeType.Cell));
			}
		}
	}

	/// <summary>
	/// Find for hidden singles, and collect them into <paramref name="result"/>.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="node">The node.</param>
	/// <param name="result">The result.</param>
	private static void FindHiddenSingle(in Grid grid, DependencyNode node, HashSet<(AssignmentInfo, DependencyNodeType)> result)
	{
		// Iterate on each digit.
		for (var digit = 0; digit < 9; digit++)
		{
			// Iterate on each house.
			for (var house = 0; house < 27; house++)
			{
				var count = 0;
				var firstFoundCell = -1;
				foreach (var cell in HousesMap[house])
				{
					var cellState = grid.GetState(cell);
					if (grid.GetDigit(cell) == digit)
					{
						goto NextHouse;
					}
					if (grid.Exists(cell, digit) is true)
					{
						firstFoundCell = cell;
						if (++count >= 2)
						{
							break;
						}
					}
				}

				if (count == 1)
				{
					result.Add(
						(
							new(firstFoundCell * 9 + digit),
							house switch
							{
								< 9 => DependencyNodeType.Block,
								< 18 => DependencyNodeType.Row,
								_ => DependencyNodeType.Column
							}
						)
					);
				}

			NextHouse:;
			}
		}
	}

	/// <summary>
	/// An unsafe entry to method <c>GetHeaderBits</c> defined in type <see cref="Grid"/>.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="cell">The cell.</param>
	/// <returns>The mask of mask bits, unshifted.</returns>
	[UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetHeaderBits")]
	private static extern Mask GetHeaderBits(ref readonly Grid grid, Cell cell);
}
