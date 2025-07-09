namespace Sudoku.Generating.Filtering;

/// <summary>
/// Represent an operator to be used in precedence checking.
/// </summary>
public enum PrecedenceOperator
{
	/// <summary>
	/// Indicates the precendence operator is predecessor.
	/// </summary>
	Predecessor,

	/// <summary>
	/// Indicates the precendence operator is successor.
	/// </summary>
	Successor
}
