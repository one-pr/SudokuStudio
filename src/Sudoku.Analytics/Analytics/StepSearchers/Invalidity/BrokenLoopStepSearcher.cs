namespace Sudoku.Analytics.StepSearchers;

/// <summary>
/// Provides with a <b>Broken Loop</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Broken Loop Type 1</item>
/// <item>Broken Loop Type 2</item>
/// <item>Broken Loop Type 3</item>
/// <item>Broken Loop Type 4</item>
/// </list>
/// </summary>
[StepSearcher(
	"StepSearcherName_BrokenLoopStepSearcher",
	Technique.BrokenLoopType1, Technique.BrokenLoopType2,
	Technique.BrokenLoopType3, Technique.BrokenLoopType4)]
public sealed partial class BrokenLoopStepSearcher
{
	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		ref readonly var grid = ref context.Grid;

		// Iterate on all non-bivalue cells.
		var tempLoop = CandidateMap.Empty;
		var traversedSpaces = SpaceSet.Empty;
		var tempGuardians = CandidateMap.Empty;
		var foundLoopsGroupedByGuardians = new Dictionary<CandidateMap, List<Candidate>>();
		foreach (var cell in EmptyCells & BivalueCells)
		{
			// Iterate on each extra start.
			var d1 = BitOperations.PopTwo((uint)grid.GetCandidates(cell), out var d2);
			tempLoop.Clear();
			tempLoop += cell * 9 + d1;
			tempLoop += cell * 9 + d2;
			traversedSpaces.Clear();
			tempGuardians.Clear();
			dfs(
				ref context,
				grid,
				cell,
				d2,
				cell,
				d2,
				(NextSpaceType)(-1),
				[cell * 9 + d1, cell * 9 + d2],
				tempGuardians,
				ref traversedSpaces,
				tempLoop
			);
			if (context.CancellationToken.IsCancellationRequested)
			{
				// Canceled.
				return null;
			}
		}

		// Check for subtypes.
		foreach (var kvp in foundLoopsGroupedByGuardians)
		{
			ref readonly var guardians = ref kvp.KeyRef;
			var loop = kvp.Value;
			switch (guardians)
			{
				// Invalid grid (no valid solutions found).
				case []:
				{
					throw new PuzzleInvalidException(grid, typeof(BrokenLoopStepSearcher));
				}

				// Check for type 1.
				case [var guardian]:
				{
					if (CheckType1(ref context, grid, guardian, loop) is { } type1Step)
					{
						return type1Step;
					}
					break;
				}

				// Check for type 2, 3, 4.
				default:
				{
					if (CheckType2(ref context, grid, guardians, loop) is { } type2Step)
					{
						return type2Step;
					}
					if (CheckType3(ref context, grid, guardians, loop) is { } type3Step)
					{
						return type3Step;
					}
					if (CheckType4(ref context, grid, guardians, loop) is { } type4Step)
					{
						return type4Step;
					}
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
			NextSpaceType unitTypeMode,
			List<Candidate> loopCandidates,
			CandidateMap guardians,
			ref SpaceSet traversedSpaces,
			CandidateMap loop
		)
		{
			if (context.CancellationToken.IsCancellationRequested)
			{
				// User canceled.
				return;
			}

			// Check whether guardian candidates overlap with loop or not. If so, invalid.
			if (guardians & loop)
			{
				return;
			}

			if (!!guardians
				&& BitOperations.PopCount((uint)guardians.Digits) != 1 // Type 1 and 2
				&& guardians.Cells is not { Count: 2, FirstSharedHouse: not FallbackConstants.@int }) // Type 3 and 4
			{
				return;
			}

			var availableNext = new List<(NextSpaceType NextMode, Candidate NextCandidate, Candidate? NewGuardian)>();
			for (var mode = NextSpaceType.Cell; mode <= NextSpaceType.Column; mode++)
			{
				if (mode == unitTypeMode)
				{
					continue;
				}

				if (mode == NextSpaceType.Cell)
				{
					if (previousCell == currentCell)
					{
						// Same cell.
						continue;
					}

					// Check digits in the current cell.
					var digitsMask = grid.GetCandidates(currentCell);
					var otherDigitsMask = (Mask)(digitsMask & ~(1 << currentDigit));
					switch (BitOperations.PopCount((uint)otherDigitsMask))
					{
						default:
						{
							// Invalid.
							continue;
						}
						case 1:
						{
							var otherDigit = BitOperations.Log2((uint)otherDigitsMask);
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
								var nextGuardian = currentCell * 9 + BitOperations.Log2((uint)(otherDigitsMask & ~(1 << digit)));
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
					if ((int)sharedHouse.HouseType == (int)(mode - 1))
					{
						// Same house.
						continue;
					}

					// Get for the next house.
					var nextHouse = currentCell.GetHouse((HouseType)(mode - 1));
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
				// Now check whether an odd-length loop is formed if we add next candidate into the loop.
				if (nextCandidate == loopCandidates[0] && (loopCandidates.Count & 1) == 1)
				{
					// Construct guardians map.
					var tempGuardians = guardians;
					if (nextGuardianIfWorth.HasValue)
					{
						tempGuardians += nextGuardianIfWorth.Value;
					}

					if (loop & tempGuardians)
					{
						// Invalid as rescue check.
						continue;
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
					return;
				}

				// Otherwise, check whether the cell has already been added or not. If so, invalid.
				if (loop.Contains(nextCandidate))
				{
					continue;
				}

				var nextCell = nextCandidate / 9;
				var nextSpace = nextMode switch
				{
					NextSpaceType.Cell => Space.RowColumn(nextCell / 9, nextCell % 9),
					NextSpaceType.Block => Space.BlockDigit(nextCell.GetHouse(HouseType.Block), nextCandidate % 9),
					NextSpaceType.Row => Space.RowDigit(nextCell.GetHouse(HouseType.Row) - 9, nextCandidate % 9),
					_ => Space.RowDigit(nextCell.GetHouse(HouseType.Column) - 18, nextCandidate % 9)
				};

				// Check whether the candidate is traversed.
				// This pruning operation will miss some cases, but large faster than before.
				// e.g. type 1:
				//   456+2+398....+8+4+67+2+5...25+8+1+6.4..91+2+3+46.2.+1.4+5..8.4+3.+781+2.9.+4+8+527...+2+57+1+4.+8...73+9+6542:129 738 172
				if (traversedSpaces.Contains(nextSpace))
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

				// Continue further searching. Update data temporarily.
				loopCandidates.Add(nextCandidate);
				traversedSpaces += nextSpace;

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
					nextGuardianIfWorth.HasValue ? guardians + nextGuardianIfWorth.Value : guardians,
					ref traversedSpaces,
					loop + nextCandidate
				);

				// The branch has already been checked. Now do backtrack to revert variables' states.
				loopCandidates.RemoveAt(^1);
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
		var linkOffsets = GetLinkViewNodes(loop, guardian.AsCandidateMap(), out var candidateOffsets);
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
		in CandidateMap guardians,
		List<Candidate> loop
	)
	{
		// Check whether eliminations can be found.
		var digitsMask = guardians.Digits;
		if (!BitOperations.IsPow2(digitsMask))
		{
			return null;
		}

		var guardianDigit = BitOperations.Log2((uint)digitsMask);
		var elimMap = guardians.Cells % CandidatesMap[guardianDigit];
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
			guardians
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
		in CandidateMap guardians,
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
			foreach (ref readonly var subsetCells in availableCells | availableCells.Count - 1)
			{
				// Determine whether size matches.
				var digitsMask = (Mask)(guardianDigitsMask | grid[subsetCells]);
				if (BitOperations.PopCount((uint)digitsMask) != subsetCells.Count + 1)
				{
					continue;
				}

				// Type 3 found.
				var conclusions = new List<Conclusion>();
				foreach (var digit in digitsMask)
				{
					foreach (var cell in HousesMap[house] & CandidatesMap[digit] & ~(subsetCells | guardianCells))
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
				foreach (var cell in subsetCells)
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						if (candidateOffsets.FindIndex(node => node.Candidate == cell * 9 + digit) is var i and not -1)
						{
							candidateOffsets.RemoveAt(i);
						}
						candidateOffsets.Add(new(ColorDescriptorAlias.Auxiliary2, cell * 9 + digit));
					}
				}

				var step = new BrokenLoopType3Step(
					conclusions.AsMemory(),
					[[.. candidateOffsets, .. linkOffsets]],
					context.Options,
					loop.AsMemory(),
					guardians,
					subsetCells,
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
	/// Check type 4.
	/// </summary>
	/// <param name="context">The context.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="guardians">The guardians.</param>
	/// <param name="loop">The loop.</param>
	/// <returns>The step found.</returns>
	private BrokenLoopType4Step? CheckType4(
		ref StepAnalysisContext context,
		in Grid grid,
		in CandidateMap guardians,
		List<Candidate> loop
	)
	{
		// Type 4 must use 2 cells.
		if (guardians is not
			{
				Cells: { Count: 2, SharedHouses: var houses and not 0 } guardianCells,
				Digits: var guardianDigitsMask
			})
		{
			return null;
		}

		// Check whether two cells has a conjugate pair or not.
		var cell1DigitsMask = (Mask)(grid.GetCandidates(guardianCells[0]) & ~guardianDigitsMask);
		var cell2DigitsMask = (Mask)(grid.GetCandidates(guardianCells[1]) & ~guardianDigitsMask);
		foreach (var conjugatePairDigit in (Mask)(cell1DigitsMask & cell2DigitsMask))
		{
			// Iterate on each shared house.
			foreach (var house in houses)
			{
				// Check whether the digit has a conjugate pair.
				if ((HousesMap[house] & CandidatesMap[conjugatePairDigit]) != guardianCells)
				{
					continue;
				}

				// Okay. Now the digit is a conjugate pair.
				// Check eliminations.
				var conclusions = new List<Conclusion>();
				foreach (var cell in guardianCells)
				{
					var guardianDigitsMaskThisCell = guardians.GetDigitsFor(cell);
					foreach (var digit in (Mask)(grid.GetCandidates(cell) & ~guardianDigitsMaskThisCell & ~(1 << conjugatePairDigit)))
					{
						conclusions.Add(new(Elimination, cell, digit));
					}
				}
				if (conclusions.Count == 0)
				{
					continue;
				}

				var linkOffsets = GetLinkViewNodes(loop, guardians, out var candidateOffsets);
				candidateOffsets.Add(new(ColorDescriptorAlias.Auxiliary2, guardianCells[0] * 9 + conjugatePairDigit));
				candidateOffsets.Add(new(ColorDescriptorAlias.Auxiliary2, guardianCells[1] * 9 + conjugatePairDigit));

				var step = new BrokenLoopType4Step(
					conclusions.AsMemory(),
					[
						[
							.. candidateOffsets,
							.. linkOffsets,
							new ConjugateLinkViewNode(ColorDescriptorAlias.Normal, guardianCells[0], guardianCells[1], conjugatePairDigit)
						]
					],
					context.Options,
					loop.AsMemory(),
					guardians,
					new(guardianCells, conjugatePairDigit)
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
		in CandidateMap guardians,
		out List<CandidateViewNode> candidateOffsets
	)
	{
		candidateOffsets = [];
		var linkOffsets = new List<ChainLinkViewNode>();
		foreach (var candidate in loop)
		{
			candidateOffsets.Add(new(ColorDescriptorAlias.Normal, candidate));
		}
		foreach (var guardian in guardians)
		{
			candidateOffsets.Add(new(ColorDescriptorAlias.Auxiliary1, guardian));
		}
		for (var i = 0; i < loop.Count; i++)
		{
			var first = loop[i];
			var second = loop[(i + 1) % loop.Count];
			linkOffsets.Add(new(ColorDescriptorAlias.Normal, first.AsCandidateMap(), second.AsCandidateMap(), true));
		}
		return linkOffsets.AsSpan();
	}
}

/// <summary>
/// Represents a file-local type defining four kinds of spaces to be searched.
/// </summary>
file enum NextSpaceType : sbyte
{
	/// <summary>
	/// Indicates the mode is to search for cell.
	/// </summary>
	Cell,

	/// <summary>
	/// Indicates the mode is to search for block.
	/// </summary>
	Block,

	/// <summary>
	/// Indicates the mode is to search for row.
	/// </summary>
	Row,

	/// <summary>
	/// Indicates the mode is to search for column.
	/// </summary>
	Column
}
