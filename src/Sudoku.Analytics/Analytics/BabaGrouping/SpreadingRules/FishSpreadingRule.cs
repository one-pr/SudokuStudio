namespace Sudoku.Analytics.BabaGrouping.SpreadingRules;

/// <summary>
/// Defines fish spreading rule.
/// </summary>
public sealed class FishSpreadingRule : SpreadingRule
{
	/// <inheritdoc/>
	public override void Spread(Candidate candidate, ref CellMap cells, ref readonly Grid grid)
	{
		var nestedSpreadingRule = new HiddenSingleSpreadingRule();
		var playground = grid;
		var digit = candidate % 9;
		var cell = candidate / 9;
		playground.SetDigit(cell, digit);

		var lastCells = grid.CandidatesMap[digit];
		var rows = (lastCells.RowMask << 9).AllSets;
		var columns = (lastCells.ColumnMask << 18).AllSets;

		// Iterate on size.
		for (var size = 2; size <= 4; size++)
		{
			search(rows, ref playground, ref cells, true);
			search(columns, ref playground, ref cells, false);


			void search(ReadOnlySpan<House> houses, ref Grid playground, ref CellMap cells, bool isRow)
			{
				// Iterate on each combination of houses.
				foreach (var houseCombination in houses & size)
				{
					var patternCells = CellMap.Empty;
					var mask = (Mask)0;
					foreach (var house in houseCombination)
					{
						var cellsCurrentHouse = HousesMap[house] & lastCells;
						mask |= isRow ? cellsCurrentHouse.ColumnMask : cellsCurrentHouse.RowMask;
						patternCells |= cellsCurrentHouse;
					}
					if (BitOperations.PopCount((uint)mask) != size)
					{
						continue;
					}

					// Fish found.
					var conclusions = CellMap.Empty;
					foreach (var house in mask << (isRow ? 18 : 9))
					{
						conclusions |= lastCells & HousesMap[house] & ~patternCells;
					}

					// Remove candidate.
					foreach (var c in conclusions)
					{
						playground[c] &= (Mask)~(1 << digit);
					}

					nestedSpreadingRule.Spread(candidate, ref cells, in playground);
				}
			}
		}
	}
}
