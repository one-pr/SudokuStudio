namespace Sudoku.Drawing.Nodes;

/// <summary>
/// Defines a view node that highlights for a cell.
/// </summary>
/// <param name="identifier"><inheritdoc/></param>
/// <param name="cell"><inheritdoc cref="Cell" path="/summary"/></param>
[method: JsonConstructor]
public sealed class CellViewNode(ColorDescriptor identifier, Cell cell) : BasicViewNode(identifier)
{
	/// <summary>
	/// Indicates the cell highlighted.
	/// </summary>
	public Cell Cell { get; } = cell;


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	public void Deconstruct(out ColorDescriptor identifier, out Cell cell) => (identifier, cell) = (Identifier, Cell);

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] ViewNode? other)
		=> base.Equals(other) && other is CellViewNode comparer && Cell == comparer.Cell;

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(Cell, TypeIdentifier);

	/// <inheritdoc/>
	public override string ToString()
	{
		var cellString = Cell.ToCellString(Cell, CoordinateConverter.InvariantCulture);
		return $"{nameof(CellViewNode)} {{ {nameof(Cell)} = {cellString}, {nameof(Identifier)} = {Identifier} }}";
	}

	/// <inheritdoc/>
	public override CellViewNode Clone() => new(Identifier, Cell);
}
