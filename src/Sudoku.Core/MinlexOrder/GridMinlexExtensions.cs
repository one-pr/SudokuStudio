namespace Sudoku.MinlexOrder;

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
		/// Adjust the grid to minimum lexicographical form.
		/// </summary>
		public void MakeMinLex() => @this = @this.MinLexGrid;
	}

	/// <summary>
	/// Provides extension members on <see langword="in"/> <see cref="Grid"/>.
	/// </summary>
	extension(in Grid @this)
	{
		/// <summary>
		/// Indicates whether the current grid is the minimum lexicographical form,
		/// meaning the corresponding string text code is the minimum value in all equivalent transforming cases
		/// in lexicographical order.
		/// </summary>
		public bool IsMinLex
			=> !@this.IsSukaku && @this.Uniqueness == Uniqueness.Unique && @this.ToString("0") is var s
				? new MinlexFinder().Find(s) == s
				: throw new InvalidOperationException(SR.ExceptionMessage("MinLexShouldBeUniqueAndNotSukaku"));

		/// <summary>
		/// Indicates the new grid form of the current grid, which is at the minimal-lexicographical order.
		/// </summary>
		public Grid MinLexGrid => new MinlexFinder().Find(in @this);


		/// <summary>
		/// Returns a <see cref="Transform"/> instance that is the necessary transformation to the current grid,
		/// starting from <paramref name="minlex"/>.
		/// </summary>
		/// <param name="minlex">The min-lex grid.</param>
		/// <returns>The target transformation.</returns>
		public Transform GetTransformFromMinlex(out Grid minlex)
		{
			minlex = new MinlexFinder().Find(@this, out var result);
			return result;
		}
	}
}
