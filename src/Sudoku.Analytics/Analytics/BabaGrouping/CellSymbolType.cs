namespace Sudoku.Analytics.BabaGrouping;

/// <summary>
/// Represents a type of an instance of type <see cref="CellSymbolValue"/>.
/// </summary>
/// <seealso cref="CellSymbolValue"/>
public enum CellSymbolType
{
	/// <summary>
	/// Indicates the type is fuzzy.
	/// </summary>
	Fuzzy,

	/// <summary>
	/// Indicates the type is accurate.
	/// </summary>
	Accurate
}
