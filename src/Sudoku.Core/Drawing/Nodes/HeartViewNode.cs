namespace Sudoku.Drawing.Nodes;

/// <summary>
/// Defines a heart view node.
/// </summary>
/// <param name="identifier"><inheritdoc cref="IconViewNode(ColorDescriptor, Cell)" path="/param[@name='identifier']"/></param>
/// <param name="cell"><inheritdoc cref="IconViewNode(ColorDescriptor, Cell)" path="/param[@name='cell']"/></param>
public sealed class HeartViewNode(ColorDescriptor identifier, Cell cell) : IconViewNode(identifier, cell)
{
	/// <inheritdoc/>
	public override HeartViewNode Clone() => new(Identifier, Cell);
}
