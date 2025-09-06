namespace Sudoku.Shuffling.Minlex;

public partial class MinlexRanker
{
	/// <summary>
	/// Represents a stack-allocated memory exchanging area.
	/// </summary>
	/// <remarks>
	/// This type is defined
	/// <see href="https://github.com/GPenet/Virtual-catalog-DLL/blob/f8bca9fab905e1bbb8acaefdc4a5b4f185fdf2fa/skvcats.h#L30">here</see>.
	/// </remarks>
	/// <seealso href="https://github.com/GPenet/Virtual-calatog">Virtual Catalog (GitHub repository)</seealso>
	/// <seealso href="http://forum.enjoysudoku.com/virtual-catalog-dll-t45193.html">Virtual catalog DLL (EnjoySudoku forum)</seealso>
	[StructLayout(LayoutKind.Sequential)]
	private partial struct VCDESC
	{
		/// <summary>
		/// Indicates the rank.
		/// </summary>
		public ulong rank;

		/// <summary>
		/// Indicates the calculated values to represent.
		/// </summary>
		public int i416, i9992, i660k1, ir1, i660k2, ir2;

		/// <summary>
		/// Indicates the nested type that stores the backing data.
		/// </summary>
		public G g;
	}
}
