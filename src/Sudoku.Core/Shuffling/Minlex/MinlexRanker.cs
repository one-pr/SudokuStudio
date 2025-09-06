namespace Sudoku.Shuffling.Minlex;

/// <summary>
/// Represents a ranker type that can rank a min-lex grid, or unrank a value to target grid.
/// </summary>
/// <remarks>
/// This type is implemented externally. You should copy <c>*.dll</c> files in folder <c>minlex-rank</c>
/// defined in <c>miscellaneous</c> folder in this repository to your device before consume methods defined in this type.
/// </remarks>
/// <seealso href="https://github.com/GPenet/Virtual-calatog">Virtual Catalog (GitHub repository)</seealso>
/// <seealso href="http://forum.enjoysudoku.com/virtual-catalog-dll-t45193.html">Virtual catalog DLL (EnjoySudoku forum)</seealso>
public static class MinlexRanker
{

}

/// <summary>
/// Represents a stack-allocated memory exchanging area.
/// </summary>
/// <remarks>
/// This type is defined
/// <see href="https://github.com/GPenet/Virtual-catalog-DLL/blob/f8bca9fab905e1bbb8acaefdc4a5b4f185fdf2fa/skvcats.h#L30">here</see>.
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct VCDESC
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


	/// <summary>
	/// Represents the backing values storage.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct G
	{
		/// <summary>
		/// The <c>b1</c> field.
		/// </summary>
		public fixed sbyte b1[27];

		/// <summary>
		/// The <c>r4</c> field.
		/// </summary>
		public fixed sbyte r4[9];

		/// <summary>
		/// The <c>rx</c> field.
		/// </summary>
		public fixed sbyte rx[45];
	}
}
