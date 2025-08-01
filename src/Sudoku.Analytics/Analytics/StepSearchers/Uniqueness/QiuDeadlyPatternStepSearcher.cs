namespace Sudoku.Analytics.StepSearchers;

/// <summary>
/// Provides with a <b>Qiu's Deadly Pattern</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>
/// Standard types:
/// <list type="bullet">
/// <item>Qiu's Deadly Pattern Type 1</item>
/// <item>Qiu's Deadly Pattern Type 2</item>
/// <item>Qiu's Deadly Pattern Type 3</item>
/// <item>Qiu's Deadly Pattern Type 4</item>
/// <item>Qiu's Deadly Pattern Locked Type</item>
/// </list>
/// </item>
/// <item>
/// External types:
/// <list type="bullet">
/// <item>Qiu's Deadly Pattern External Type 1</item>
/// <item>Qiu's Deadly Pattern External Type 2</item>
/// </list>
/// </item>
/// </list>
/// </summary>
[StepSearcher(
	"StepSearcherName_QiuDeadlyPatternStepSearcher",
	Technique.QiuDeadlyPatternType1, Technique.QiuDeadlyPatternType2,
	Technique.QiuDeadlyPatternType3, Technique.QiuDeadlyPatternType4, Technique.LockedQiuDeadlyPattern,
	Technique.QiuDeadlyPatternExternalType1, Technique.QiuDeadlyPatternExternalType2,
	SupportedSudokuTypes = SudokuType.Standard,
	SupportAnalyzingMultipleSolutionsPuzzle = false)]
public sealed partial class QiuDeadlyPatternStepSearcher : StepSearcher
{
	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		// Handle for case 1.
		foreach (var pattern in QiuDeadlyPattern1Pattern.Patterns)
		{
			if (Collect(ref context, pattern) is { } step)
			{
				return step;
			}
		}

		// Handle for case 2.
		foreach (var pattern in QiuDeadlyPattern2Pattern.Patterns)
		{
			if (Collect(ref context, pattern) is { } step)
			{
				return step;
			}
		}
		return null;
	}

	/// <summary>
	/// <inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/summary"/>
	/// </summary>
	/// <param name="context"><inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/param[@name='context']"/></param>
	/// <param name="pattern">The target pattern.</param>
	/// <returns><inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/returns"/></returns>
	private QiuDeadlyPatternStep? Collect(ref StepAnalysisContext context, QiuDeadlyPattern1Pattern pattern)
	{
		ref readonly var grid = ref context.Grid;

		// We should check for the distinction for the lines.
		var lines = pattern.Lines;
		var l1 = BitOperations.TrailingZeroCount(lines);
		var l2 = lines.GetNextSet(l1);
		var valueCellsInBothLines = CellMap.Empty;
		foreach (var cell in HousesMap[l1] | HousesMap[l2])
		{
			if (grid.GetState(cell) != CellState.Empty)
			{
				valueCellsInBothLines.Add(cell);
			}
		}

		if (pattern.Corner & ~EmptyCells)
		{
			// Corners cannot be non-empty.
			return null;
		}

		var isRow = l1 is >= 9 and < 18;
		if (CheckForBaseType(ref context, grid, pattern, valueCellsInBothLines, isRow) is { } type1Step)
		{
			return type1Step;
		}
		return null;
	}

	/// <summary>
	/// <inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/summary"/>
	/// </summary>
	/// <param name="context"><inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/param[@name='context']"/></param>
	/// <param name="pattern">The target pattern.</param>
	/// <returns><inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/returns"/></returns>
	private QiuDeadlyPatternStep? Collect(ref StepAnalysisContext context, QiuDeadlyPattern2Pattern pattern)
	{
		// TODO: Re-implement later.
		return null;
	}

	/// <summary>
	/// Check for base type.
	/// </summary>
	private QiuDeadlyPatternStep? CheckForBaseType(
		ref StepAnalysisContext context,
		in Grid grid,
		QiuDeadlyPattern1Pattern pattern,
		in CellMap valueCellsInBothLines,
		bool isRow
	)
	{
		var lines = pattern.Lines;
		var l1 = BitOperations.TrailingZeroCount(lines);
		var l2 = lines.GetNextSet(l1);

		// Check whether both two lines are finished.
		if (((HousesMap[l1] | HousesMap[l2]) & ~EmptyCells).Count == 18)
		{
			return null;
		}

		var alignedPosMask = (Mask)0;
		var (l1AlignedMask, l2AlignedMask) = ((Mask)0, (Mask)0);
		for (var pos = 0; pos < 9; pos++)
		{
			if (valueCellsInBothLines.Contains(HousesCells[l1][pos]))
			{
				alignedPosMask |= (Mask)(1 << pos);
				l1AlignedMask |= (Mask)(1 << pos);
			}
			if (valueCellsInBothLines.Contains(HousesCells[l2][pos]))
			{
				alignedPosMask |= (Mask)(1 << pos);
				l2AlignedMask |= (Mask)(1 << pos);
			}
		}
		if (BitOperations.PopCount((Mask)(l1AlignedMask | l2AlignedMask)) > Math.Max(BitOperations.PopCount(l1AlignedMask), BitOperations.PopCount(l2AlignedMask)))
		{
			// Distinction is not 1.
			return null;
		}

		// Check whether the paired cells contain at least 2 empty cells and not in a block.
		var emptyCellsInPairedCells = CellMap.Empty;
		foreach (var pos in alignedPosMask)
		{
			if (HousesCells[l1][pos] is var cell1 && grid.GetState(cell1) == CellState.Empty)
			{
				emptyCellsInPairedCells.Add(cell1);
			}
			if (HousesCells[l2][pos] is var cell2 && grid.GetState(cell2) == CellState.Empty)
			{
				emptyCellsInPairedCells.Add(cell2);
			}
		}
		if (emptyCellsInPairedCells.Count >= 2)
		{
			// Distinction is not 1.
			return null;
		}

		// Check whether the digits contain only 1 digit different.
		var nonEmptyCellsDigitsMaskForLine1 = grid[valueCellsInBothLines & HousesMap[l1]];
		var nonEmptyCellsDigitsMaskForLine2 = grid[valueCellsInBothLines & HousesMap[l2]];
		if (BitOperations.PopCount((Mask)(nonEmptyCellsDigitsMaskForLine1 ^ nonEmptyCellsDigitsMaskForLine2)) >= 2)
		{
			return null;
		}

		// Check whether at least 2 cells is empty in cross-line.
		var crossline = pattern.Crossline;
		if ((crossline & ~EmptyCells).Count >= 3)
		{
			return null;
		}

		// Check whether the number of locked digits appeared in cross-line is at least 2.
		var block = crossline.FirstSharedHouse;
		var allDigitsMaskNotAppearedInCrossline = grid[HousesMap[block] & ~crossline];
		var allDigitsMaskAppearedInCrossline = grid[crossline];
		var cornerLockedDigitsMask = (Mask)(allDigitsMaskAppearedInCrossline & ~allDigitsMaskNotAppearedInCrossline);
		if (BitOperations.PopCount(cornerLockedDigitsMask) < 2)
		{
			return null;
		}

		var corner = pattern.Corner;

		// Check whether corner cells intersect with at least 1 digit. e.g. Digits [1, 2, 3] & [1, 4].
		// This is necessary because we should guarantee the target eliminations should cover at least one digit.
		// For example, digits [1, 2, 3] and [1, 4] intersect with digit [1]. If the cross-line locks the digit [1, 2],
		// then we can conclude that if 1 is set in [1, 4] and 2 is set in [1, 2, 3], the target pattern will form a deadly pattern.
		var cornerDigitsMaskIntersected = grid[corner, false, MaskAggregator.And];
		if (cornerDigitsMaskIntersected == 0)
		{
			return null;
		}

		// Check whether cross-line cells contain the base digits.
		var cornerDigitsMask = grid[corner];
		if ((grid[crossline & ~EmptyCells, true] & cornerDigitsMask) != 0)
		{
			return null;
		}

		// Check whether at least one cross-line has fully covered by value digits.
		// Counter-example (Wrong):
		//   .+9.5+17...51.9+4.+7.....38.+59124+5+7+6+1+839.....+9.+5.96...+5.1773..54.......+93.75.+5.2+78...:411 611 413 417 353 853 173 873 883 687
		var atLeastOneCrosslineIsFullyCoveredByValueDigits = false;
		foreach (var baseCell in pattern.Corner)
		{
			var lineType = BitOperations.TrailingZeroCount(pattern.Lines) < 18 ? HouseType.Row : HouseType.Column;
			var lineTypeTransposed = lineType == HouseType.Row ? HouseType.Column : HouseType.Row;
			var coveredCrossline = PeersMap[baseCell] & HousesMap[baseCell.ToHouse(lineTypeTransposed)] & pattern.Crossline;
			if (!(coveredCrossline & EmptyCells))
			{
				atLeastOneCrosslineIsFullyCoveredByValueDigits = true;
				break;
			}
		}
		if (atLeastOneCrosslineIsFullyCoveredByValueDigits)
		{
			return null;
		}

		// Okay. Now we have a rigorous pattern. Now check for mask and determine its sub-type that can be categorized.

		//
		// Part I - Corner-cell Types (Type 1-4)
		//
		var maskIntersected = cornerLockedDigitsMask & cornerDigitsMaskIntersected;
		if (maskIntersected == cornerDigitsMaskIntersected)
		{
			// One cell only holds those two and the other doesn't only hold them.
			var cornerExtraDigitsMask = (Mask)(cornerDigitsMask & ~cornerDigitsMaskIntersected);
			var cornerContainingExtraDigit = corner;
			var tempMap = CellMap.Empty;
			foreach (var digit in cornerExtraDigitsMask)
			{
				tempMap |= CandidatesMap[digit];
			}
			cornerContainingExtraDigit &= tempMap;

			if (BaseType_Type1(
				ref context, corner, crossline, grid, l1, l2,
				cornerDigitsMaskIntersected, cornerContainingExtraDigit) is { } type1Step)
			{
				return type1Step;
			}

			if (BaseType_Type2(
				ref context, corner, crossline, grid, l1, l2, cornerDigitsMaskIntersected,
				cornerExtraDigitsMask, cornerContainingExtraDigit) is { } type2Step)
			{
				return type2Step;
			}

			if (BaseType_Type3(
				ref context, corner, crossline, grid, l1, l2, cornerDigitsMaskIntersected,
				cornerExtraDigitsMask, cornerContainingExtraDigit) is { } type3Step)
			{
				return type3Step;
			}

			if (BaseType_Type4(ref context, corner, crossline, l1, l2, cornerDigitsMaskIntersected, cornerContainingExtraDigit)
				is { } type4Step)
			{
				return type4Step;
			}
		}
		else if (maskIntersected != 0)
		{
			if (BaseType_TypeLocked(ref context, corner, crossline, grid, l1, l2, cornerLockedDigitsMask) is { } typeLockedStep)
			{
				return typeLockedStep;
			}
		}

		//
		// Part II - Guardian Types (External Type 1-2)
		//
		if ((grid[crossline] & cornerDigitsMask) == cornerDigitsMask)
		{
			// Check whether the number of empty cross-line cells are same as the number of locked digits.
			// Counter-example:
			//   .....+8936+95+31+64+7+8+2.+6+8..+9+5+1+46.9.+1+78+2+5...+8.+6+3+9..+8.+9.5+64..+9.68+1+4+5+3+81+6..+3+27+9...+792+1+6+8:211 711 412 213 713 215 235 251 451 751 452 261 761
			if (BitOperations.PopCount(cornerDigitsMask) > (crossline & EmptyCells).Count)
			{
				return null;
			}

			// Now check for subtypes.
			var mirror = pattern.Mirror;
			if (BaseType_ExternalType1(ref context, corner, crossline, mirror, grid, l1, l2, cornerDigitsMask) is { } externalType1Step)
			{
				return externalType1Step;
			}

			if (BaseType_ExternalType2(ref context, corner, crossline, mirror, grid, l1, l2, cornerDigitsMask) is { } externalType2Step)
			{
				return externalType2Step;
			}
		}

		// No valid steps found. Return null.
		return null;
	}

	/// <summary>
	/// Check for base type (type 1).
	/// </summary>
	/// <param name="context"><inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/param[@name='context']"/></param>
	/// <param name="corner">Indicates the corner cells used in a pattern.</param>
	/// <param name="crossline">Indicates the cross-line cells used in a pattern.</param>
	/// <param name="grid">The grid used.</param>
	/// <param name="l1">The line 1.</param>
	/// <param name="l2">The line 2.</param>
	/// <param name="digitsMaskAppearedInCorner">A mask value that holds a list of digits appeared in corner cells.</param>
	/// <param name="cornerContainingExtraDigit">Indicates the cells which are corner cells and contain extra digit.</param>
	/// <returns><inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/returns"/></returns>
	private QiuDeadlyPatternType1Step? BaseType_Type1(
		ref StepAnalysisContext context,
		in CellMap corner,
		in CellMap crossline,
		in Grid grid,
		House l1,
		House l2,
		Mask digitsMaskAppearedInCorner,
		in CellMap cornerContainingExtraDigit
	)
	{
		if (cornerContainingExtraDigit is not [var targetCell])
		{
			return null;
		}

		var elimDigitsMask = (Mask)(grid.GetCandidates(targetCell) & digitsMaskAppearedInCorner);
		var candidateOffsets = new List<CandidateViewNode>();
		foreach (var digit in digitsMaskAppearedInCorner)
		{
			foreach (var cell in CandidatesMap[digit] & crossline)
			{
				candidateOffsets.Add(new(ColorIdentifier.Auxiliary2, cell * 9 + digit));
			}
		}

		var theOtherCornerCellNoElimination = (corner - targetCell)[0];
		foreach (var digit in grid.GetCandidates(theOtherCornerCellNoElimination))
		{
			candidateOffsets.Add(new(ColorIdentifier.Normal, theOtherCornerCellNoElimination * 9 + digit));
		}

		var step = new QiuDeadlyPatternType1Step(
			(from digit in elimDigitsMask select new Conclusion(Elimination, targetCell, digit)).ToArray(),
			[
				[
					.. candidateOffsets,
					new HouseViewNode(ColorIdentifier.Normal, l1),
					new HouseViewNode(ColorIdentifier.Normal, l2),
					new CellViewNode(ColorIdentifier.Normal, corner[0]),
					new CellViewNode(ColorIdentifier.Normal, corner[1])
				]
			],
			context.Options,
			true,
			1 << l1 | 1 << l2,
			corner[0],
			corner[1],
			targetCell,
			elimDigitsMask
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;
	}

	/// <summary>
	/// Check for base type (type 2).
	/// </summary>
	/// <param name="context"><inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/param[@name='context']"/></param>
	/// <param name="corner">Indicates the corner cells used in a pattern.</param>
	/// <param name="crossline">Indicates the cross-line cells used in a pattern.</param>
	/// <param name="grid">The grid used.</param>
	/// <param name="l1">The line 1.</param>
	/// <param name="l2">The line 2.</param>
	/// <param name="digitsMaskAppearedInCorner">A mask value that holds a list of digits appeared in corner cells.</param>
	/// <param name="extraDigitsMask">Indicates a mask value that holds extra digits.</param>
	/// <param name="cornerContainingExtraDigit">Indicates the cells which are corner cells and contain extra digit.</param>
	/// <returns><inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/returns"/></returns>
	private QiuDeadlyPatternType2Step? BaseType_Type2(
		ref StepAnalysisContext context,
		in CellMap corner,
		in CellMap crossline,
		in Grid grid,
		House l1,
		House l2,
		Mask digitsMaskAppearedInCorner,
		Mask extraDigitsMask,
		in CellMap cornerContainingExtraDigit
	)
	{
		if (!BitOperations.IsPow2(extraDigitsMask))
		{
			return null;
		}

		var extraDigit = BitOperations.Log2((uint)extraDigitsMask);
		var elimMap = corner % CandidatesMap[extraDigit];
		if (!elimMap)
		{
			return null;
		}

		var candidateOffsets = new List<CandidateViewNode>();
		foreach (var cell in cornerContainingExtraDigit)
		{
			foreach (var digit in grid.GetCandidates(cell))
			{
				candidateOffsets.Add(
					new(
						digit == extraDigit ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal,
						cell * 9 + digit
					)
				);
			}
		}
		foreach (var digit in digitsMaskAppearedInCorner)
		{
			foreach (var cell in CandidatesMap[digit] & crossline)
			{
				candidateOffsets.Add(new(ColorIdentifier.Auxiliary2, cell * 9 + digit));
			}
		}

		var step = new QiuDeadlyPatternType2Step(
			(from cell in elimMap select new Conclusion(Elimination, cell, extraDigit)).ToArray(),
			[
				[
					.. candidateOffsets,
					new HouseViewNode(ColorIdentifier.Normal, l1),
					new HouseViewNode(ColorIdentifier.Normal, l2),
					new CellViewNode(ColorIdentifier.Normal, corner[0]),
					new CellViewNode(ColorIdentifier.Normal, corner[1])
				]
			],
			context.Options,
			true,
			1 << l1 | 1 << l2,
			corner[0],
			corner[1],
			extraDigit
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;
	}

	/// <summary>
	/// Check for base type (type 3).
	/// </summary>
	/// <param name="context"><inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/param[@name='context']"/></param>
	/// <param name="corner">Indicates the corner cells used in a pattern.</param>
	/// <param name="crossline">Indicates the cross-line cells used in a pattern.</param>
	/// <param name="grid">The grid used.</param>
	/// <param name="l1">The line 1.</param>
	/// <param name="l2">The line 2.</param>
	/// <param name="digitsMaskAppearedInCorner">A mask value that holds a list of digits appeared in corner cells.</param>
	/// <param name="cornerContainingExtraDigit">Indicates the cells which are corner cells and contain extra digit.</param>
	/// <param name="extraDigitsMask">Indicates a mask value that holds extra digits.</param>
	/// <returns><inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/returns"/></returns>
	private QiuDeadlyPatternType3Step? BaseType_Type3(
		ref StepAnalysisContext context,
		in CellMap corner,
		in CellMap crossline,
		in Grid grid,
		House l1,
		House l2,
		Mask digitsMaskAppearedInCorner,
		Mask extraDigitsMask,
		in CellMap cornerContainingExtraDigit
	)
	{
		if (BitOperations.IsPow2(digitsMaskAppearedInCorner))
		{
			return null;
		}

		if (BitOperations.IsPow2(extraDigitsMask))
		{
			return null;
		}

		if (cornerContainingExtraDigit != corner)
		{
			return null;
		}

		var size = BitOperations.PopCount(digitsMaskAppearedInCorner);
		foreach (var cornerCellCoveredHouse in corner.SharedHouses)
		{
			var emptyCellsInCurrentHouse = HousesMap[cornerCellCoveredHouse] & EmptyCells;
			var availableCellsToBeIterated = emptyCellsInCurrentHouse & ~corner;
			if (availableCellsToBeIterated.Count - 1 < size)
			{
				// No more cells or eliminations to exist.
				continue;
			}

			foreach (ref readonly var extraCells in emptyCellsInCurrentHouse & size)
			{
				var currentDigitsMask = grid[extraCells];
				if (currentDigitsMask != extraDigitsMask)
				{
					continue;
				}

				var elimMap = emptyCellsInCurrentHouse & ~extraCells & ~corner;
				var conclusions = new List<Conclusion>();
				foreach (var digit in currentDigitsMask)
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
				foreach (var cell in corner)
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						candidateOffsets.Add(
							new(
								(extraDigitsMask >> digit & 1) != 0 ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal,
								cell * 9 + digit
							)
						);
					}
				}
				foreach (var d in digitsMaskAppearedInCorner)
				{
					foreach (var cell in CandidatesMap[d] & crossline)
					{
						candidateOffsets.Add(new(ColorIdentifier.Auxiliary2, cell * 9 + d));
					}
				}
				foreach (var cell in extraCells)
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						candidateOffsets.Add(new(ColorIdentifier.Auxiliary3, cell * 9 + digit));
					}
				}

				var step = new QiuDeadlyPatternType3Step(
					conclusions.AsMemory(),
					[
						[
							.. candidateOffsets,
							new HouseViewNode(ColorIdentifier.Normal, l1),
							new HouseViewNode(ColorIdentifier.Normal, l2),
							new HouseViewNode(ColorIdentifier.Auxiliary1, cornerCellCoveredHouse),
							new CellViewNode(ColorIdentifier.Normal, corner[0]),
							new CellViewNode(ColorIdentifier.Normal, corner[1])
						]
					],
					context.Options,
					true,
					1 << l1 | 1 << l2,
					corner[0],
					corner[1],
					extraCells,
					extraDigitsMask,
					true
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
	/// Check for base type (type 4).
	/// </summary>
	/// <param name="context"><inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/param[@name='context']"/></param>
	/// <param name="corner">Indicates the corner cells used in a pattern.</param>
	/// <param name="crossline">Indicates the cross-line cells used in a pattern.</param>
	/// <param name="l1">The line 1.</param>
	/// <param name="l2">The line 2.</param>
	/// <param name="digitsMaskAppearedInCorner">A mask value that holds a list of digits appeared in corner cells.</param>
	/// <param name="cornerContainingExtraDigit">Indicates the cells which are corner cells and contain extra digit.</param>
	/// <returns><inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/returns"/></returns>
	private QiuDeadlyPatternType4Step? BaseType_Type4(
		ref StepAnalysisContext context,
		in CellMap corner,
		in CellMap crossline,
		House l1,
		House l2,
		Mask digitsMaskAppearedInCorner,
		in CellMap cornerContainingExtraDigit
	)
	{
		foreach (var digit in digitsMaskAppearedInCorner)
		{
			foreach (var cornerCellCoveredHouse in (corner & CandidatesMap[digit]).SharedHouses)
			{
				if ((CandidatesMap[digit] & HousesMap[cornerCellCoveredHouse]) == corner)
				{
					var conclusions = new List<Conclusion>();
					foreach (var elimDigit in (Mask)(digitsMaskAppearedInCorner & ~(1 << digit)))
					{
						foreach (var cell in CandidatesMap[elimDigit] & corner)
						{
							conclusions.Add(new(Elimination, cell, elimDigit));
						}
					}
					if (conclusions.Count == 0)
					{
						continue;
					}

					var candidateOffsets = new List<CandidateViewNode>();
					foreach (var d in digitsMaskAppearedInCorner)
					{
						foreach (var cell in CandidatesMap[d] & crossline)
						{
							candidateOffsets.Add(new(ColorIdentifier.Auxiliary2, cell * 9 + d));
						}
					}
					foreach (var cell in corner)
					{
						candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, cell * 9 + digit));
					}

					var step = new QiuDeadlyPatternType4Step(
						conclusions.AsMemory(),
						[
							[
								.. candidateOffsets,
								new HouseViewNode(ColorIdentifier.Normal, l1),
								new HouseViewNode(ColorIdentifier.Normal, l2),
								new ConjugateLinkViewNode(ColorIdentifier.Auxiliary1, corner[0], corner[1], digit),
								new CellViewNode(ColorIdentifier.Normal, corner[0]),
								new CellViewNode(ColorIdentifier.Normal, corner[1])
							]
						],
						context.Options,
						true,
						1 << l1 | 1 << l2,
						corner[0],
						corner[1],
						new(corner, digit)
					);
					if (context.OnlyFindOne)
					{
						return step;
					}

					context.Accumulator.Add(step);
				}
			}
		}

		return null;
	}

	/// <summary>
	/// Check for base type (locked type).
	/// </summary>
	/// <param name="context"><inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/param[@name='context']"/></param>
	/// <param name="corner">Indicates the corner cells used in a pattern.</param>
	/// <param name="crossline">Indicates the cross-line cells used in a pattern.</param>
	/// <param name="grid">The grid used.</param>
	/// <param name="l1">The line 1.</param>
	/// <param name="l2">The line 2.</param>
	/// <param name="cornerLockedDigitsMask">A mask value that holds digits locked in cross-line cells.</param>
	/// <returns><inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/returns"/></returns>
	private QiuDeadlyPatternLockedTypeStep? BaseType_TypeLocked(
		ref StepAnalysisContext context,
		in CellMap corner,
		in CellMap crossline,
		in Grid grid,
		House l1,
		House l2,
		Mask cornerLockedDigitsMask
	)
	{
		var currentDigitsMask = grid[corner];
		if (BitOperations.PopCount(currentDigitsMask) > 4)
		{
			// Corner cells hold at least 5 digits, which is disallowed in the pattern.
			return null;
		}

		var digitsShouldBeLocked = (Mask)(cornerLockedDigitsMask & currentDigitsMask);
		var currentDigitsNotLocked = (Mask)(currentDigitsMask & ~digitsShouldBeLocked);
		if (!BitOperations.IsPow2(currentDigitsNotLocked))
		{
			return null;
		}

		var elimDigit = BitOperations.Log2((uint)currentDigitsNotLocked);
		var elimMap = crossline & CandidatesMap[elimDigit];
		if (!elimMap)
		{
			return null;
		}

		var candidateOffsets = new List<CandidateViewNode>();
		foreach (var digit in digitsShouldBeLocked)
		{
			foreach (var cell in CandidatesMap[digit] & crossline)
			{
				candidateOffsets.Add(new(ColorIdentifier.Auxiliary2, cell * 9 + digit));
			}
		}
		foreach (var cell in corner)
		{
			foreach (var digit in grid.GetCandidates(cell))
			{
				candidateOffsets.Add(
					new(
						(digitsShouldBeLocked >> digit & 1) != 0 ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal,
						cell * 9 + digit
					)
				);
			}
		}

		var house = crossline.FirstSharedHouse;
		var lockedMap = CandidateMap.Empty;
		foreach (var digit in currentDigitsMask)
		{
			foreach (var cell in HousesMap[house] & CandidatesMap[digit])
			{
				lockedMap.Add(cell * 9 + digit);
			}
		}

		var step = new QiuDeadlyPatternLockedTypeStep(
			(from cell in elimMap select new Conclusion(Elimination, cell, elimDigit)).ToArray(),
			[
				[
					.. candidateOffsets,
					new HouseViewNode(ColorIdentifier.Normal, l1),
					new HouseViewNode(ColorIdentifier.Normal, l2),
					new HouseViewNode(ColorIdentifier.Auxiliary1, house),
					new CellViewNode(ColorIdentifier.Normal, corner[0]),
					new CellViewNode(ColorIdentifier.Normal, corner[1])
				]
			],
			context.Options,
			true,
			1 << l1 | 1 << l2,
			corner[0],
			corner[1],
			lockedMap
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;
	}

	/// <summary>
	/// Check for base type (external type 1).
	/// </summary>
	/// <param name="context"><inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/param[@name='context']"/></param>
	/// <param name="corner">Indicates the corner cells used in a pattern.</param>
	/// <param name="crossline">Indicates the cross-line cells used in a pattern.</param>
	/// <param name="mirror">Indicates the mirror cells used in a pattern.</param>
	/// <param name="grid">The grid used.</param>
	/// <param name="l1">The line 1.</param>
	/// <param name="l2">The line 2.</param>
	/// <param name="externalDigitsMaskToBeChecked">A mask value that holds digits should be checked.</param>
	/// <returns><inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/returns"/></returns>
	private QiuDeadlyPatternExternalType1Step? BaseType_ExternalType1(
		ref StepAnalysisContext context,
		in CellMap corner,
		in CellMap crossline,
		in CellMap mirror,
		in Grid grid,
		House l1,
		House l2,
		Mask externalDigitsMaskToBeChecked
	)
	{
		if ((mirror & EmptyCells) is not [var targetCell])
		{
			return null;
		}

		// Check whether the block where cross-line cells is lying,
		// contains empty cells if it is not categorized as cross-line or the target cell (where eliminations are raised).
		// Counter-example:
		//   +67+13..9..+3.+85..+61.52.+1.6+38.8+6......+1.35+61.72.+1.......6+28.7..+163+71+3.+62....+56.+31+27.:425 426 443 943 245 446 948 956 459 463 963 864 964 265 965 466 866 966 467 476
		var cellsShouldNotBeEmpty = (HousesMap[BitOperations.Log2((uint)crossline.BlockMask)] & ~crossline & ~mirror) - targetCell;
		if (cellsShouldNotBeEmpty & EmptyCells)
		{
			return null;
		}

		var elimDigits = (Mask)(grid.GetCandidates(targetCell) & ~externalDigitsMaskToBeChecked);
		if (elimDigits == 0)
		{
			return null;
		}

		var candidateOffsets = new List<CandidateViewNode>();
		foreach (var cell in crossline & EmptyCells)
		{
			foreach (var digit in (Mask)(grid.GetCandidates(cell) & externalDigitsMaskToBeChecked))
			{
				candidateOffsets.Add(new(ColorIdentifier.Auxiliary2, cell * 9 + digit));
			}
		}
		foreach (var cell in corner)
		{
			foreach (var digit in grid.GetCandidates(cell))
			{
				candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + digit));
			}
		}

		var step = new QiuDeadlyPatternExternalType1Step(
			(from digit in elimDigits select new Conclusion(Elimination, targetCell, digit)).ToArray(),
			[
				[
					.. candidateOffsets,
					new HouseViewNode(ColorIdentifier.Normal, l1),
					new HouseViewNode(ColorIdentifier.Normal, l2),
					new CellViewNode(ColorIdentifier.Normal, corner[0]),
					new CellViewNode(ColorIdentifier.Normal, corner[1])
				]
			],
			context.Options,
			true,
			1 << l1 | 1 << l2,
			corner[0],
			corner[1],
			targetCell,
			externalDigitsMaskToBeChecked
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;
	}

	/// <summary>
	/// Check for base type (external type 2).
	/// </summary>
	/// <param name="context"><inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/param[@name='context']"/></param>
	/// <param name="corner">Indicates the corner cells used in a pattern.</param>
	/// <param name="crossline">Indicates the cross-line cells used in a pattern.</param>
	/// <param name="mirror">Indicates the mirror cells used in a pattern.</param>
	/// <param name="grid">The grid used.</param>
	/// <param name="l1">The line 1.</param>
	/// <param name="l2">The line 2.</param>
	/// <param name="externalDigitsMaskToBeChecked">A mask value that holds digits should be checked.</param>
	/// <returns><inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/returns"/></returns>
	private QiuDeadlyPatternExternalType2Step? BaseType_ExternalType2(
		ref StepAnalysisContext context,
		in CellMap corner,
		in CellMap crossline,
		in CellMap mirror,
		in Grid grid,
		House l1,
		House l2,
		Mask externalDigitsMaskToBeChecked
	)
	{
		var elimDigits = (Mask)(grid[mirror] & externalDigitsMaskToBeChecked);
		if (!BitOperations.IsPow2(elimDigits))
		{
			return null;
		}

		var elimDigit = BitOperations.Log2((uint)elimDigits);
		var range = HousesMap[BitOperations.TrailingZeroCount(mirror.BlockMask)] & EmptyCells & ~crossline;
		var truth = range & CandidatesMap[elimDigit];
		var elimMap = range % CandidatesMap[elimDigit];
		if (!elimMap)
		{
			return null;
		}

		// Check whether the block where cross-line cells is lying,
		// contains empty cells if it is not categorized as cross-line or the target cell (where eliminations are raised).
		var cellsShouldNotBeEmpty = HousesMap[BitOperations.Log2((uint)crossline.BlockMask)] & ~crossline & ~mirror & ~truth;
		if (cellsShouldNotBeEmpty & EmptyCells)
		{
			return null;
		}

		// Check whether other digits cannot be eliminated are locked in the cross-line cells.
		// Counter-example:
		//   .+5243+19+6..3+67...1519.....2.2.......694.......+5+6....74+93.9......6.+5.1.....8.5..69.:825 826 833 837 839 844 254 854 255 256 864 477 877 779 789 495 496 499 799
		var allOtherDigitsAreLockedInCrosslineCells = true;
		var crosslineBlock = HousesMap[BitOperations.Log2((uint)crossline.BlockMask)] & EmptyCells;
		foreach (var digit in (Mask)(externalDigitsMaskToBeChecked & ~(1 << elimDigit)))
		{
			if (crosslineBlock & CandidatesMap[digit] & ~crossline)
			{
				allOtherDigitsAreLockedInCrosslineCells = false;
				break;
			}
		}
		if (!allOtherDigitsAreLockedInCrosslineCells)
		{
			return null;
		}

		var candidateOffsets = new List<CandidateViewNode>();
		foreach (var cell in crossline & EmptyCells)
		{
			foreach (var digit in (Mask)(grid.GetCandidates(cell) & externalDigitsMaskToBeChecked))
			{
				candidateOffsets.Add(new(ColorIdentifier.Auxiliary2, cell * 9 + digit));
			}
		}
		foreach (var cell in corner)
		{
			foreach (var digit in grid.GetCandidates(cell))
			{
				candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + digit));
			}
		}
		foreach (var cell in truth)
		{
			candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, cell * 9 + elimDigit));
		}

		var step = new QiuDeadlyPatternExternalType2Step(
			(from cell in elimMap select new Conclusion(Elimination, cell, elimDigit)).ToArray(),
			[
				[
					.. candidateOffsets,
					new HouseViewNode(ColorIdentifier.Normal, l1),
					new HouseViewNode(ColorIdentifier.Normal, l2),
					new CellViewNode(ColorIdentifier.Normal, corner[0]),
					new CellViewNode(ColorIdentifier.Normal, corner[1])
				]
			],
			context.Options,
			true,
			1 << l1 | 1 << l2,
			corner[0],
			corner[1],
			mirror,
			elimDigit
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;
	}
}
