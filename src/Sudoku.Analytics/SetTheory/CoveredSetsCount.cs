namespace Sudoku.SetTheory;

/// <summary>
/// Represents a result value of covered sets, returned by <see cref="PatternReasoner.GetCoveredSetsCount(in Pattern, int)"/>.
/// </summary>
/// <param name="TruthsCount">Indicates the number of truths covered.</param>
/// <param name="LinksCount">Indicates the number of links covered.</param>
/// <seealso cref="PatternReasoner.GetCoveredSetsCount(in Pattern, int)"/>.
public readonly record struct CoveredSetsCount(int TruthsCount, int LinksCount) :
	IEqualityOperators<CoveredSetsCount, CoveredSetsCount, bool>
{
	/// <summary>
	/// Indicates whether a candidate is exact-covered.
	/// </summary>
	public bool IsExactCovered => this is (1, 1);
}
