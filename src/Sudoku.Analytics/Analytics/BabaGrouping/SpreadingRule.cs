namespace Sudoku.Analytics.BabaGrouping;

/// <summary>
/// Represents a spreading rule.
/// </summary>
public abstract class SpreadingRule
{
	/// <summary>
	/// Spreads the symbol, to find for next symbols that can be concluded.
	/// </summary>
	/// <param name="context">The context.</param>
	public abstract void Spread(ref GroupSpreadingContext context);
}
