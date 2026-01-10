namespace Sudoku.Drawing.Nodes;

/// <summary>
/// Defines a diamond view node.
/// </summary>
/// <param name="identifier"><inheritdoc cref="IconViewNode(ColorDescriptor, Cell)" path="/param[@name='identifier']"/></param>
/// <param name="cell"><inheritdoc cref="IconViewNode(ColorDescriptor, Cell)" path="/param[@name='cell']"/></param>
public sealed class DiamondViewNode(ColorDescriptor identifier, Cell cell) : IconViewNode(identifier, cell)
{
	/// <inheritdoc/>
	public override DiamondViewNode Clone() => new(Identifier, Cell);
}
