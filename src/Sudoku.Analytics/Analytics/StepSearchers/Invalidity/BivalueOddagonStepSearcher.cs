namespace Sudoku.Analytics.StepSearchers;

/// <summary>
/// <para>
/// Provides with a <b>Bi-value Oddagon</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <!--<item>Bi-value Oddagon Type 1</item>-->
/// <item>Bi-value Oddagon Type 2</item>
/// <item>Bi-value Oddagon Type 3</item>
/// <item>Bi-value Oddagon Type 4</item>
/// </list>
/// </para>
/// <para>
/// In practicing, type 1 doesn't exist. A bi-value oddagon type 1 is a remote pair.
/// A Remote Pair is a XY-Chain, only using two digits.
/// </para>
/// </summary>
[StepSearcher(
	"StepSearcherName_BivalueOddagonStepSearcher",
	Technique.BivalueOddagonType2, Technique.BivalueOddagonType3, Technique.BivalueOddagonType4)]
public sealed partial class BivalueOddagonStepSearcher : StepSearcher
{
	/// <summary>
	/// The maximum number of loops can be searched for in code.
	/// </summary>
	private const int MaximumCount = 100;


	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		if (BivalueCells.Count < 4)
		{
			return null;
		}

		var tempAccumulator = context.OnlyFindOne ? null : new SortedSet<BivalueOddagonStep>();

		// Now iterate on each bi-value cells as the start cell to get all possible unique loops,
		// making it the start point to execute the recursion.
		ref readonly var grid = ref context.Grid;
		var onlyFindOne = context.OnlyFindOne;
		if (collect(grid) is not { Count: not 0 } oddagonInfoList)
		{
			return null;
		}

		foreach (var (currentLoop, extraCells, comparer) in oddagonInfoList)
		{
			var d1 = BitOperations.PopTwo((uint)comparer, out var d2);
			var hasType2HandledByNot1CaseBranch = false;
			switch (extraCells.Count)
			{
				// This puzzle has no puzzle solutions.
				case 0:
				{
					throw new PuzzleInvalidException(grid, typeof(BivalueOddagonStepSearcher));
				}

#if false
				// Type 1 (but here we can directly ignore the case).
				case 1:
				{
					break;
				}
#endif

				// Type 3 and 4.
				case 2:
				{
					if (!hasType2HandledByNot1CaseBranch
						&& CheckType2(tempAccumulator, grid, ref context, d1, d2, currentLoop, extraCells, comparer) is { } step2)
					{
						return step2;
					}
					if (CheckType3(tempAccumulator, grid, ref context, d1, d2, currentLoop, extraCells, comparer) is { } step3)
					{
						return step3;
					}
					if (CheckType4(tempAccumulator, grid, ref context, d1, d2, currentLoop, extraCells, comparer) is { } step4)
					{
						return step4;
					}
					break;
				}

				// Type 2.
				case not 1:
				{
					if (CheckType2(tempAccumulator, grid, ref context, d1, d2, currentLoop, extraCells, comparer) is { } step2)
					{
						return step2;
					}
					hasType2HandledByNot1CaseBranch = true;
					goto case 2;
				}
			}
		}

		if (context.OnlyFindOne && tempAccumulator is { Count: not 0 })
		{
			return tempAccumulator.Min;
		}
		if (!context.OnlyFindOne && tempAccumulator!.Count != 0)
		{
			context.Accumulator.AddRange(tempAccumulator);
		}
		return null;


		static HashSet<BivalueOddagonPattern> collect(in Grid grid)
		{
			var (foundLoopsCount, result) = (-1, new HashSet<BivalueOddagonPattern>(MaximumCount));
			for (var d1 = 0; d1 < 8; d1++)
			{
				for (var d2 = d1 + 1; d2 < 9; d2++)
				{
					var comparer = (Mask)(1 << d1 | 1 << d2);
					var cellsContainingBothTwoDigits = CandidatesMap[d1] & CandidatesMap[d2];
					foreach (var cell in cellsContainingBothTwoDigits)
					{
						dfs(
							grid,
							cell,
							cell,
							-1,
							cellsContainingBothTwoDigits,
							cell.AsCellMap(),
							BitOperations.PopCount((uint)grid.GetCandidates(cell)) > 2 ? [cell] : [],
							result,
							ref foundLoopsCount,
							comparer,
							0
						);
					}
				}
			}
			return result;
		}

		static void dfs(
			in Grid grid,
			Cell startCell,
			Cell previousCell,
			House previousHouse,
			in CellMap cellsContainingBothDigits,
			in CellMap loop,
			in CellMap extraCells,
			HashSet<BivalueOddagonPattern> result,
			ref int loopsCount,
			Mask comparer,
			Mask extraDigitsMask
		)
		{
			if (loopsCount > MaximumCount)
			{
				// There is no need to iterate more loops because they are same.
				return;
			}

			var h = (stackalloc House[3]);
			foreach (var houseType in HouseTypes)
			{
				var nextHouse = previousCell.GetHouse(houseType);
				if (nextHouse == previousHouse)
				{
					continue;
				}

				var loopCellsInThisHouse = loop & HousesMap[nextHouse];
				if (loopCellsInThisHouse.Count >= 2 && !loopCellsInThisHouse.Contains(startCell))
				{
					continue;
				}

				var otherCellsCanBeIterated = (cellsContainingBothDigits & ~loop) + startCell & HousesMap[nextHouse];
				if (!otherCellsCanBeIterated)
				{
					continue;
				}

				foreach (var cell in otherCellsCanBeIterated)
				{
					cell.CopyHouseInfo(ref h[0]);

					if (((loop & HousesMap[h[0]]).Count >= 2 || (loop & HousesMap[h[1]]).Count >= 2 || (loop & HousesMap[h[2]]).Count >= 2)
						&& startCell != cell)
					{
						// All valid loops can only at most 2 cells from all houses that the current loop uses.
						continue;
					}

					if (startCell == cell)
					{
						if ((loop.Count & 1) != 0 && loop.Count > 4)
						{
							// The pattern is found.
							if (++loopsCount < MaximumCount)
							{
								result.Add(new(loop, extraCells, comparer));
							}

							return;
						}
					}
					else
					{
						var newExtraDigitsMask = (Mask)(extraDigitsMask | (Mask)(grid.GetCandidates(cell) & ~comparer));
						var newExtraCells = BitOperations.PopCount((uint)grid.GetCandidates(cell)) > 2
							? extraCells + cell
							: extraCells;
						if (newExtraCells.FirstSharedHouse != FallbackConstants.@int
							|| BitOperations.IsPow2(newExtraDigitsMask)
							&& !!(newExtraCells.PeerIntersection & CandidatesMap[BitOperations.Log2((uint)newExtraDigitsMask)])
							|| newExtraCells.Count < 3)
						{
							dfs(
								grid,
								startCell,
								cell,
								nextHouse,
								cellsContainingBothDigits,
								loop + cell,
								newExtraCells,
								result,
								ref loopsCount,
								comparer,
								newExtraDigitsMask
							);
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// Check for type 2.
	/// </summary>
	private BivalueOddagonType2Step? CheckType2(
		SortedSet<BivalueOddagonStep>? accumulator,
		in Grid grid,
		ref StepAnalysisContext context,
		Digit d1,
		Digit d2,
		in CellMap loop,
		in CellMap extraCellsMap,
		Mask comparer
	)
	{
		var mask = (Mask)(grid[extraCellsMap] & ~comparer);
		if (!BitOperations.IsPow2(mask))
		{
			goto ReturnNull;
		}

		var extraDigit = BitOperations.TrailingZeroCount(mask);
		if (extraCellsMap % CandidatesMap[extraDigit] is not (var elimMap and not []))
		{
			goto ReturnNull;
		}

		var candidateOffsets = new List<CandidateViewNode>();
		foreach (var cell in loop)
		{
			foreach (var digit in grid.GetCandidates(cell))
			{
				candidateOffsets.Add(new(digit == extraDigit ? ColorDescriptorAlias.Auxiliary1 : ColorDescriptorAlias.Normal, cell * 9 + digit));
			}
		}

		var hamiltonianLoop = new CellGraph(loop).GetHamiltonianCycles() is [var l, ..] ? l : default;
		var links = new List<CellLinkViewNode>();
		foreach (var (first, second) in hamiltonianLoop.EnumerateAdjacentCells())
		{
			links.Add(new(ColorDescriptorAlias.Normal, first, second));
		}

		var step = new BivalueOddagonType2Step(
			(from cell in elimMap select new Conclusion(Elimination, cell, extraDigit)).ToArray(),
			[[.. candidateOffsets, .. links]],
			context.Options,
			loop,
			d1,
			d2,
			extraDigit
		);

		if (context.OnlyFindOne)
		{
			return step;
		}

		accumulator!.Add(step);

	ReturnNull:
		return null;
	}

	/// <summary>
	/// Check for type 3.
	/// </summary>
	private BivalueOddagonType3Step? CheckType3(
		SortedSet<BivalueOddagonStep>? accumulator,
		in Grid grid,
		ref StepAnalysisContext context,
		Digit d1,
		Digit d2,
		in CellMap loop,
		in CellMap extraCellsMap,
		Mask comparer
	)
	{
		var notSatisfiedType3 = false;
		foreach (var cell in extraCellsMap)
		{
			var mask = grid.GetCandidates(cell);
			if ((mask & comparer) == 0 || mask == comparer)
			{
				notSatisfiedType3 = true;
				break;
			}
		}

		if (extraCellsMap.FirstSharedHouse == FallbackConstants.@int || notSatisfiedType3)
		{
			goto ReturnNull;
		}

		var m = grid[extraCellsMap];
		if ((m & comparer) != comparer)
		{
			goto ReturnNull;
		}

		var otherDigitsMask = (Mask)(m & ~comparer);
		foreach (var house in extraCellsMap.SharedHouses)
		{
			if ((ValuesMap[d1] | ValuesMap[d2]) & HousesMap[house])
			{
				goto ReturnNull;
			}

			var otherCells = HousesMap[house] & EmptyCells & ~loop;
			for (var size = BitOperations.PopCount((uint)otherDigitsMask) - 1; size < otherCells.Count; size++)
			{
				foreach (ref readonly var cells in otherCells & size)
				{
					var mask = grid[cells];
					if (BitOperations.PopCount((uint)mask) != size + 1 || (mask & otherDigitsMask) != otherDigitsMask)
					{
						continue;
					}

					if ((HousesMap[house] & EmptyCells & ~cells & ~loop) is not (var elimMap and not []))
					{
						continue;
					}

					var conclusions = new List<Conclusion>(16);
					foreach (var digit in mask)
					{
						foreach (var cell in elimMap & CandidatesMap[digit])
						{
							conclusions.Add(new(Elimination, cell, digit));
						}
					}
					if (conclusions.Count == 0)
					{
						continue;
					}

					var candidateOffsets = new List<CandidateViewNode>();
					foreach (var cell in loop)
					{
						foreach (var digit in grid.GetCandidates(cell))
						{
							candidateOffsets.Add(
								new(
									(otherDigitsMask >> digit & 1) != 0 ? ColorDescriptorAlias.Auxiliary1 : ColorDescriptorAlias.Normal,
									cell * 9 + digit
								)
							);
						}
					}
					foreach (var cell in cells)
					{
						foreach (var digit in grid.GetCandidates(cell))
						{
							candidateOffsets.Add(new(ColorDescriptorAlias.Auxiliary1, cell * 9 + digit));
						}
					}

					var hamiltonianLoop = new CellGraph(loop).GetHamiltonianCycles() is [var l, ..] ? l : default;
					var links = new List<CellLinkViewNode>();
					foreach (var (first, second) in hamiltonianLoop.EnumerateAdjacentCells())
					{
						links.Add(new(ColorDescriptorAlias.Normal, first, second));
					}
					var step = new BivalueOddagonType3Step(
						conclusions.AsMemory(),
						[[.. candidateOffsets, new HouseViewNode(ColorDescriptorAlias.Normal, house), .. links]],
						context.Options,
						loop,
						d1,
						d2,
						cells,
						mask
					);

					if (context.OnlyFindOne)
					{
						return step;
					}

					accumulator!.Add(step);
				}
			}
		}

	ReturnNull:
		return null;
	}

	/// <summary>
	/// Check for type 4.
	/// </summary>
	private BivalueOddagonType4Step? CheckType4(
		SortedSet<BivalueOddagonStep>? accumulator,
		in Grid grid,
		ref StepAnalysisContext context,
		Digit d1,
		Digit d2,
		in CellMap loop,
		in CellMap extraCellsMap,
		Mask comparer
	)
	{
		var mask = (Mask)(grid[extraCellsMap] & ~comparer);
		if (!BitOperations.IsPow2(mask))
		{
			goto ReturnNull;
		}
		if (extraCellsMap is not [var firstExtraCell, var secondExtraCell])
		{
			goto ReturnNull;
		}

		var extraDigit = BitOperations.TrailingZeroCount(mask);

		// Get hamiltonian cycle here.
		// Why here? Because we should know two cells from extra cells map must be adjacent with each other in the loop.
		// This is important.
		var hamiltonianLoop = new CellGraph(loop).GetHamiltonianCycles() is [var l, ..] ? l : default;
		if (!hamiltonianLoop.IsAdjacentWithEachOther(firstExtraCell, secondExtraCell))
		{
			goto ReturnNull;
		}

		// Find for two cells that is on the loop, and they can see one of the conjugate pair of 'extraDigit' here.
		var loopCellsCanSeeAnyCellOfConjugatePair = loop & ~extraCellsMap & extraCellsMap.ExpandedPeers;

		// Why we should know they are adjacent with each other?
		// Because in the odd-length loop, the cells will be connected like:
		//
		//   inCell -> firstExtraCell -> secondExtraCell -> outCell
		//
		// if 'firstExtraCell' and 'secondExtraCell' are adjacent in the loop,
		// we will know that they must be assumed with different digits.
		// At the same time, we know that the number of all last cells
		// not calculated in the conjugate pair is also an odd number (loopLength (odd) - 2 is also an odd value).
		// Therefore, we can know that two cells in 'loopCellsCanSeeAnyCellOfConjugatePair' will be assumed with a same digit.

		// Now check for two cells in the conjugate pair.
		var isAnyLoopCellSeeingBothCells = false;
		var elimMap = CandidateMap.Empty;
		var elimMapTemplate = CandidatesMap[d1] | CandidatesMap[d2];
		foreach (var extraCell in extraCellsMap)
		{
			// Get the cell that in loop that can see this cell.
			if ((loopCellsCanSeeAnyCellOfConjugatePair & Peer.PeersMap[extraCell]) is not [var loopCellCanSeeExtraCell])
			{
				isAnyLoopCellSeeingBothCells = true;
				break;
			}

			// Cells 'extraCell' and 'loopCellCanSeeExtraCell' here are always adjacent in loop,
			// with different digits assumed.
			// Here we should assume cells to form a naked pair of digits 'd1' and 'd2',
			// and find for eliminations.
			var localElimMap = CandidateMap.Empty;
			foreach (var loopCellCanSeeConjugatePair in loopCellsCanSeeAnyCellOfConjugatePair)
			{
				foreach (var cell in (extraCell.AsCellMap() + loopCellCanSeeConjugatePair).PeerIntersection & elimMapTemplate & ~loop)
				{
					if (grid.Exists(cell, d1) is true)
					{
						localElimMap += cell * 9 + d1;
					}
					if (grid.Exists(cell, d2) is true)
					{
						localElimMap += cell * 9 + d2;
					}
				}
			}
			if (elimMap)
			{
				elimMap &= localElimMap;
			}
			else
			{
				elimMap |= localElimMap;
			}
		}
		if (isAnyLoopCellSeeingBothCells)
		{
			goto ReturnNull;
		}
		if (!elimMap)
		{
			goto ReturnNull;
		}

		var candidateOffsets = new List<CandidateViewNode>();
		foreach (var cell in loop)
		{
			foreach (var digit in grid.GetCandidates(cell))
			{
				candidateOffsets.Add(new(digit == extraDigit ? ColorDescriptorAlias.Auxiliary1 : ColorDescriptorAlias.Normal, cell * 9 + digit));
			}
		}

		var links = new List<CellLinkViewNode>();
		foreach (var (first, second) in hamiltonianLoop.EnumerateAdjacentCells())
		{
			links.Add(new(ColorDescriptorAlias.Normal, first, second));
		}

		var step = new BivalueOddagonType4Step(
			(from candidate in elimMap select new Conclusion(Elimination, candidate)).ToArray(),
			[
				[
					.. candidateOffsets,
					.. links,
					new ConjugateLinkViewNode(ColorDescriptorAlias.Normal, extraCellsMap[0], extraCellsMap[1], extraDigit)
				]
			],
			context.Options,
			loop,
			d1,
			d2,
			new(extraCellsMap, extraDigit)
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		accumulator!.Add(step);

	ReturnNull:
		return null;
	}
}
