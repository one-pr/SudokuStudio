namespace Sudoku.Analytics;

public partial class Hub
{
	public partial class DifficultyCalculator
	{
		/// <summary>
		/// Represents a list of methods that calculates for values in OEIS sequences.
		/// </summary>
		/// <shared-comments>
		/// <para>The index of the sequence. The index is 0-based.</para>
		/// <para>The index of the sequence. The index is 1-based.</para>
		/// </shared-comments>
		public sealed class OeisSequences
		{
			/// <summary>
			/// Gets the value at the specified index in the sequence <see href="https://oeis.org/A002024">A002024</see>
			/// (1, 2, 2, 3, 3, 3, 4, 4, 4, 4, 5, ..).
			/// </summary>
			/// <param name="index"><inheritdoc cref="OeisSequences" path="//shared-comments/para[2]"/></param>
			/// <returns>The result value at the specified index.</returns>
			public static int A002024(int index) => (int)(Math.Sqrt(index << 1) + .5);
		}
	}
}
