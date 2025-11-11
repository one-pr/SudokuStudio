namespace Sudoku.Analytics.Dependency;

/// <summary>
/// Represents a checker that analyzes dependency of a grid,
/// to know how to conclude assignment <c>b</c> from the specified assignment <c>a</c>.
/// </summary>
public static class DependencyChecker
{
	/// <summary>
	/// Try to analyze dependency spaces (truth and link) in the grid.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="previous">The previous assignment.</param>
	/// <param name="current">The current assignment.</param>
	/// <param name="truth">Indicates the truth inferred.</param>
	/// <param name="link">Indicates the link inferred.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public static bool TryGetDependencySpaces(
		in Grid grid,
		in DependencyAssignment previous,
		in DependencyAssignment current,
		out Space truth,
		out Space link
	)
	{
		// Just check for such cases, inferring them by checking satisfiability of such patterns. Here're the cases:
		// .---------------------------------------------.-------------------------------.-----------------------------.
		// |     \\Cases | Case 1:                       | Case 2:                       | Case 3:                     |
		// | Types\\     |   Cell link + House truth     |   House link + House truth    |   House link + Cell truth   |
		// :-------------+-------------------------------+-------------------------------+-----------------------------:
		// |             | r1c1 = 1                      | r1c1 = 1                      | r1c1 = 1                    |
		// | Non-grouped |   => r1c1 <> 2 (cell link)    |   => r4c1 <> 1 (house link)   |   => r1c2 <> 1 (house link) |
		// |             |   => r1c2 = 2 (house truth)   |   => r4c4 = 1 (house truth)   |   => r1c2 = 2 (cell truth)  |
		// :-------------+-------------------------------+-------------------------------+-----------------------------:
		// |             | r1c1 = 1                      | r1c123 = 1                    | r1c123 = 1                  |
		// | Grouped     |   => r1c1 <> 2 (cell link)    |   => r123c2 <> 1 (house link) |   => r1c7 <> 1 (house link) |
		// |             |   => r1c789 = 2 (house truth) |   => r789c2 = 1 (house truth) |   => r1c7 = 2 (cell truth)  |
		// '-------------'-------------------------------'-------------------------------'-----------------------------'

		var emptyCells = grid.EmptyCells;
		var (previousDigit, previousCells) = previous;
		var (currentDigit, currentCells) = current;

		// Check shared houses for current assignment, trying to check which house covers previous assignment.
		// If does, a hidden single can be inferred.
		foreach (var house in currentCells.SharedHouses)
		{
			// All previously-assigned cells should be inside the shared house.
			if ((HousesMap[house] & previousCells) == previousCells) { goto Case1; } else { goto Case2; }

		Case1:
			// May be case 1.
			if (previousDigit == currentDigit)
			{
				// Same digit, same house covered. No valid types available.
				continue;
			}

			// Check previous assignment. The previous assignment cannot be grouped one in case 1.
			if (previousCells is not [var previousCell])
			{
				continue;
			}

			// Check whether the grid contains 'current.Digit' in cell 'previousCell'.
			if ((grid.GetCandidates(previousCell) >> currentDigit & 1) == 0)
			{
				continue;
			}

			// Case 1 check passed.
			truth = house switch
			{
				< 9 => Space.BlockDigit(house, currentDigit),
				< 18 => Space.RowDigit(house - 9, currentDigit),
				_ => Space.ColumnDigit(house - 18, currentDigit)
			};
			link = Space.RowColumn(previousCell / 9, previousCell % 9);
			return true;

		Case2:
			// Otherwise, may be case 2.
			if (previousDigit != currentDigit)
			{
				// Different digits, with different house covered. No valid types available.
				continue;
			}

			// Collect cells that contains the current digit.
			var cellsIncludingCurrentDigit = CellMap.Empty;
			foreach (var cell in HousesMap[house] & emptyCells)
			{
				if ((grid.GetCandidates(cell) >> currentDigit & 1) != 0)
				{
					cellsIncludingCurrentDigit += cell;
				}
			}

			// Determine whether all the last cells (excluding 'currentCells') can be seen by previous cells or not.
			var lastCells = cellsIncludingCurrentDigit & ~currentCells;
			if (!lastCells)
			{
				// There's no cell to be included in link.
				continue;
			}
			if ((previousCells.PeerIntersection & lastCells) != lastCells)
			{
				continue;
			}

			// Okay. Now use shared house to describe links.
			var linkHouse = previousCells.FirstSharedHouse;
			Debug.Assert(linkHouse != FallbackConstants.@int);

			truth = house switch
			{
				< 9 => Space.BlockDigit(house, currentDigit),
				< 18 => Space.RowDigit(house - 9, currentDigit),
				_ => Space.ColumnDigit(house - 18, currentDigit)
			};
			link = linkHouse switch
			{
				< 9 => Space.BlockDigit(linkHouse, currentDigit),
				< 18 => Space.RowDigit(linkHouse - 9, currentDigit),
				_ => Space.ColumnDigit(linkHouse - 18, currentDigit)
			};
			return true;
		}

		// If here, case 3 should be checked now.
		// In case 3, current assignment can only hold one cell in order to construct cell truth.
		if (currentCells is not [var currentCell])
		{
			goto ReturnFalse;
		}

		// Check whether the target cell contains 'previousDigit'.
		if ((grid.GetCandidates(currentCell) >> previousDigit & 1) == 0)
		{
			goto ReturnFalse;
		}

		// Check whether the previous cell has a house that includes target cell 'currentCell'.
		// Iterate on each shared house.
		foreach (var house in previousCells.SharedHouses)
		{
			// Check whether including or not.
			if (!HousesMap[house].Contains(currentCell))
			{
				continue;
			}

			truth = Space.RowColumn(currentCell / 9, currentCell % 9);
			link = house switch
			{
				< 9 => Space.BlockDigit(house, previousDigit),
				< 18 => Space.RowDigit(house - 9, previousDigit),
				_ => Space.ColumnDigit(house - 18, previousDigit)
			};
			return true;
		}

	ReturnFalse:
		// Otherwise, we cannot infer a valid truth and link that can transfer assignments from 'previous' to 'current'.
		truth = link = default;
		return false;
	}

	/// <summary>
	/// Creates two lists of spaces indicating truths and links, representing corresponding logic to a complex pattern.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="lastNode">The last node.</param>
	/// <param name="cause">The space which doesn't have any possible positions to be filled.</param>
	/// <param name="truths">The truths.</param>
	/// <param name="links">The links.</param>
	public static void InferSpaces(in Grid grid, DependencyNode lastNode, Space cause, out SpaceSet truths, out SpaceSet links)
	{
		(truths, links) = (SpaceSet.Empty, SpaceSet.Empty);
		var assignments = lastNode.Assignments.Span;

		// Trace removed candidates, and build truths.
		if (!buildTruths(in grid, ref truths, assignments, out var removed))
		{
			throw new InvalidOperationException("It seems that you have reached this impossible branch.");
		}

		// Add cause directly.
		truths += cause;

		// Add all candidates in target space as truth-removed.
		removed |= cause.GetAvailableRange(grid);

		// Construct a dictionary that stores candidates and its containing truth.
		var removedCandidatesLookup = new Dictionary<Candidate, HashSet<Space>>(removed.Count);
		foreach (var candidate in removed)
		{
			foreach (var space in candidate.Spaces)
			{
				if (truths.Contains(space) && !removedCandidatesLookup.TryAdd(candidate, [space]))
				{
					removedCandidatesLookup[candidate].Add(space);
				}
			}
		}

		// Build links.
		buildLinks(in grid, ref links, assignments, in removed);


		bool buildTruths(
			ref readonly Grid grid,
			ref SpaceSet truths,
			ReadOnlySpan<DependencyAssignment> assignments,
			out CandidateMap removed
		)
		{
			removed = CandidateMap.Empty;

			// Now we have spaces of eliminations as potential links.
			// Here, we can iterate on each assignment in this branch, to get reasons that how an assignment is concluded.
			// Then we can rely on its reason of it, to find for original candidates we have already removed before.
			foreach (var ancestor in lastNode.EnumerateAncestors(true))
			{
				// Gets the reason why the node can be concluded.
				var type = ancestor.Type;

				// Gets the cells of the node, and corresponding digit used.
				var truth = ancestor.Truth;
				if (truth != Space.InvalidSpace)
				{
					truths += truth;
					removed |= truth.GetAvailableRange(grid);
				}
			}

			// Remove candidates that is from assignments.
			foreach (ref readonly var assignment in assignments)
			{
				foreach (var cell in assignment.Cells)
				{
					removed -= cell * 9 + assignment.Digit;
				}
			}
			return true;
		}

		void buildLinks(
			ref readonly Grid grid,
			ref SpaceSet links,
			ReadOnlySpan<DependencyAssignment> assignments,
			ref readonly CandidateMap removed
		)
		{
			var tempRemovedCandidates = CandidateMap.Empty;
			var emptyCells = grid.EmptyCells;

			// Iterate on each assignment, and update grid by removing some candidates,
			// and then check which candidates are removed. Then traverse them to find all links that can cover them.
			foreach (ref readonly var assignment in assignments)
			{
				// Fetch candidates assigned in the current round.
				var cells = assignment.Cells;
				var digit = assignment.Digit;

				// Check for such removed candidates, and find for links.
				// Here we use a 4-directional check, to find for relations between assignment and elimination.
				foreach (var house in cells.SharedHouses)
				{
					// Check eliminated candidates.
					foreach (var cell in HousesMap[house] & emptyCells & ~cells)
					{
						var candidate = cell * 9 + digit;
						if (grid.Exists(cell, digit) is not true || tempRemovedCandidates.Contains(candidate))
						{
							continue;
						}

						if (!removed.Contains(candidate))
						{
							continue;
						}

						// If a candidate is inside both a block and a link, we only count it in line.
						var sharedHouses = (cells + cell).SharedHouses;
						if ((sharedHouses & AllBlocksMask) != 0 && (sharedHouses & (AllRowsMask | AllColumnsMask)) != 0
							&& house.HouseType == HouseType.Block)
						{
							// Skip checking for block links.
							continue;
						}

						tempRemovedCandidates += candidate;
						links += house switch
						{
							< 9 => Space.BlockDigit(house, digit),
							< 18 => Space.RowDigit(house - 9, digit),
							_ => Space.ColumnDigit(house - 18, digit)
						};
					}
				}
				if (cells is [var onlyCell])
				{
					// Check eliminated candidates.
					foreach (var d in (Mask)(grid.GetCandidates(onlyCell) & ~(1 << digit)))
					{
						var candidate = onlyCell * 9 + d;
						if (tempRemovedCandidates.Add(candidate) && removed.Contains(candidate))
						{
							links += Space.RowColumn(onlyCell / 9, onlyCell % 9);
						}
					}
				}
			}
		}
	}
}
