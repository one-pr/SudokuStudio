namespace Sudoku.Concepts;

using Type_Cell = Cell;

public partial class Limits
{
	/// <summary>
	/// Indicates limits on chute cells count.
	/// </summary>
	public static class ChuteCellsCount
	{
		/// <summary>
		/// Indicates min value of chute cells count.
		/// </summary>
		public const Type_Cell Min = 0;

		/// <summary>
		/// Indicates max value of chute cells count.
		/// </summary>
		public const Type_Cell Max = 27;
	}
}
