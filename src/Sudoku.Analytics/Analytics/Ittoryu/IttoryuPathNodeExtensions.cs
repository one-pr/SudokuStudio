namespace Sudoku.Analytics.Ittoryu;

/// <summary>
/// Provides extension methods on <see cref="Grid"/> on applying <see cref="IttoryuPathNode"/> instances.
/// </summary>
/// <seealso cref="Grid"/>
/// <seealso cref="IttoryuPathNode"/>
internal static class IttoryuPathNodeExtensions
{
	/// <summary>
	/// Provides extension members on <see langword="ref"/> <see cref="Grid"/>.
	/// </summary>
	extension(ref Grid @this)
	{
		/// <summary>
		/// Applies the node to the current grid.
		/// </summary>
		/// <param name="node">The node.</param>
		public void Apply(IttoryuPathNode node)
		{
			if (@this.GetState(node.Cell) == CellState.Empty)
			{
				@this.SetDigit(node.Cell, node.Digit);
			}
		}
	}
}
