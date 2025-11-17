namespace Sudoku.Theories.BabaGroupingTheory;

/// <summary>
/// Provides a way to define spreading rule.
/// </summary>
public abstract class SpreadingRule
{
	/// <summary>
	/// Spreads the candidate.
	/// </summary>
	/// <param name="candidate">The candidate.</param>
	/// <param name="cells">The cells.</param>
	/// <param name="grid">The grid.</param>
	public abstract void Spread(Candidate candidate, ref CellMap cells, ref readonly Grid grid);
}
