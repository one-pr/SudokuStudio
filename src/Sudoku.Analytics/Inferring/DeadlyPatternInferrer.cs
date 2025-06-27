namespace Sudoku.Inferring;

/// <summary>
/// Represents a type that checks and infers for a pattern (specified as a <see cref="Grid"/>, but invalid - multiple solutions),
/// determining whether the pattern is a deadly pattern.
/// </summary>
/// <seealso cref="Grid"/>
public sealed class DeadlyPatternInferrer : IInferrable<DeadlyPatternInferredResult>
{
	/// <inheritdoc cref="TryInfer(in Grid, ref readonly CellMap, out DeadlyPatternInferredResult)"/>
	public static bool TryInfer(in Grid grid, out DeadlyPatternInferredResult result)
		=> TryInfer(grid, in CellMap.nullref, out result);

	/// <summary>
	/// Try to analyze whether a grid is a deadly pattern, by only checking the specified cells.
	/// </summary>
	/// <param name="grid">Indicates the grid. It's required that only specified cells are assigned with candidates.</param>
	/// <param name="cells">Indicates the cells to be checked.</param>
	/// <param name="result">The result after the analysis operation is finished.</param>
	/// <exception cref="DeadlyPatternInferrerLimitReachedException">
	/// Throws when the pattern contains more than 10000 solutions.
	/// </exception>
	public static bool TryInfer(in Grid grid, [AllowNull] ref readonly CellMap cells, out DeadlyPatternInferredResult result)
	{
		var patternCandidates = CandidateMap.Empty;
		if (grid.IsValid || grid.EmptyCellsCount != 81 || grid.PuzzleType != SudokuType.Standard)
		{
			// Invalid values to be checked.
			goto FastFail;
		}

		// Collect all used cells. This value may not be necessary but will make program be a little bit faster if cached.
		CellMap cellsUsed;
		if (Unsafe.IsNullRef(in cells))
		{
			cellsUsed = CellMap.Empty;
			for (var cell = 0; cell < 81; cell++)
			{
				if (grid.GetCandidates(cell) != Grid.MaxCandidatesMask)
				{
					cellsUsed.Add(cell);
				}
			}
		}
		else
		{
			cellsUsed = cells;
		}

		// Verify whether at least one cell in pattern hold nothing.
		foreach (var cell in cellsUsed)
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
		foreach (var house in cellsUsed.Houses)
		{
			if ((HousesMap[house] & cellsUsed).Count == 1)
			{
				goto FastFail;
			}
		}

		// Step 1: Get all solutions for that pattern.
		var solutions = getCombinations(grid, cellsUsed);
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
				foreach (var house in cellsUsed.Houses)
				{
					var mask1 = solution[HousesMap[house] & cellsUsed, true];
					var mask2 = tempGrid[HousesMap[house] & cellsUsed, true];
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
		result = new(grid, failedCases.Count == 0, failedCases.AsSpan(), patternCandidates);
		return true;

	FastFail:
		result = new(grid, false, [], patternCandidates);
		return true;


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
			if (combinations.Length >= 10000)
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
}
