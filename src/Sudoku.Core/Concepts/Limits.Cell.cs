namespace Sudoku.Concepts;

using Type_Cell = Cell;

public partial class Limits
{
	/// <summary>
	/// Indicates cell-related limits.
	/// </summary>
	public static class Cell
	{
		/// <summary>
		/// Indicates min value of cell.
		/// </summary>
		public const Type_Cell Min = 0;

		/// <summary>
		/// Indicates max value of cell.
		/// </summary>
		public const Type_Cell Max = 80;
	}
}
