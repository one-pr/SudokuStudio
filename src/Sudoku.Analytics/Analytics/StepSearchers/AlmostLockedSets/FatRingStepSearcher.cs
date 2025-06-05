namespace Sudoku.Analytics.StepSearchers;

/// <summary>
/// Provides with a <b>Fat Ring</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Fat Ring</item>
/// </list>
/// </summary>
[StepSearcher(
	"StepSearcherName_FatRingStepSearcher",
	Technique.FatRing, Technique.XyzLoop, Technique.GroupedXyzLoop)]
public sealed partial class FatRingStepSearcher : StepSearcher
{
	/// <summary>
	/// Indicates the detailed pattern positions.
	/// </summary>
	private static readonly (RowIndex Row, ColumnIndex Column, BlockIndex[] CanceledBlocks)[] PatternPositions;


	/// <include file='../../global-doc-comments.xml' path='g/static-constructor' />
	static FatRingStepSearcher()
	{
		var positions = new List<(RowIndex, ColumnIndex, BlockIndex[])>(81);
		for (var row = 9; row < 18; row++)
		{
			for (var column = 18; column < 27; column++)
			{
				var canceledBlocks = new List<BlockIndex>(4);
				for (var block = 0; block < 9; block++)
				{
					if (!((HousesMap[row] | HousesMap[column]) & HousesMap[block]))
					{
						canceledBlocks.Add(block);
					}
				}

				positions.Add((row, column, [.. canceledBlocks]));
			}
		}
		PatternPositions = [.. positions];
	}


	/// <inheritdoc/>
	/// <remarks>
	/// <include file="../../global-doc-comments.xml" path="/g/developer-notes"/>
	/// <para>
	/// The pattern will use a row and a column, with multiple blocks. For example, row 1, column 1 and blocks in [5, 6, 8, 9].
	/// Some empty cells in row 1 and column 1 will be chosen, with each digit satisfying either the specified conditions:
	/// <list type="bullet">
	/// <item>the digit can be appeared at most once</item>
	/// <item>
	/// the digit can be appeared at most twice, but two different positions must be connected
	/// with a strong link (or a grouped strong link, i.e. empty rectangle) in extra blocks
	/// </item>
	/// </list>
	/// This is a dynamic loop, extended from technique "XYZ-Ring" (XYZ Loop) technique.
	/// </para>
	/// </remarks>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		ref readonly var grid = ref context.Grid;
		var digitsDistributionMap = new Dictionary<Digit, CellMap>(9);

		// Iterate patterns.
		foreach (var (row, column, canceledBlocks) in PatternPositions.AsReadOnlySpan())
		{
			// Check for row and column information, to collection empty cells to be iterated.
			var rowCells = HousesMap[row] & EmptyCells;
			var columnCells = HousesMap[column] & EmptyCells;

			// Iterate on empty cells, such that the number of cells chosen in row is >= 2,
			// and the number of cells chosen in column is >= 2.
			// In addition, the number of all cells chosen is <= 9.
			for (var rowCellsCount = 2; rowCellsCount <= Math.Min(7, rowCells.Count); rowCellsCount++)
			{
				foreach (ref readonly var rowCellsChosen in rowCells & rowCellsCount)
				{
					for (var columnCellsCount = 2; rowCellsCount + columnCellsCount <= 10; columnCellsCount++)
					{
						foreach (ref readonly var columnCellsChosen in columnCells & columnCellsCount)
						{
							var intersected = columnCellsChosen & rowCellsChosen;
							if ((HousesMap[row] & HousesMap[column]) != intersected)
							{
								// The chosen cells in column cannot be same as row.
								continue;
							}

							if (!!(EmptyCells & (rowCellsChosen & columnCellsChosen)) && rowCellsCount + columnCellsCount == 10)
							{
								// The number of cells chosen is exceeded.
								continue;
							}

							var allCells = rowCellsChosen | columnCellsChosen;
							var allDigitsMask = grid[allCells];
							if (BitOperations.PopCount(allDigitsMask) != allCells.Count)
							{
								// The desired number of digits appeared in cells must be equal to the number of cells of pattern.
								continue;
							}

							// Collect digits that can only appear at most once,
							// and the others should be checked for existence of empty rectangle.
							digitsDistributionMap.Clear();

							var digitsCanAppearAtMostOnceMask = (Mask)0;
							foreach (var digit in allDigitsMask)
							{
								var cells = CandidatesMap[digit] & allCells;
								digitsDistributionMap.Add(digit, cells);

								if (cells.SharedLine == FallbackConstants.@int)
								{
									// There's no shared house - the digit can be appeared twice or more.
									continue;
								}

								digitsCanAppearAtMostOnceMask |= (Mask)(1 << digit);
							}

							// If no digits can appear twice or more, the rank-0 pattern will be formed,
							// meaning we can eliminate all digits on their own appearing directions.
							if (digitsCanAppearAtMostOnceMask == allDigitsMask)
							{
								// Collect eliminations.
								var conclusions = new List<Conclusion>();
								foreach (var digit in allDigitsMask)
								{
									var elimMap = HousesMap[digitsDistributionMap[digit].SharedLine] & CandidatesMap[digit] & ~allCells;
									foreach (var cell in elimMap)
									{
										conclusions.Add(new(Elimination, cell, digit));
									}
								}
								if (conclusions.Count == 0)
								{
									continue;
								}

								// Collect view nodes.
								var candidateOffsets = new List<CandidateViewNode>();
								foreach (var cell in allCells)
								{
									foreach (var digit in grid.GetCandidates(cell))
									{
										candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + digit));
									}
								}

								// Add the step to the collection.
								var step = new FatRingStep(
									conclusions.AsMemory(),
									[[.. candidateOffsets]],
									context.Options,
									row,
									column,
									0,
									allDigitsMask,
									0,
									Technique.None
								);
								if (context.OnlyFindOne)
								{
									return step;
								}

								context.Accumulator.Add(step);
							}
							else
							{
								// Extra check: Check for empty rectangles.
								var digitsCanAppearTwiceOrMoreMask = (Mask)(allDigitsMask & ~digitsCanAppearAtMostOnceMask);
								var desiredCount = BitOperations.PopCount(digitsCanAppearTwiceOrMoreMask);
								var canceledHousesDictionary = new Dictionary<Digit, House>();
								foreach (var digit in digitsCanAppearTwiceOrMoreMask)
								{
									foreach (var block in canceledBlocks)
									{
										// Check whether the target block 'block' will form an empty rectangle of digit 'digit'.
										if (!EmptyRectangle.IsEmptyRectangle(CandidatesMap[digit] & HousesMap[block], block, out var r, out var c))
										{
											// Failed.
											continue;
										}

										// Check whether the row and column is covered with all digits,
										// but the intersected cell of row and column should be ignored.
										var cellsToCover = CandidatesMap[digit] & allCells & ~intersected;
										if (((HousesMap[r] | HousesMap[c]) & cellsToCover) != cellsToCover)
										{
											continue;
										}

										// Valid.
										canceledHousesDictionary.Add(digit, block);
									}
								}
								if (canceledHousesDictionary.Count != desiredCount)
								{
									// The pattern may not be valid.
									continue;
								}

								// Okay for the pattern. Now check for eliminations.

								// Phase 1: Basic eliminations.
								var conclusions = new List<Conclusion>();
								foreach (var digit in digitsCanAppearAtMostOnceMask)
								{
									var elimMap = HousesMap[digitsDistributionMap[digit].SharedLine] & CandidatesMap[digit] & ~allCells;
									foreach (var cell in elimMap)
									{
										conclusions.Add(new(Elimination, cell, digit));
									}
								}

								// Phase 2: Cannibalism.
								// If all appearances of digit as canceled one can see the intersection
								// of any cells inside the canceled block, we can remove them.
								foreach (var digit in canceledHousesDictionary.Keys)
								{
									foreach (var cell in (allCells & CandidatesMap[digit]).PeerIntersection
										& HousesMap[canceledHousesDictionary[digit]]
										& CandidatesMap[digit])
									{
										conclusions.Add(new(Elimination, cell, digit));
									}
								}
								if (conclusions.Count == 0)
								{
									continue;
								}

								// Collect view nodes.
								var candidateOffsets = new List<CandidateViewNode>();
								foreach (var cell in allCells)
								{
									foreach (var digit in grid.GetCandidates(cell))
									{
										candidateOffsets.Add(
											new(
												(digitsCanAppearTwiceOrMoreMask >> digit & 1) != 0
													? ColorIdentifier.Auxiliary1
													: ColorIdentifier.Normal,
												cell * 9 + digit
											)
										);
									}
								}
								foreach (var (digit, canceledBlock) in canceledHousesDictionary)
								{
									foreach (var cell in HousesMap[canceledBlock] & CandidatesMap[digit])
									{
										candidateOffsets.Add(new(ColorIdentifier.Auxiliary2, cell * 9 + digit));
									}
								}

								// Add the step to the collection.
								var step = new FatRingStep(
									conclusions.AsMemory(),
									[[.. candidateOffsets]],
									context.Options,
									row,
									column,
									Mask.Create(canceledHousesDictionary.Values),
									allDigitsMask,
									digitsCanAppearTwiceOrMoreMask,
									(allCells, canceledHousesDictionary.First()) switch
									{
										({ Count: not 3 }, _) => Technique.None,
										(_, var (d, c)) => (HousesMap[c] & CandidatesMap[d]).Count switch
										{
											> 2 => Technique.GroupedXyzLoop,
											_ => Technique.XyzLoop
										}
									}
								);
								if (context.OnlyFindOne)
								{
									return step;
								}

								context.Accumulator.Add(step);
							}
						}
					}
				}
			}
		}

		return null;
	}
}
