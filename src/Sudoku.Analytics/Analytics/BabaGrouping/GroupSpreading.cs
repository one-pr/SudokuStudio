namespace Sudoku.Analytics.BabaGrouping;

/// <summary>
/// Provides a way to spread a cell symbol.
/// </summary>
public static class GroupSpreading
{
	/// <summary>
	/// Spreads the specified assumption into multiple different cells.
	/// </summary>
	/// <param name="symbol">The original symbol.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="spreadingRules">The spreading rules.</param>
	/// <returns>All found symbols found.</returns>
	/// <exception cref="ArgumentException">Throws when an assumption is not fuzzy type.</exception>
	public static ReadOnlySpan<CellSymbol> Spread(
		CellSymbol symbol,
		in Grid grid,
		ReadOnlySpan<ISimpleSpreadingRule> spreadingRules
	)
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
			result.Add(new(cell, value));
		}
		return result.AsSpan();


		void dfs(
			Cell startCell,
			Grid playground,
			ref readonly Grid originalGrid,
			ref CellMap resultCells,
			ReadOnlySpan<ISimpleSpreadingRule> spreadingRules
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

	/// <summary>
	/// Try to suppose a house with different digits set, and find any conclusions on spread.
	/// </summary>
	/// <param name="house">The house to suppose.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="spreadingRules">The spreading rules used.</param>
	/// <param name="onlyFindOne">Indicates whether the method only find one conclusion and return.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>The conclusions found.</returns>
	public static ReadOnlySpan<BabaGroupingConclusion> Suppose(
		House house,
		in Grid grid,
		ReadOnlySpan<ISymbolSpreadingRule> spreadingRules,
		bool onlyFindOne,
		CancellationToken cancellationToken = default
	)
	{
		return (grid.EmptyCells & HousesMap[house]) switch
		{
			// There's no empty cells available. No conclusions found.
			[] => [],

			// Naked single found.
			[var onlyCell] => (BabaGroupingConclusion[])[
				new SyncBabaGroupingConclusion(
					[new CellSymbol(onlyCell, CellSymbolValue.FuzzyValues[0])],
					grid.GetCandidates(onlyCell)
				)
			],

			// Otherwise, we should iterate them up by multiple branches.
			var emptyCells => entry(in emptyCells, in grid, spreadingRules, onlyFindOne, cancellationToken)
		};


		static ReadOnlySpan<BabaGroupingConclusion> entry(
			scoped ref readonly CellMap emptyCells,
			ref readonly Grid grid,
			ReadOnlySpan<ISymbolSpreadingRule> spreadingRules,
			bool onlyFindOne,
			CancellationToken cancellationToken
		)
		{
			// Create a collection that stores initial suppositions.
			var initialSymbols = new List<CellSymbol>();

			// Provide a dictionary to map mask to symbol.
			var symbolValueLookup = new Dictionary<CellSymbolValue, Mask>();

			// Construct a list of suppositions.
			var emptyCellIndex = 0;
			foreach (var cell in emptyCells)
			{
				var value = CellSymbolValue.FuzzyValues[emptyCellIndex++];
				initialSymbols.Add(new(cell, value));
				symbolValueLookup.Add(value, grid.GetCandidates(cell));
			}

			// Define a dictionary that stores cached symbols applied into the grid, preventing them recording twice.
			// This variable can also be checked in the phase of conclusions.<br/><br/>
			// There are the cases can be concluded:
			// <list type="number">
			// <item>A symbol must be equal to a value cell in a house</item>
			// <item>Some symbols should form a subset or a distributed disjointed subset</item>
			// <item>A cell should be filled with one number defined in a symbol</item>
			// </list>
			var mappedSymbols = new Dictionary<CellMap, ComplexCellSymbol>();

			// Then we should suppose the cells. Iterate on them and add them into queue.
			var queue = new Queue<GroupSpreadingNode>();
			foreach (var initialSymbol in initialSymbols)
			{
				var symbol = initialSymbol.AsComplex();
				queue.Enqueue(new(symbol, null));
				mappedSymbols.Add(initialSymbol.Cell.AsCellMap(), symbol);
			}

			// Define a variable that stores results.
			var result = new List<BabaGroupingConclusion>();

			// Perform BFS operation.
			while (queue.Count != 0)
			{
				// Cancel the operation if requested.
				if (!cancellationToken)
				{
					goto ReturnsEmptyArray;
				}

				// Otherwise, check validity of mapped symbols, to find conclusions.
				// If any conclusions are found, we can collect it into the result.
				if (findConclusions(onlyFindOne))
				{
					goto ReturnResult;
				}

				// Dequeue a node.
				var node = queue.Dequeue();

				// Find for the next conclusions.

			}

		ReturnResult:
			return result.AsSpan();

		ReturnsEmptyArray:
			return [];


			[DoesNotReturn]
			bool findConclusions(bool onlyFindOne)
			{
				mappedSymbols.Clear();
				throw new NotImplementedException();
			}
		}
	}
}

/// <summary>
/// Represents a node that is used for spreading.
/// </summary>
/// <param name="symbol">The symbol.</param>
/// <param name="parent">The parent node.</param>
file sealed class GroupSpreadingNode(ComplexCellSymbol symbol, GroupSpreadingNode? parent)
{
	/// <summary>
	/// Indicates the symbol.
	/// </summary>
	public ComplexCellSymbol Symbol { get; } = symbol;

	/// <summary>
	/// Indicates the parent node.
	/// </summary>
	public GroupSpreadingNode? Parent { get; } = parent;
}
