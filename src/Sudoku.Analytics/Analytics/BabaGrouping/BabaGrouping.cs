namespace Sudoku.Analytics.BabaGrouping;

/// <summary>
/// Provides a way to suppose values.
/// </summary>
public static class BabaGrouping
{
	/// <summary>
	/// Spreads the specified assumption into multiple different cells.
	/// </summary>
	/// <param name="symbol">The original symbol.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="spreadingRules">The spreading rules.</param>
	/// <returns>All found symbols found.</returns>
	/// <exception cref="ArgumentException">Throws when an assumption is not fuzzy type.</exception>
	public static ReadOnlySpan<CellSymbol> Spread(CellSymbol symbol, in Grid grid, ReadOnlySpan<SpreadingRule> spreadingRules)
	{
		var value = symbol.FirstValue;
		ArgumentException.Assert(value != CellSymbolValue.Invalid && value.Type == CellSymbolType.Fuzzy);

		var startCell = symbol.Cell;

		var resultCells = CellMap.Empty;
		var playground = grid;
		dfs(startCell, playground, in grid, ref resultCells, spreadingRules);

		// Return value.
		var result = new List<CellSymbol>();
		foreach (var cell in resultCells)
		{
			result.Add(new(cell, [value]));
		}
		return result.AsSpan();


		void dfs(
			Cell startCell,
			Grid playground,
			ref readonly Grid originalGrid,
			ref CellMap resultCells,
			ReadOnlySpan<SpreadingRule> spreadingRules
		)
		{
			// Enumerate all possible candidates, to know whether they can reach the target cell.
			var spreadingCells = new Dictionary<Digit, CellMap>();
			foreach (var digit in playground.GetCandidates(startCell))
			{
				var startCandidate = startCell * 9 + digit;

				// Spread candidate.
				var cells = CellMap.Empty;
				foreach (var spreadingRule in spreadingRules)
				{
					spreadingRule.Spread(startCandidate, ref cells, in playground);
				}

				// Add it into the collection.
				spreadingCells.Add(digit, cells);
			}

			// Find for intersection.
			var resultCellsCurrentRound = CellMap.Empty;
			var isFirst = true;
			foreach (var digit in spreadingCells.Keys)
			{
				if (isFirst)
				{
					isFirst = false;
					resultCellsCurrentRound |= spreadingCells[digit];
				}
				else
				{
					resultCellsCurrentRound &= spreadingCells[digit];
				}
			}
			if (resultCellsCurrentRound)
			{
				resultCells |= resultCellsCurrentRound;

				// Update candidates.
				foreach (var cell in resultCellsCurrentRound)
				{
					playground.SetCandidates(cell, originalGrid.GetCandidates(symbol.Cell));
				}

				// Perform DFS.
				foreach (var cell in resultCellsCurrentRound)
				{
					dfs(cell, playground, in originalGrid, ref resultCells, spreadingRules);
				}
			}
		}
	}
}
