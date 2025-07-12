namespace Sudoku.Generating;

/// <summary>
/// Represents a puzzle generator that can avoid selections on empty houses.
/// </summary>
[TypeImpl(TypeImplFlags.AllObjectMethods)]
public ref partial struct EmptyHouseBasedGenerator() : IGenerator<Grid>
{
	/// <summary>
	/// <para>
	/// Indicates invalid combinations of houses to be cleared.
	/// Such combinations will make puzzle have multiple solutions.
	/// </para>
	/// <para>
	/// You can compare houses cleared with such values,
	/// to know whether a combination of cleared houses are invalid.
	/// For example, a valid puzzle (having a unique solution) cannot remove 27 givens
	/// from block 1, 2, 3, block 4, 5, 6, and so on).
	/// </para>
	/// </summary>
	private static readonly HouseMask[][] InvalidHouseCombinations = [
		[0b000_000_000__000_000_000__000_000_111, 0b000_000_001__000_000_001__000_000_001],
		[0b000_000_000__000_000_000__000_000_111, 0b000_000_010__000_000_010__000_000_010],
		[0b000_000_000__000_000_000__000_000_111, 0b000_000_100__000_000_100__000_000_100],
		[0b000_000_000__000_000_000__000_111_000, 0b000_001_000__000_001_000__000_001_000],
		[0b000_000_000__000_000_000__000_111_000, 0b000_010_000__000_010_000__000_010_000],
		[0b000_000_000__000_000_000__000_111_000, 0b000_100_000__000_100_000__000_100_000],
		[0b000_000_000__000_000_000__111_000_000, 0b001_000_000__001_000_000__001_000_000],
		[0b000_000_000__000_000_000__111_000_000, 0b010_000_000__010_000_000__010_000_000],
		[0b000_000_000__000_000_000__111_000_000, 0b100_000_000__100_000_000__100_000_000],
		[0b000_000_000__000_000_011__000_000_000, 0b000_000_000__000_000_101__000_000_000],
		[0b000_000_000__000_000_110__000_000_000, 0b000_000_000__000_000_011__000_000_000],
		[0b000_000_000__000_000_101__000_000_000, 0b000_000_000__000_000_110__000_000_000],
		[0b000_000_000__000_011_000__000_000_000, 0b000_000_000__000_101_000__000_000_000],
		[0b000_000_000__000_110_000__000_000_000, 0b000_000_000__000_011_000__000_000_000],
		[0b000_000_000__000_101_000__000_000_000, 0b000_000_000__000_110_000__000_000_000],
		[0b000_000_000__011_000_000__000_000_000, 0b000_000_000__101_000_000__000_000_000],
		[0b000_000_000__110_000_000__000_000_000, 0b000_000_000__011_000_000__000_000_000],
		[0b101_000_000__000_000_000__000_000_000, 0b110_000_000__000_000_000__000_000_000],
		[0b000_000_011__000_000_000__000_000_000, 0b000_000_101__000_000_000__000_000_000],
		[0b000_000_110__000_000_000__000_000_000, 0b000_000_011__000_000_000__000_000_000],
		[0b000_000_101__000_000_000__000_000_000, 0b000_000_110__000_000_000__000_000_000],
		[0b000_011_000__000_000_000__000_000_000, 0b000_101_000__000_000_000__000_000_000],
		[0b000_110_000__000_000_000__000_000_000, 0b000_011_000__000_000_000__000_000_000],
		[0b000_101_000__000_000_000__000_000_000, 0b000_110_000__000_000_000__000_000_000],
		[0b011_000_000__000_000_000__000_000_000, 0b101_000_000__000_000_000__000_000_000],
		[0b110_000_000__000_000_000__000_000_000, 0b011_000_000__000_000_000__000_000_000],
		[0b101_000_000__000_000_000__000_000_000, 0b110_000_000__000_000_000__000_000_000]
	];


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
	/// <para>Indicates the desired missing houses mask.</para>
	/// <para>
	/// This property can be made a <see cref="Dictionary{TKey, TValue}"/>
	/// of type arguments <see cref="HouseType"/> (the desired house types)
	/// and <see cref="int"/> (indicating the number of empty houses) in logic.
	/// You can set the desired number of 1 bits to construct the mask,
	/// telling the current generator the number of empty houses you want to keep for generated puzzles.
	/// Use 0 as placeholders. Set all 1's to target bit ranges that the mask represents.
	/// </para>
	/// <para>
	/// For example, if you want to keep 1 empty row, 1 empty column and 1 empty block,
	/// you can set the mask with value <c>0b000000001000000001000000001</c> (i.e. 262657).
	/// </para>
	/// </summary>
	public HouseMask DesiredMissingHousesMask { get; init; }


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
			while (true)
			{
				while (!GenerateForFullGrid()) ;

				if (GenerateInitPos(cancellationToken))
				{
					break;
				}
			}
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
	private unsafe bool GenerateInitPos(CancellationToken cancellationToken = default)
	{
		var bitsMap = new Dictionary<HouseType, int>(3)
		{
			{ HouseType.Block, BitOperations.PopCount(DesiredMissingHousesMask & Grid.MaxCandidatesMask) },
			{ HouseType.Row, BitOperations.PopCount(DesiredMissingHousesMask >> 9 & Grid.MaxCandidatesMask) },
			{ HouseType.Column, BitOperations.PopCount(DesiredMissingHousesMask >> 18) }
		};

		var blockIndices = SpanEnumerable.Range(0, 9).ToArray().AsSpan();
		var rowIndices = SpanEnumerable.Range(9, 9).ToArray().AsSpan();
		var columnIndices = SpanEnumerable.Range(18, 9).ToArray().AsSpan();
		var localPairs = stackalloc LocalPair[]
		{
			new() { HouseType = HouseType.Block, HouseIndices = &blockIndices },
			new() { HouseType = HouseType.Row, HouseIndices = &rowIndices },
			new() { HouseType = HouseType.Column, HouseIndices = &columnIndices }
		};

		var patternTrialTimes = -1;
		while (true)
		{
			if (++patternTrialTimes >= 126)
			{
				return false;
			}

			cancellationToken.ThrowIfCancellationRequested();

			// Step 1: Clear some houses.
			// We start with the full board.
			// Randomize the number of values indicating the target houses are empty.
			_newValidSudoku = _newFullSudoku;
			var used = CellMap.Empty;
			var remainingClues = 81;

			// Fetch the number of empty houses set.
			var hasUniqueSolutionAfterClearingValuesFromSpecifiedHouses = true;
			for (var i = 0; i < 3; i++)
			{
				var l = localPairs + i;
				if (bitsMap[l->HouseType] == 0)
				{
					// Skip for the unset house types.
					continue;
				}

				_rng.Shuffle(*l->HouseIndices);
				var (previousHousesMask, j) = (0, 0);
				foreach (var house in *l->HouseIndices)
				{
					// Check whether the house selected will directly cause multiple solutions.
					// If so, we should skip for the house.
					var willHouseCauseMultipleSolutions = false;
					foreach (var combination in InvalidHouseCombinations[house])
					{
						if (combination == (previousHousesMask | 1 << house))
						{
							willHouseCauseMultipleSolutions = true;
							break;
						}
					}
					if (willHouseCauseMultipleSolutions)
					{
						continue;
					}

					// Remove all possible cells from such target house.
					foreach (var cell in HousesMap[house])
					{
						_newValidSudoku.SetDigit(cell, -1);
						if (used.Add(cell))
						{
							remainingClues--;
						}
					}

					// Determine whether the puzzle has unique solution.
					if (!_solver.CheckValidity(_newValidSudoku.ToString("!0")))
					{
						hasUniqueSolutionAfterClearingValuesFromSpecifiedHouses = false;
						goto CheckFlag;
					}

					previousHousesMask |= 1 << house;
					if (++j >= bitsMap[l->HouseType])
					{
						// Enough houses are cleared.
						break;
					}
				}
			}
		CheckFlag:
			if (!hasUniqueSolutionAfterClearingValuesFromSpecifiedHouses)
			{
				continue;
			}

			// Step 2: Generate puzzles.
			var step2TrialTimes = -1;
			while (true)
			{
				if (++step2TrialTimes >= 1000)
				{
					break;
				}

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
					return true;
				}
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

/// <summary>
/// To simplify for loop, we use a temporary type to construct values into a sequence of unsafe values.
/// </summary>
file ref struct LocalPair
{
	/// <summary>
	/// Indicates the target house type.
	/// </summary>
	public HouseType HouseType;

	/// <summary>
	/// Indicates the target house indices.
	/// </summary>
	public unsafe Span<int>* HouseIndices;
}
