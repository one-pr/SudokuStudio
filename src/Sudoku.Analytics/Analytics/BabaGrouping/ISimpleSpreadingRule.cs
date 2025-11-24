namespace Sudoku.Analytics.BabaGrouping;

/// <summary>
/// Represents simple spreading rule.
/// </summary>
public interface ISimpleSpreadingRule
{
	/// <summary>
	/// Spreads the candidate.
	/// </summary>
	/// <param name="candidate">The candidate.</param>
	/// <param name="cells">The cells.</param>
	/// <param name="grid">The grid.</param>
	void Spread(Candidate candidate, ref CellMap cells, ref readonly Grid grid);
}
