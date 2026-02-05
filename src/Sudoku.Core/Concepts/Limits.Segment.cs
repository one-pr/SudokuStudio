namespace Sudoku.Concepts;

using Type_SegmentIndex = int;

public partial class Limits
{
	/// <summary>
	/// Indicates segment-related limits.
	/// </summary>
	public static class Segment
	{
		/// <summary>
		/// Indicates min value of segment index.
		/// </summary>
		public const Type_SegmentIndex Min = 0;

		/// <summary>
		/// Indicates max value of segment index.
		/// </summary>
		public const Type_SegmentIndex Max = 53;
	}
}
