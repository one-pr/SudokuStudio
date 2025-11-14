namespace Sudoku.DeadlyPatternTheory;

/// <summary>
/// Represents a reason describing why the analysis operation is failed.
/// </summary>
public enum DeadlyPatternResultFailedReason
{
	/// <summary>
	/// Indicates nothing goes wrong.
	/// </summary>
	None,

	/// <summary>
	/// Indicates the grid is not a deadly pattern.
	/// </summary>
	NotDeadlyPattern,

	/// <summary>
	/// Indicates the number of solutions is reached.
	/// </summary>
	MaxSolutionsReached
}
