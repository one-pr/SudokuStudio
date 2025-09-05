namespace Sudoku.Shuffling.Minlex;

/// <summary>
/// Provides with extension methods on <see cref="Grid"/> for minlex.
/// </summary>
/// <seealso cref="Grid"/>
public static class GridMinlexExtensions
{
	/// <summary>
	/// Provides extension members on <see langword="ref"/> <see cref="Grid"/>.
	/// </summary>
	extension(ref Grid @this)
	{
		/// <summary>
		/// Adjust the grid to minimal lexicographical form.
		/// </summary>
		public void MakeMinLex() => @this = @this.MinLexGrid;
	}

	/// <summary>
	/// Provides extension members on <see langword="in"/> <see cref="Grid"/>.
	/// </summary>
	extension(in Grid @this)
	{
		/// <summary>
		/// Indicates whether the current grid is the minimal lexicographical form,
		/// meaning the corresponding string text code is the minimum value in all equivalent transforming cases
		/// in lexicographical order.
		/// </summary>
		public bool IsMinLex
			=> @this.PuzzleType != SudokuType.Sukaku && @this.Uniqueness == Uniqueness.Unique && @this.ToString("0") is var s
				? new MinlexFinder().Find(s, out _) == s
				: throw new InvalidOperationException(SR.ExceptionMessage("MinLexShouldBeUniqueAndNotSukaku"));

		/// <summary>
		/// Indicates the new grid form of the current grid, which is at the minimal-lexicographical order.
		/// </summary>
		public Grid MinLexGrid => new MinlexFinder().Find(in @this);
	}
}
