namespace Sudoku.Concepts;

/// <summary>
/// Provides with extension members on <see cref="Grid"/> instances.
/// </summary>
/// <seealso cref="Grid"/>
public static class GridConclusionExtensions
{
	/// <summary>
	/// Provides extension members on <see langword="ref"/> <see cref="Grid"/>.
	/// </summary>
	extension(ref Grid @this)
	{
#if USER_DEFINED_COMPOUND_ASSIGNMENT_OPERATORS
		/// <summary>
		/// Applies the conclusion to the current grid.
		/// </summary>
		/// <param name="conclusion">The conclusion.</param>
		public void operator >>=(Conclusion conclusion) => @this.Apply(conclusion);
#endif


		/// <summary>
		/// Applies the conclusion to the target grid.
		/// </summary>
		/// <param name="grid">The grid.</param>
		/// <param name="conclusion">The conclusion.</param>
		/// <returns>The target grid.</returns>
		public static Grid operator >>(in Grid grid, Conclusion conclusion)
		{
			var tempGrid = grid;
			tempGrid.Apply(conclusion);
			return tempGrid;
		}
	}
}
