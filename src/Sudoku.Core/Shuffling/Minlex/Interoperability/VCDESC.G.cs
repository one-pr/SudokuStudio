namespace Sudoku.Shuffling.Minlex.Interoperability;

internal partial struct VCDESC
{
	/// <summary>
	/// Represents the backing values storage.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct G
	{
		/// <summary>
		/// The <c>b1</c> field.
		/// </summary>
		public unsafe fixed sbyte b1[27];

		/// <summary>
		/// The <c>r4</c> field.
		/// </summary>
		public unsafe fixed sbyte r4[9];

		/// <summary>
		/// The <c>rx</c> field.
		/// </summary>
		public unsafe fixed sbyte rx[45];
	}
}
