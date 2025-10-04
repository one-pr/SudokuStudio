namespace Sudoku.Analytics.StepSearchers;

/// <summary>
/// Provides with a <b>Broken Loop</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Broken Loop Type 1</item>
/// <item>Broken Loop Type 2</item>
/// <item>Broken Loop Type 3</item>
/// </list>
/// </summary>
[StepSearcher(
	"StepSearcherName_BrokenLoopStepSearcher",
	Technique.BrokenLoopType1, Technique.BrokenLoopType2, Technique.BrokenLoopType3)]
public sealed partial class BrokenLoopStepSearcher
{
	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		// Modes: 0 - Cell, 1 - Block, 2 - Row, 3 - Column.
		const byte mode_CellNext = 0, mode_ColumnNext = 3;
		var foundLoopsGroupedByGuardians = new Dictionary<List<Candidate>, List<Candidate>>(
			EqualityComparer<List<Candidate>>.Create(
				static (left, right) => left!.Count == right!.Count && left.Count switch
				{
					0 => true,
					1 => left[0] == right[0],
					_ => left[0] == right[0] && left[1] == right[1] || left[0] == right[1] && left[1] == right[0]
				},
				static obj => obj.Count switch { 0 => 0, 1 => obj[0], _ => obj[0] ^ obj[1] }
			)
		);

		ref readonly var grid = ref context.Grid;

		// Iterate on all non-bivalue cells.
		foreach (var cell in EmptyCells & ~BivalueCells)
		{
			var digitsMask = grid.GetCandidates(cell);
			if (PopCount((uint)digitsMask) != 3)
			{
				// The cell must contain 3 candidates, otherwise:
				//   1. The cell has only one candidate -> naked single.
				//   2. The cell has 4 different candidates
				//      -> we cannot find a valid loop using 2 extra guardians,
				//      or it can be replaced with a coloring technique.
				continue;
			}

			// Iterate on each extra start.
			foreach (var digit in digitsMask)
			{
				var d1 = BitOperations.PopTwo((uint)(digitsMask & ~(1 << digit)), out var d2);
				var tempCandidatesMap = CandidateMap.Empty + (cell * 9 + d1) + (cell * 9 + d2);
				dfs(
					ref context,
					grid,
					cell,
					d2,
					cell,
					d2,
					byte.MaxValue,
					[cell * 9 + d1, cell * 9 + d2],
					[cell * 9 + digit],
					ref tempCandidatesMap
				);
				if (!context.CancellationToken)
				{
					// Canceled.
					return null;
				}
			}
			if (foundLoopsGroupedByGuardians.Count == 0)
			{
				// The cell cannot find any valid loops.
				continue;
			}
		}

		// Check for subtypes.
		foreach (var (guardians, loop) in foundLoopsGroupedByGuardians)
		{
			switch (guardians)
			{
				// Invalid grid (no valid solutions found).
				case []:
				{
					throw new PuzzleInvalidException(grid, typeof(BrokenLoopStepSearcher));
				}

				// Only checks for type 1.
				case [var guardian]:
				{
					if (CheckType1(ref context, grid, guardian, loop) is { } type1Step)
					{
						return type1Step;
					}
					break;
				}

				// Check for other types (2, 3, 4).
				case { Count: 2 }:
				{
					if (CheckType2(ref context, grid, guardians, loop) is { } type2Step)
					{
						return type2Step;
					}
					if (CheckType3(ref context, grid, guardians, loop) is { } type3Step)
					{
						return type3Step;
					}

					// TODO: Check type 4.
					break;
				}
			}
		}
		return null;


		void dfs(
			ref StepAnalysisContext context,
			in Grid grid,
			Cell currentCell,
			Digit currentDigit,
			Cell previousCell,
			Digit previousDigit,
			byte unitTypeMode,
			List<Candidate> loopCandidates,
			List<Candidate> guardians,
			ref CandidateMap loop
		)
		{
			if (!context.CancellationToken)
			{
				// User canceled.
				return;
			}

			var availableNext = new List<(byte NextMode, Candidate NextCandidate, Candidate? NewGuardian)>(2);
			for (var mode = mode_CellNext; mode <= mode_ColumnNext; mode++)
			{
				if (mode == unitTypeMode)
				{
					continue;
				}

				if (mode == mode_CellNext)
				{
					if (previousCell == currentCell)
					{
						continue;
					}

					// Check digits in the current cell.
					var digitsMask = grid.GetCandidates(currentCell);
					var otherDigitsMask = (Mask)(digitsMask & ~(1 << currentDigit));
					switch (PopCount((uint)otherDigitsMask))
					{
						default:
						{
							// Invalid.
							continue;
						}
						case 1:
						{
							var otherDigit = Log2((uint)otherDigitsMask);
							var nextCandidate = currentCell * 9 + otherDigit;
							availableNext.AddRef((mode, nextCandidate, null));
							break;
						}
						case 2:
						{
							// We may choose an extra guardian here.
							foreach (var digit in otherDigitsMask)
							{
								var nextCandidate = currentCell * 9 + digit;
								var nextGuardian = currentCell * 9 + Log2((uint)(otherDigitsMask & ~(1 << digit)));
								availableNext.AddRef((mode, nextCandidate, nextGuardian));
							}
							break;
						}
					}
				}
				else
				{
					// Check the previous house.
					var sharedHouse = (previousCell.AsCellMap() + currentCell).FirstSharedHouse;
					var houseType = (int)sharedHouse.HouseType;
					if (houseType == mode - 1)
					{
						// Same house.
						continue;
					}

					// Get for the next house.
					var nextHouse = currentCell >> (HouseType)(mode - 1);
					var nextPossibleCells = (CandidatesMap[currentDigit] & HousesMap[nextHouse]) - currentCell;
					switch (nextPossibleCells.Count)
					{
						default:
						{
							// Invalid.
							continue;
						}
						case 1:
						{
							var nextCell = nextPossibleCells[0];
							var nextCandidate = nextCell * 9 + currentDigit;
							availableNext.AddRef((mode, nextCandidate, null));
							break;
						}
						case 2:
						{
							foreach (var nextCell in nextPossibleCells)
							{
								var nextCandidate = nextCell * 9 + currentDigit;
								var nextGuardian = (nextPossibleCells - nextCell)[0] * 9 + currentDigit;
								availableNext.AddRef((mode, nextCandidate, nextGuardian));
							}
							break;
						}
					}
				}
			}
			if (availableNext.Count == 0)
			{
				return;
			}

			// Check for available cases for next.
			foreach (var (nextMode, nextCandidate, nextGuardianIfWorth) in availableNext)
			{
				// Check guardian: we only allow at most 2 guardians in the pattern.
				switch (guardians, nextGuardianIfWorth)
				{
					case ({ Count: 2 }, not null):
					{
						// Invalid.
						continue;
					}
					case ([var firstGuardian], { } nextGuardian)
					when !(
						firstGuardian / 9 == nextGuardian / 9
							|| firstGuardian % 9 == nextGuardian % 9
							&& PeersMap[firstGuardian / 9].Contains(nextGuardian / 9)
					):
					{
						// The new guardian must be either:
						//   1. in same house with the previous guardian, with same digit
						//   2. in same cell.
						continue;
					}
				}

				// Now check whether an odd-length loop is formed if we add next candidate into the loop.
				if (nextCandidate == loopCandidates[0] && (loopCandidates.Count & 1) == 1)
				{
					// Construct guardians map.
					var tempGuardians = new List<Candidate>(guardians);
					if (nextGuardianIfWorth is { } nextGuardian)
					{
						tempGuardians.Add(nextGuardian);
					}

					// Check minimum length of all traversed loops.
					// If we find a new loop with greater length, we should discard it.
					if (foundLoopsGroupedByGuardians.TryGetValue(tempGuardians, out var minimumLength)
						&& loopCandidates.Count >= minimumLength.Count)
					{
						// Discard.
						continue;
					}

					// Then add or update dictionary of loop with minimum length.
					foundLoopsGroupedByGuardians[tempGuardians] = [.. loopCandidates];
					continue;
				}

				// Otherwise, check whether the cell has already been added or not.
				if (!loop.Add(nextCandidate))
				{
					continue;
				}

				// If the length is greater than minimum length, we should discard it,
				// preventing greater-lengthed loops found.
				if (foundLoopsGroupedByGuardians.TryGetValue(guardians, out var minimumLoopCurrentlyFound)
					&& loopCandidates.Count >= minimumLoopCurrentlyFound.Count)
				{
					continue;
				}

				// Check whether the next guardian is held by the loop.
				// A valid guardian cannot be inside in a loop.
				if (nextGuardianIfWorth.HasValue && loop.Contains(nextGuardianIfWorth.Value))
				{
					continue;
				}

				// Continue further searching. Update data temporarily.
				loopCandidates.Add(nextCandidate);
				if (nextGuardianIfWorth.HasValue)
				{
					guardians.Add(nextGuardianIfWorth.Value);
				}

				// Do DFS.
				dfs(
					ref context,
					grid,
					nextCandidate / 9,
					nextCandidate % 9,
					currentCell,
					currentDigit,
					nextMode,
					loopCandidates,
					guardians,
					ref loop
				);

				// The branch has already been checked. Now do backtrack to revert variables' states.
				loop -= nextCandidate;
				loopCandidates.RemoveAt(^1);
				if (nextGuardianIfWorth.HasValue)
				{
					guardians.RemoveAt(^1);
				}
			}
		}
	}

	/// <summary>
	/// Check type 1.
	/// </summary>
	/// <param name="context">The context.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="guardian">The guardian.</param>
	/// <param name="loop">The loop.</param>
	/// <returns>The step found.</returns>
	private BrokenLoopType1Step? CheckType1(
		ref StepAnalysisContext context,
		in Grid grid,
		Candidate guardian,
		List<Candidate> loop
	)
	{
		var linkOffsets = GetLinkViewNodes(loop, [guardian], out var candidateOffsets);
		var step = new BrokenLoopType1Step(
			Array.Single(new Conclusion(Assignment, guardian)),
			[[.. candidateOffsets, .. linkOffsets]],
			context.Options,
			loop.AsMemory(),
			guardian
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;
	}

	/// <summary>
	/// Check type 2.
	/// </summary>
	/// <param name="context">The context.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="guardians">The guardians.</param>
	/// <param name="loop">The loop.</param>
	/// <returns>The step found.</returns>
	private BrokenLoopType2Step? CheckType2(
		ref StepAnalysisContext context,
		in Grid grid,
		List<Candidate> guardians,
		List<Candidate> loop
	)
	{
		// Check whether eliminations can be found.
		var guardiansMap = guardians.AsSpan().AsCandidateMap();
		var digitsMask = guardiansMap.Digits;
		if (!IsPow2(digitsMask))
		{
			return null;
		}

		var guardianDigit = Log2((uint)digitsMask);
		var elimMap = guardiansMap.Cells % CandidatesMap[guardianDigit];
		if (!elimMap)
		{
			return null;
		}

		var linkOffsets = GetLinkViewNodes(loop, guardians, out var candidateOffsets);
		var step = new BrokenLoopType2Step(
			(from cell in elimMap select new Conclusion(Elimination, cell, guardianDigit)).ToArray(),
			[[.. candidateOffsets, .. linkOffsets]],
			context.Options,
			loop.AsMemory(),
			guardiansMap
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;
	}

	/// <summary>
	/// Check type 3.
	/// </summary>
	/// <param name="context">The context.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="guardians">The guardians.</param>
	/// <param name="loop">The loop.</param>
	/// <returns>The step found.</returns>
	private BrokenLoopType3Step? CheckType3(
		ref StepAnalysisContext context,
		in Grid grid,
		List<Candidate> guardians,
		List<Candidate> loop
	)
	{
		// Check guardian size.
		if (guardians is not [var first, var second] || first / 9 == second / 9 || first % 9 == second % 9)
		{
			return null;
		}

		// Get cells of guardians.
		var guardianCells = CellMap.Empty;
		var guardianDigitsMask = (Mask)0;
		foreach (var guardian in guardians)
		{
			guardianCells += guardian / 9;
			guardianDigitsMask |= (Mask)(1 << guardian % 9);
		}

		// Iterate on each shared house.
		foreach (var house in guardianCells.SharedHouses)
		{
			var availableCells = HousesMap[house] & EmptyCells & ~guardianCells;
			if (availableCells.Count < 2)
			{
				// Not enough cells to be iterated.
				continue;
			}

			// Iterate on each combination of cells.
			foreach (ref readonly var cells in availableCells | availableCells.Count - 1)
			{
				// Determine whether size matches.
				var digitsMask = (Mask)(guardianDigitsMask | grid[cells]);
				if (PopCount((uint)digitsMask) != cells.Count + 1)
				{
					continue;
				}

				// Type 3 found.
				var conclusions = new List<Conclusion>();
				foreach (var digit in digitsMask)
				{
					foreach (var cell in HousesMap[house] & CandidatesMap[digit] & ~(cells | guardianCells))
					{
						conclusions.Add(new(Elimination, cell, digit));
					}
				}
				if (conclusions.Count == 0)
				{
					// No eliminations found.
					continue;
				}

				var linkOffsets = GetLinkViewNodes(loop, guardians, out var candidateOffsets);
				foreach (var cell in cells)
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						if (candidateOffsets.FindIndex(node => node.Candidate == cell * 9 + digit) is var i and not -1)
						{
							candidateOffsets.RemoveAt(i);
						}
						candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, cell * 9 + digit));
					}
				}

				var step = new BrokenLoopType3Step(
					conclusions.AsMemory(),
					[[.. candidateOffsets, .. linkOffsets]],
					context.Options,
					loop.AsMemory(),
					[.. guardians],
					cells,
					digitsMask
				);
				if (context.OnlyFindOne)
				{
					return step;
				}

				context.Accumulator.Add(step);
			}
		}
		return null;
	}

	/// <summary>
	/// Construct view nodes.
	/// </summary>
	/// <param name="loop">The loop.</param>
	/// <param name="guardians">The guardians.</param>
	/// <param name="candidateOffsets">Candidate view nodes.</param>
	/// <returns>Link view nodes.</returns>
	private ReadOnlySpan<ChainLinkViewNode> GetLinkViewNodes(
		List<Candidate> loop,
		List<Candidate> guardians,
		out List<CandidateViewNode> candidateOffsets
	)
	{
		candidateOffsets = [];
		var linkOffsets = new List<ChainLinkViewNode>();
		foreach (var candidate in loop)
		{
			candidateOffsets.Add(new(ColorIdentifier.Normal, candidate));
		}
		foreach (var guardian in guardians)
		{
			candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, guardian));
		}
		for (var i = 0; i < loop.Count; i++)
		{
			var first = loop[i];
			var second = loop[(i + 1) % loop.Count];
			linkOffsets.Add(new(ColorIdentifier.Normal, first.AsCandidateMap(), second.AsCandidateMap(), true));
		}
		return linkOffsets.AsSpan();
	}
}
