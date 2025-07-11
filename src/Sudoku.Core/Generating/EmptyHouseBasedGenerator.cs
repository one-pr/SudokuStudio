namespace Sudoku.Generating;

/// <summary>
/// Represents a puzzle generator that can avoid selections on empty houses.
/// </summary>
[TypeImpl(TypeImplFlags.AllObjectMethods)]
public ref partial struct EmptyHouseBasedGenerator() : IGenerator<Grid>
{
	/// <summary>
	/// The order in which cells are set when generating a full grid.
	/// </summary>
	private readonly int[] _generateIndices = new int[81];

	/// <summary>
	/// A random generator for creating new puzzles.
	/// </summary>
	private readonly Random _rng = new();

	/// <summary>
	/// Indicates the internal fast solver.
	/// </summary>
	private readonly BitwiseSolver _solver = new();

	/// <summary>
	/// The recursion stack.
	/// </summary>
	private readonly Span<GeneratorRecursionStackEntry> _stack = new GeneratorRecursionStackEntry[82];

	/// <summary>
	/// The final grid to be used.
	/// </summary>
	private Grid _newFullSudoku, _newValidSudoku;


	/// <summary>
	/// Indicates the missing house type.
	/// </summary>
	public HouseType MissingHouseType { get; init; }

	/// <summary>
	/// Indicates the number of missing houses.
	/// </summary>
	public Digit MissingHousesCount { get; init; }


	/// <summary>
	/// Try to generate a puzzle randomly, or return <see cref="Grid.Undefined"/> if a user cancelled the operation.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token that can cancel the operation.</param>
	/// <returns>The result grid, or <see cref="Grid.Undefined"/> if a user cancelled the operation.</returns>
	public Grid Generate(CancellationToken cancellationToken = default)
	{
		_stack.Fill(new());

		try
		{
			while (!GenerateForFullGrid()) ;

			GenerateInitPos(cancellationToken);
			return _newValidSudoku.FixedGrid;
		}
		catch (OperationCanceledException)
		{
			return Grid.Undefined;
		}
	}

	/// <summary>
	/// Takes a full sudoku from <see cref="_newFullSudoku"/> and generates a valid puzzle by deleting cells.
	/// If a deletion produces a grid with more than one solution it is of course undone.
	/// </summary>
	/// <inheritdoc cref="Generate(CancellationToken)"/>
	private void GenerateInitPos(CancellationToken cancellationToken = default)
	{
		var houseIndices = SpanEnumerable.Range(9).ToArray().AsSpan();

		// Do until we have only 17 clues left or until all cells have been tried.
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();

			// Step 1: Clear some houses.
			// We start with the full board.
			// Randomize the number of values indicating the target houses are empty.
			_newValidSudoku = _newFullSudoku;
			var used = CellMap.Empty;
			var remainingClues = 81;

			_rng.Shuffle(houseIndices);
			var hasUniqueSolutionAfterClearingValuesFromSpecifiedHouses = true;
			foreach (var house in houseIndices[..MissingHousesCount])
			{
				// Remove all possible cells from such target house.
				foreach (var cell in HousesMap[(int)MissingHouseType * 9 + house])
				{
					_newValidSudoku.SetDigit(cell, -1);
					used.Add(cell);
					remainingClues--;
				}

				// Determine whether the puzzle has unique solution.
				if (!_solver.CheckValidity(_newValidSudoku.ToString("!0")))
				{
					hasUniqueSolutionAfterClearingValuesFromSpecifiedHouses = false;
					break;
				}
			}
			if (!hasUniqueSolutionAfterClearingValuesFromSpecifiedHouses)
			{
				continue;
			}

			// Step 2: Generate puzzles.
			var targetRemainingClues = _rng.Next(17, 30);
			while (remainingClues > targetRemainingClues && used != ~CellMap.Empty)
			{
				cancellationToken.ThrowIfCancellationRequested();

				// Get the next position to try.
				var cell = _rng.NextCell();
				do
				{
					if (cell < 80)
					{
						cell++;
					}
					else
					{
						cell = 0;
					}
				}
				while (used.Contains(cell));
				used.Add(cell);

				if (_newValidSudoku.GetDigit(cell) == -1)
				{
					// Already deleted.
					continue;
				}

				// Delete cell.
				_newValidSudoku.SetDigit(cell, -1);
				remainingClues--;

				if (!_solver.CheckValidity(_newValidSudoku.ToString("!0")))
				{
					// If not unique, revert deletion.
					_newValidSudoku.SetDigit(cell, _newFullSudoku.GetDigit(cell));
					remainingClues++;
				}
			}

			// Check validity.
			if (_newValidSudoku.ModifiableCellsCount <= targetRemainingClues
				&& _solver.CheckValidity(_newValidSudoku.ToString("!0")))
			{
				return;
			}
		}
	}

	/// <summary>
	/// Generate a solution grid.
	/// </summary>
	/// <returns>A <see cref="bool"/> value indicating whether the generation operation is succeeded.</returns>
	private bool GenerateForFullGrid()
	{
		// Limit the number of tries.
		var actTries = 0;

		// Generate a random order for setting the cells.
		for (var i = 0; i < 81; i++)
		{
			_generateIndices[i] = i;
		}

		for (var i = 0; i < 81; i++)
		{
			var (index1, index2) = (_rng.NextCell(), _rng.NextCell());
			while (index1 == index2)
			{
				index2 = _rng.NextCell();
			}

			@ref.Swap(ref _generateIndices[index1], ref _generateIndices[index2]);
		}

		// First set a new empty Sudoku.
		(_stack[0].SudokuGrid, _stack[0].Cell, var level) = (Grid.Empty, -1, 0);
		while (true)
		{
			// Get the next unsolved cell according to _generateIndices.
			if (_stack[level].SudokuGrid.EmptyCellsCount == 0)
			{
				// Generation is complete.
				_newFullSudoku = _stack[level].SudokuGrid;
				return true;
			}

			var index = -1;
			var actValues = _stack[level].SudokuGrid.ToDigitsArray();
			for (var i = 0; i < 81; i++)
			{
				var actTry = _generateIndices[i];
				if (actValues[actTry] == 0)
				{
					index = actTry;
					break;
				}
			}

			level++;
			_stack[level].Cell = (short)index;
			_stack[level].Candidates = _stack[level - 1].SudokuGrid.GetCandidates(index);
			_stack[level].CandidateIndex = 0;

			// Not too many tries...
			actTries++;
			if (actTries > 100)
			{
				return false;
			}

			// Go to the next level.
			var done = false;
			do
			{
				// This loop runs as long as the next candidate tried produces an invalid sudoku or until all candidates have been tried.
				// Fall back all levels, where nothing is to do anymore.
				while (_stack[level].CandidateIndex >= BitOperations.PopCount(_stack[level].Candidates))
				{
					level--;
					if (level <= 0)
					{
						// No level with candidates left.
						done = true;
						break;
					}
				}
				if (done)
				{
					break;
				}

				// Try the next candidate.
				var nextCandidate = _stack[level].Candidates.SetAt(_stack[level].CandidateIndex++);

				// Start with a fresh sudoku.
				ref var targetGrid = ref _stack[level].SudokuGrid;
				targetGrid = _stack[level - 1].SudokuGrid;
				targetGrid.SetDigit(_stack[level].Cell, nextCandidate);
				if (!checkValidityOnDuplicate(targetGrid, _stack[level].Cell))
				{
					// Invalid -> try next candidate.
					continue;
				}

				if (fillFastForSingles(ref targetGrid))
				{
					// Valid move, break from the inner loop to advance to the next level.
					break;
				}
			} while (true);
			if (done)
			{
				break;
			}
		}
		return false;


		static bool checkValidityOnDuplicate(in Grid grid, Cell cell)
		{
			foreach (var peer in PeersMap[cell])
			{
				var digit = grid.GetDigit(peer);
				if (digit == grid.GetDigit(cell) && digit != -1)
				{
					return false;
				}
			}
			return true;
		}

		static bool fillFastForSingles(ref Grid grid)
		{
			var emptyCells = grid.EmptyCells;

			// For hidden singles.
			for (var house = 0; house < 27; house++)
			{
				for (var digit = 0; digit < 9; digit++)
				{
					var houseMask = 0;
					for (var i = 0; i < 9; i++)
					{
						var cell = HousesCells[house][i];
						if (emptyCells.Contains(cell) && (grid.GetCandidates(cell) >> digit & 1) != 0)
						{
							houseMask |= 1 << i;
						}
					}

					if (BitOperations.IsPow2(houseMask))
					{
						// Hidden single.
						var cell = HousesCells[house][BitOperations.TrailingZeroCount(houseMask)];
						grid.SetDigit(cell, digit);
						if (!checkValidityOnDuplicate(grid, cell))
						{
							// Invalid.
							return false;
						}
					}
				}
			}

			// For naked singles.
			foreach (var cell in emptyCells)
			{
				var mask = grid.GetCandidates(cell);
				if (BitOperations.IsPow2(mask))
				{
					grid.SetDigit(cell, BitOperations.TrailingZeroCount(mask));
					if (!checkValidityOnDuplicate(grid, cell))
					{
						// Invalid.
						return false;
					}
				}
			}

			// Both hidden singles and naked singles are valid. Return true.
			return true;
		}
	}

	/// <inheritdoc/>
	Grid IGenerator<Grid>.Generate(IProgress<GeneratorProgress>? progress, CancellationToken cancellationToken)
		=> Generate(cancellationToken: cancellationToken);
}
