namespace Sudoku.Analytics.BabaGrouping;

/// <summary>
/// Provides a type of conclusion of baba grouping.
/// </summary>
public enum BabaGroupingConclusionType
{
	/// <summary>
	/// Indicates the conclusion type is occupation.
	/// </summary>
	Occupation,

	/// <summary>
	/// Indicates the conclusion type is subset.
	/// </summary>
	Subset,

	/// <summary>
	/// Indicates the conclusion type is sync'ing.
	/// </summary>
	Sync
}
