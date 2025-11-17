namespace Sudoku.Theories.BabaGroupingTheory.SpreadingRules;

/// <summary>
/// Defines hidden single spreading rule.
/// </summary>
public sealed class HiddenSingleSpreadingRule : SpreadingRule
{
	/// <inheritdoc/>
	public override void Spread(Candidate candidate, ref CellMap cells, ref readonly Grid grid)
	{
		var playground = grid;
		playground.SetDigit(candidate / 9, candidate % 9);

		var digit = candidate % 9;
		var cell = candidate / 9;
		var lastCells = grid.CandidatesMap[digit];
		foreach (var house in lastCells.Houses)
		{
			if (HousesMap[house].Contains(cell))
			{
				continue;
			}

			var count = 0;
			var targetCell = -1;
			foreach (var c in HousesMap[house])
			{
				if (playground.GetState(c) == CellState.Empty && (playground.GetCandidates(c) >> digit & 1) != 0)
				{
					targetCell = c;
					if (++count >= 2)
					{
						break;
					}
				}
			}
			if (count == 1)
			{
				cells += targetCell;
			}
		}
	}
}
