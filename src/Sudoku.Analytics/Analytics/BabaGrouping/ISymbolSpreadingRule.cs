namespace Sudoku.Analytics.BabaGrouping;

/// <summary>
/// Represents symbol spreading rule.
/// </summary>
public interface ISymbolSpreadingRule
{
	/// <summary>
	/// Spreads the symbol.
	/// </summary>
	/// <param name="symbol">The symbol.</param>
	/// <param name="inferredSymbols">The symbols that can be inferred from <paramref name="symbol"/>.</param>
	/// <param name="grid">The grid.</param>
	void SpreadSymbol(ComplexCellSymbol symbol, HashSet<ComplexCellSymbol> inferredSymbols, ref readonly Grid grid);
}
