namespace Sudoku.Algorithms.Ittoryu;

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
		/// <inheritdoc cref="op_RightShiftAssignment(ref Grid, IttoryuPathNode)"/>
		[Obsolete(DeprecatedMessages.ExtensionOperator_Apply, false)]
		public void Apply(IttoryuPathNode node) => @this >>= node;


		/// <summary>
		/// Applies the node to the current grid.
		/// </summary>
		/// <param name="node">The node.</param>
		public void operator >>=(IttoryuPathNode node)
		{
			if (@this.GetState(node.Cell) == CellState.Empty)
			{
				@this.SetDigit(node.Cell, node.Digit);
			}
		}


		/// <inheritdoc cref="op_RightShiftAssignment(ref Grid, IttoryuPathNode)"/>
		public static Grid operator >>(in Grid grid, IttoryuPathNode node)
		{
			var tempGrid = grid;
			tempGrid >>= node;
			return tempGrid;
		}
	}
}
