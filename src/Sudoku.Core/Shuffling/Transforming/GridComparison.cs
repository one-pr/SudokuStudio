namespace Sudoku.Shuffling.Transforming;

/// <summary>
/// Specifies the comparison rule of a <see cref="IGrid{TSelf}"/> instance.
/// </summary>
/// <seealso cref="IGrid{TSelf}"/>
public enum GridComparison
{
	/// <summary>
	/// Indicates two <see cref="IGrid{TSelf}"/> instances compare with each other by using the default checking rule
	/// (cell by cell, bit by bit).
	/// </summary>
	Default,

	/// <summary>
	/// Indicates two <see cref="IGrid{TSelf}"/> instances compare with each other, including considerations on transforming cases.
	/// </summary>
	IncludingTransforms
}
