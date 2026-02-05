namespace Sudoku.Concepts;

using Type_Cell = Cell;

public partial class Limits
{
	/// <summary>
	/// Indicates limits on house cells count.
	/// </summary>
	public static class HouseCellsCount
	{
		/// <summary>
		/// Indicates min value of house cells count.
		/// </summary>
		public const Type_Cell Min = 0;

		/// <summary>
		/// Indicates max value of house cells count.
		/// </summary>
		public const Type_Cell Max = 9;
	}
}
