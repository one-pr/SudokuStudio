namespace Sudoku.Analytics.BabaGrouping.SpreadingRules;

/// <summary>
/// Defines locked candidates spreading rule.
/// </summary>
public sealed class LockedCandidatesSpreadingRule : ISpreadingRule
{
	/// <inheritdoc/>
	public void Spread(Candidate candidate, ref CellMap cells, ref readonly Grid grid)
	{
	}
}
