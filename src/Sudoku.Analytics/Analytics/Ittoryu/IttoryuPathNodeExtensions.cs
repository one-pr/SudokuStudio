namespace Sudoku.Analytics.Ittoryu;

/// <summary>
/// Provides extension methods on <see cref="Grid"/> on applying <see cref="IttoryuPathNode"/> instances.
/// </summary>
/// <seealso cref="Grid"/>
/// <seealso cref="IttoryuPathNode"/>
internal static class IttoryuPathNodeExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
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
