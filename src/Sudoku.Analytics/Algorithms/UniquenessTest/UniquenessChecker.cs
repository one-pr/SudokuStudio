namespace Sudoku.Algorithms.UniquenessTest;

/// <summary>
/// Represents a type that checks and infers for a pattern (specified as a <see cref="Grid"/>, but invalid - multiple solutions),
/// determining whether the pattern is a deadly pattern.
/// </summary>
/// <seealso cref="Grid"/>
public static class UniquenessChecker
{
	/// <summary>
	/// Try to analyze whether a grid is a deadly pattern, by only checking the specified cells.
	/// </summary>
	/// <param name="grid">Indicates the grid. It's required that only specified cells are assigned with candidates.</param>
	/// <param name="cells">Indicates the cells to be checked.</param>
	/// <returns>The result after the analysis operation is finished.</returns>
	/// <exception cref="DeadlyPatternInferrerLimitReachedException">
	/// Throws when the pattern contains more than 10000 solutions.
	/// </exception>
	public static PatternUniquenessInfo GetUniqueness(in Grid grid, in CellMap cells)
	{
		var patternCandidates = CandidateMap.Empty;
		if (grid is not { IsValid: false, EmptyCellsCount: 81, PuzzleType: SudokuType.Standard })
		{
			// Invalid values to be checked.
			goto FastFail;
		}

		// Verify whether at least one cell in pattern hold nothing.
		foreach (var cell in cells)
		{
			var mask = grid.GetCandidates(cell);
			if (mask == 0)
			{
				goto FastFail;
			}

			foreach (var digit in mask)
			{
				patternCandidates.Add(cell * 9 + digit);
			}
		}

		// Step 0: Determine whether at least one house the pattern spanned only hold one cell used.
		// A valid deadly pattern must hold at least 2 cells for all spanned houses.
		foreach (var house in cells.Houses)
		{
			if ((HousesMap[house] & cells).Count == 1)
			{
				goto FastFail;
			}
		}

		// Step 1: Get all solutions for that pattern.
		var solutions = getCombinations(grid, cells);
		if (solutions.Length == 0)
		{
			goto FastFail;
		}

		var failedCases = new List<Grid>();
		foreach (ref readonly var solution in solutions)
		{
			// Step 2: Iterate on all the other solutions,
			// and find whether each solution contains at least one possible corresponding solution
			// whose digits used in *all* houses are completely same.
			var tempSolutions = new List<Grid>();
			foreach (ref readonly var tempGrid in solutions[..])
			{
				if (tempGrid == solution)
				{
					continue;
				}

				// Check for all possible houses.
				var flag = true;
				foreach (var house in cells.Houses)
				{
					var mask1 = solution[HousesMap[house] & cells, true];
					var mask2 = tempGrid[HousesMap[house] & cells, true];
					if (mask1 != mask2)
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					tempSolutions.AddRef(tempGrid);
				}
			}

			// Step 3: Check for the validity on this case.
			// If failed to check, we should collect the case into the result, as an item in failed cases set.
			if (tempSolutions.Count == 0)
			{
				failedCases.AddRef(solution);
			}
		}

		// If all possible solutions has exchangable patterns, the pattern will be a real deadly pattern;
		// otherwise, not a deadly pattern.
		return new(grid, failedCases.Count == 0, failedCases.AsSpan(), patternCandidates);

	FastFail:
		return new(grid, false, [], patternCandidates);


		static ReadOnlySpan<Grid> getCombinations(in Grid grid, in CellMap cellsUsed)
		{
			var result = new List<Grid>();

			var truths = SpaceSet.Empty;
			foreach (var cell in cellsUsed)
			{
				truths.Add(Space.RowColumn(cell / 9, cell % 9));
			}

			var rankPattern = new RankPattern(in grid, in truths);
			var combinations = rankPattern.GetAssignmentCombinations();
			if (combinations.Length > 10000)
			{
				throw new DeadlyPatternInferrerLimitReachedException();
			}

			foreach (var combination in combinations)
			{
				var emptyGrid = Grid.Empty;
				foreach (var candidate in combination)
				{
					emptyGrid[candidate / 9] = (Mask)(Grid.ModifiableMask | 1 << candidate % 9);
				}
				result.AddRef(emptyGrid);
			}

			return result.AsSpan();
		}
	}

	/// <summary>
	/// Try to assign a candidate, and find a complete pattern where all cells are assigned,
	/// and then check whether the assigned pattern can form a deadly pattern.
	/// </summary>
	/// <param name="assigned">The assigned candidate.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="cells">The cells in the grid to be checked.</param>
	/// <param name="result">
	/// The last node assigned if return value is <see langword="true"/>; otherwise, <see langword="null"/>.
	/// If non-<see langword="null"/>, you can iterate with property <see cref="PatternTrialNode.Parent"/>
	/// to find all candidates assigned.
	/// </param>
	/// <returns>
	/// A <see cref="bool"/> result indicating whether the candidate <paramref name="assigned"/>
	/// will make the pattern to be a deadly pattern.
	/// </returns>
	public static bool TryAssign(Candidate assigned, in Grid grid, in CellMap cells, [NotNullWhen(true)] out PatternTrialNode? result)
	{
		if (grid.GetState(assigned / 9) != CellState.Empty)
		{
			// Invalid state.
			goto ReturnFalse;
		}

		var allSpecifiedCellsAreNonGiven = true;
		foreach (var cell in cells)
		{
			if (grid.GetState(cell) == CellState.Given)
			{
				allSpecifiedCellsAreNonGiven = false;
				break;
			}
		}
		if (!allSpecifiedCellsAreNonGiven)
		{
			// Invalid state.
			goto ReturnFalse;
		}

		var root = new PatternTrialNode(assigned, null);
		var queue = new Queue<PatternTrialNode>();
		queue.Enqueue(root);

		while (queue.TryDequeue(out var currentNode))
		{
			var appliedGrid = grid;
			currentNode.ApplyTo(ref appliedGrid);

			var unassignedCells = cells & ~appliedGrid.ModifiableCells;
			if (!unassignedCells)
			{
				// All cells are assigned.
				// Check whether the digits can form a deadly pattern or not.
				var tempGrid = Grid.Empty;
				foreach (var cell in cells)
				{
					tempGrid.SetDigit(cell, appliedGrid.GetDigit(cell));
				}
				if (!checkDeadlyPatternOnAssignedCells(tempGrid, cells))
				{
					continue;
				}

				// Valid.
				result = currentNode;
				return true;
			}

			// If not, we should continue to search for cells unassigned.
			foreach (var cell in unassignedCells)
			{
				if (appliedGrid.GetState(cell) != CellState.Empty)
				{
					continue;
				}

				// Check any hidden singles and naked singles.
				if (isNakedSingle(appliedGrid, cell, out var digit))
				{
					// Assign the value.
					var nextNode = new PatternTrialNode(cell * 9 + digit, currentNode);
					queue.Enqueue(nextNode);
				}
				else if (isHiddenSingle(appliedGrid, cell, out var targetCell, out digit))
				{
					var nextNode = new PatternTrialNode(targetCell * 9 + digit, currentNode);
					queue.Enqueue(nextNode);
				}
			}
		}

	ReturnFalse:
		result = null;
		return false;


		static bool isNakedSingle(in Grid grid, Cell cell, out Digit digit)
		{
			var mask = (uint)(grid.GetCandidates(cell) & Grid.MaxCandidatesMask);
			if (IsPow2(mask))
			{
				digit = Log2(mask);
				return true;
			}
			digit = -1;
			return false;
		}

		static bool isHiddenSingle(in Grid grid, Cell cell, out Cell targetCell, out Digit digit)
		{
			for (var eachDigit = 0; eachDigit < 9; eachDigit++)
			{
				foreach (var houseType in HouseTypes)
				{
					var counter = 0;
					targetCell = -1;
					foreach (var eachCell in HousesMap[cell.ToHouse(houseType)])
					{
						if (grid.Exists(eachCell, eachDigit) is true && (targetCell = eachCell) is var _ && ++counter >= 2)
						{
							break;
						}
					}
					if (counter == 1)
					{
						digit = eachDigit;
						return true;
					}
				}
			}

			digit = -1;
			targetCell = -1;
			return false;
		}

		static bool checkDeadlyPatternOnAssignedCells(in Grid grid, in CellMap cells)
		{
			// Find for all digits appeared in all houses.
			var houseDigitsDictionary = new Dictionary<House, Mask>(27);
			foreach (var house in cells.Houses)
			{
				houseDigitsDictionary.Add(house, grid[HousesMap[house] & cells]);
			}

			// Then enumerate assignments to find a new assignments, where all digits are same as the current one.
			var queue = new Queue<PatternTrialNode>();
			var firstCell = cells[0];

			var a = houseDigitsDictionary[firstCell.ToHouse(HouseType.Block)];
			var b = houseDigitsDictionary[firstCell.ToHouse(HouseType.Row)];
			var c = houseDigitsDictionary[firstCell.ToHouse(HouseType.Column)];
			foreach (var digit in (Mask)(a | (Mask)(b | c)))
			{
				queue.Enqueue(new(firstCell * 9 + digit, null));
			}

			// Try to assign the value.
			while (queue.TryDequeue(out var currentNode))
			{
				var appliedGrid = grid;
				currentNode.ApplyTo(ref appliedGrid);

				var unassignedCells = cells & ~appliedGrid.ModifiableCells;
				if (!unassignedCells)
				{
					if (grid == appliedGrid)
					{
						continue;
					}

					// Check for each house.
					var tempHouseDigitsDictionary = new Dictionary<House, Mask>(27);
					var houses = cells.Houses;
					foreach (var house in houses)
					{
						tempHouseDigitsDictionary.Add(house, appliedGrid[HousesMap[house] & cells]);
					}
					var flag = true;
					foreach (var house in houses)
					{
						if (houseDigitsDictionary[house] != tempHouseDigitsDictionary[house])
						{
							flag = false;
							break;
						}
					}
					if (!flag)
					{
						continue;
					}

					return true;
				}

				// Otherwise, we should continue to iterate other cells.
				foreach (var cell in unassignedCells)
				{
					var d = houseDigitsDictionary[cell.ToHouse(HouseType.Block)];
					var e = houseDigitsDictionary[cell.ToHouse(HouseType.Row)];
					var f = houseDigitsDictionary[cell.ToHouse(HouseType.Column)];
					var g = (Mask)0;
					foreach (var houseType in HouseTypes)
					{
						foreach (var tempCell in cells & HousesMap[cell.ToHouse(houseType)] & ~unassignedCells)
						{
							g |= (Mask)(1 << appliedGrid.GetDigit(tempCell));
						}
					}
					foreach (var digit in (Mask)((Mask)(d | (Mask)(e | f)) & ~g))
					{
						queue.Enqueue(new(cell * 9 + digit, currentNode));
					}
				}
			}

			return false;
		}
	}
}
