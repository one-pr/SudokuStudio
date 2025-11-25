namespace Sudoku.Analytics.BabaGrouping;

/// <summary>
/// Provides a way to spread a <see cref="ComplexCellSymbol"/>.
/// </summary>
/// <seealso cref="ComplexCellSymbol"/>
public sealed class GroupSpreader
{
	/// <summary>
	/// Indicates the rules.
	/// </summary>
	public ReadOnlyMemory<SpreadingRule> Rules { get; }


	/// <summary>
	/// Try to suppose a house with different digits set, and find any conclusions on spread.
	/// </summary>
	/// <param name="house">The house to suppose.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="onlyFindOne">Indicates whether the method only find one conclusion and return.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>The conclusions found.</returns>
	public ReadOnlySpan<BabaGroupingConclusion> Suppose(House house, in Grid grid, bool onlyFindOne, CancellationToken cancellationToken = default)
	{
		var emptyCells = grid.EmptyCells & HousesMap[house];
		if (!emptyCells)
		{
			// There's no empty cells available. No conclusions found.
			return [];
		}

		if (emptyCells is [var onlyCell])
		{
			// Naked single found.
			return (BabaGroupingConclusion[])[
				new SyncBabaGroupingConclusion(
					new CellSymbol(onlyCell, CellSymbolValue.FuzzyValues[0]).AsComplex(),
					grid.GetCandidates(onlyCell)
				)
			];
		}

		// Otherwise, we should iterate them up by multiple branches.
		var context = new GroupSpreadingContext(in grid) { OnlyFindOne = onlyFindOne };

		// Create a collection that stores initial suppositions.
		var initialSymbols = new List<CellSymbol>();

		// Construct a list of suppositions.
		var emptyCellIndex = 0;
		foreach (var cell in emptyCells)
		{
			var value = CellSymbolValue.FuzzyValues[emptyCellIndex++];
			initialSymbols.Add(new(cell, value));
			context.SymbolValuesLookup.Add(value, grid.GetCandidates(cell));
		}

		// Then we should suppose the cells. Iterate on them and add them into queue.
		var queue = new Queue<GroupSpreadingNode>();
		foreach (var initialSymbol in initialSymbols)
		{
			var symbol = initialSymbol.AsComplex();
			queue.Enqueue(new(symbol, null));
			context.MappedSymbols.Add(initialSymbol.Cell.AsCellMap(), symbol);
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
			if (findConclusions(ref context))
			{
				goto ReturnResult;
			}

			// Dequeue a node.
			var node = queue.Dequeue();

			// Find for the next symbols.

		}

	ReturnResult:
		return result.AsSpan();

	ReturnsEmptyArray:
		return [];


		static bool findConclusions(ref GroupSpreadingContext context)
		{
			throw new NotImplementedException();
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
