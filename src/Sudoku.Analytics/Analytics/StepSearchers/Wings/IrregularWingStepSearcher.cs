namespace Sudoku.Analytics.StepSearchers;

/// <summary>
/// Provides with an <b>Irregular Wing</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>
/// W-Wing (George Woods' Wing) family:
/// <list type="bullet">
/// <item>W-Wing</item>
/// <item>Grouped W-Wing</item>
/// <item>Multiple Branch W-Wing</item>
/// </list>
/// </item>
/// <item>
/// M-Wing (Medusa Wing) family:
/// <list type="bullet">
/// <item>M-Wing</item>
/// <item>Grouped M-Wing</item>
/// </list>
/// </item>
/// </list>
/// </summary>
[StepSearcher(
	"StepSearcherName_IrregularWingStepSearcher",
	Technique.WWing, Technique.GroupedWWing, Technique.MultiBranchWWing)]
public sealed partial class IrregularWingStepSearcher : StepSearcher
{
	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		if (Collect_WWing(ref context) is { } w)
		{
			return w;
		}
		if (Collect_MultiBranchWWing(ref context) is { } w2)
		{
			return w2;
		}
		return null;
	}

	/// <inheritdoc cref="Collect(ref StepAnalysisContext)"/>
	/// <remarks>
	/// <include file="../../global-doc-comments.xml" path="g/developer-notes"/>
	/// A valid pattern of W-Wing is <c><![CDATA[(x=y)-y=y-(y=x)]]></c>, symmetric.
	/// </remarks>
	/// <example>
	/// TODO: Add loop logic. Example:
	/// <code><![CDATA[
	/// +39+4+7.6...+2+851..3+7+616+7+3.2...8+7.+6.34..6+4..+7+5+8+31..34.+8.+67...8...23..8..76.....5...8.:435 935 567 175 185 192 193 993 195 495 995
	/// ]]></code>
	/// </example>
	private WWingStep? Collect_WWing(ref StepAnalysisContext context)
	{
		// The grid with possible W-Wing pattern should contain at least two empty cells (start and end cell).
		if (BivalueCells.Count < 2)
		{
			return null;
		}

		// Iterate on each cells.
		ref readonly var grid = ref context.Grid;
		for (var c1 = 0; c1 < 72; c1++)
		{
			if (!BivalueCells.Contains(c1))
			{
				// The cell isn't a bi-value cell.
				continue;
			}

			// Iterate on each cells which are not peers in 'c1'.
			var digits = grid.GetCandidates(c1).AllSets;
			foreach (var c2 in BivalueCells & ~(PeersMap[c1] + c1))
			{
				if (c2 < c1)
				{
					// To avoid duplicate structures found.
					continue;
				}

				if (grid.GetCandidates(c1) != grid.GetCandidates(c2))
				{
					// Two cells may contain different kinds of digits.
					continue;
				}

				var intersection = PeersMap[c1] & PeersMap[c2];
				if (!(EmptyCells & intersection))
				{
					// The pattern doesn't contain any possible eliminations.
					continue;
				}

				// Iterate on each house.
				for (var house = 0; house < 27; house++)
				{
					if (house == c1.ToHouse(HouseType.Block)
						|| house == c1.ToHouse(HouseType.Row)
						|| house == c1.ToHouse(HouseType.Column)
						|| house == c2.ToHouse(HouseType.Block)
						|| house == c2.ToHouse(HouseType.Row)
						|| house == c2.ToHouse(HouseType.Column))
					{
						// The house to search for conjugate pairs shouldn't be the same as those two cells' houses.
						continue;
					}

					// Iterate on each digit to search for the conjugate pair.
					foreach (var digit in digits)
					{
						var bridge = CandidatesMap[digit] & HousesMap[house];
						var isPassed = bridge switch
						{
							[var a, var b]
								when (c1.AsCellMap() + a).FirstSharedHouse != FallbackConstants.@int
								&& (c2.AsCellMap() + b).FirstSharedHouse != FallbackConstants.@int
								|| (c1.AsCellMap() + b).FirstSharedHouse != FallbackConstants.@int
								&& (c2.AsCellMap() + a).FirstSharedHouse != FallbackConstants.@int
								=> true,
							{ Count: > 2 and <= 6, BlockMask: var blocks } => PopCount((uint)blocks) switch
							{
								1 => ((PeersMap[c1] | PeersMap[c2]) & bridge) == bridge,
								2 when TrailingZeroCount(blocks) is var block1 && blocks.GetNextSet(block1) is var block2
									=> (HousesMap[block1] & bridge, HousesMap[block2] & bridge) switch
									{
										var (bridgeInBlock1, bridgeInBlock2) => (
											(PeersMap[c1] & bridgeInBlock1) == bridgeInBlock1,
											(PeersMap[c2] & bridgeInBlock2) == bridgeInBlock2,
											(PeersMap[c1] & bridgeInBlock2) == bridgeInBlock2,
											(PeersMap[c2] & bridgeInBlock1) == bridgeInBlock1
										) switch
										{
											(true, true, _, _) => true,
											(_, _, true, true) => true,
											_ => false
										}
									},
								_ => false
							},
							_ => false
						};
						if (!isPassed)
						{
							continue;
						}

						// Check for eliminations.
						var anotherDigit = TrailingZeroCount((Mask)(grid.GetCandidates(c1) & ~(1 << digit)));
						var elimMap = CandidatesMap[anotherDigit] & (c1.AsCellMap() + c2).PeerIntersection;
						if (!elimMap)
						{
							// No possible eliminations found.
							continue;
						}

						// Now W-Wing found. Store it into the accumulator.
						var step = new WWingStep(
							(from cell in elimMap select new Conclusion(Elimination, cell, anotherDigit)).ToArray(),
							[
								[
									new CandidateViewNode(ColorIdentifier.Auxiliary1, c1 * 9 + anotherDigit),
									new CandidateViewNode(ColorIdentifier.Auxiliary1, c2 * 9 + anotherDigit),
									new CandidateViewNode(ColorIdentifier.Normal, c1 * 9 + digit),
									new CandidateViewNode(ColorIdentifier.Normal, c2 * 9 + digit),
									.. from cell in bridge select new CandidateViewNode(ColorIdentifier.Normal, cell * 9 + digit),
									new HouseViewNode(ColorIdentifier.Normal, house)
								]
							],
							context.Options,
							c1,
							c2,
							bridge,
							(Mask)(1 << digit | 1 << anotherDigit)
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

		return null;
	}

	/// <inheritdoc cref="Collect(ref StepAnalysisContext)"/>
	private MultiBranchWWingStep? Collect_MultiBranchWWing(ref StepAnalysisContext context)
	{
		// Iterates on each digit.
		ref readonly var grid = ref context.Grid;
		for (var digit1 = 0; digit1 < 8; digit1++)
		{
			for (var digit2 = digit1 + 1; digit2 < 9; digit2++)
			{
				// Checks for bi-value cells that only holds two different digits.
				var possibleCells = BivalueCells & CandidatesMap[digit1] & CandidatesMap[digit2];
				if (possibleCells.Count < 3)
				{
					continue;
				}

				// Iterates on sizes.
				// Please note that cases whose cardinality (i.e. size of the subset) is greater than 5
				// is extremely rare to appear. Therefore we can just skip on such cases,
				// although the limit will miss some rare cases.
				for (var size = 3; size <= Math.Min(possibleCells.Count, 5); size++)
				{
					// Iterates on each combination.
					foreach (ref readonly var cells in possibleCells & size)
					{
						// Checks whether they can intersect to at least one cell, as the elimination cell.
						if (cells.PeerIntersection is not (var elimMap and not []))
						{
							continue;
						}

						// Iterates on each digit, to fix the digit as the X digit.
						// W-Wing pattern requires a pair of W digits as the first and the last place,
						// and requires the X digits are in the body of the pattern (i.e. non-terminal nodes).
						foreach (var xDigit in (digit1, digit2))
						{
							// Gets the target house that can place root cells.
							foreach (var house in (HouseMaskOperations.AllRowsMask | HouseMaskOperations.AllColumnsMask) & ~cells.Houses)
							{
								var crosshatchingHouseType = house >= 18 ? HouseType.Row : HouseType.Column;
								var emptyCellsInThisHouse = HousesMap[house] & CandidatesMap[xDigit];
								if (emptyCellsInThisHouse.Count < size)
								{
									continue;
								}

								var tempCrosshatchingHouses = CellMap.Empty;
								foreach (var cell in cells)
								{
									tempCrosshatchingHouses |= HousesMap[cell.ToHouse(crosshatchingHouseType)];
								}
								emptyCellsInThisHouse &= tempCrosshatchingHouses;
								if (emptyCellsInThisHouse.Count != size)
								{
									continue;
								}

								if ((HousesMap[house] & CandidatesMap[xDigit]) != emptyCellsInThisHouse)
								{
									// This house contains other unused empty cells
									// that can also be filled with digit X.
									continue;
								}

								var wDigit = xDigit == digit1 ? digit2 : digit1;
								var conclusions = new List<Conclusion>(elimMap.Count);
								foreach (var cell in elimMap)
								{
									if (CandidatesMap[wDigit].Contains(cell))
									{
										conclusions.Add(new(Elimination, cell, wDigit));
									}
								}
								if (conclusions.Count == 0)
								{
									continue;
								}

								var candidateOffsets = new List<CandidateViewNode>();
								foreach (var cell in cells)
								{
									foreach (var digit in grid.GetCandidates(cell))
									{
										candidateOffsets.Add(
											new(
												digit == wDigit ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal,
												cell * 9 + digit
											)
										);
									}
								}
								foreach (var cell in emptyCellsInThisHouse)
								{
									candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + xDigit));
								}

								var step = new MultiBranchWWingStep(
									conclusions.AsMemory(),
									[[.. candidateOffsets, new HouseViewNode(ColorIdentifier.Normal, house)]],
									context.Options,
									cells,
									emptyCellsInThisHouse,
									emptyCellsInThisHouse.SharedLine,
									(Mask)(1 << xDigit | 1 << wDigit)
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
