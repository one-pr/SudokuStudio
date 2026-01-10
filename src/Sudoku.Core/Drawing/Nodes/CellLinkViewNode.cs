namespace Sudoku.Drawing.Nodes;

/// <summary>
/// Defines a view node that highlights for a cell link.
/// </summary>
/// <param name="identifier"><inheritdoc/></param>
/// <param name="start"><inheritdoc cref="Start" path="/summary"/></param>
/// <param name="end"><inheritdoc cref="End" path="/summary"/></param>
[method: JsonConstructor]
public sealed class CellLinkViewNode(ColorDescriptor identifier, Cell start, Cell end) : BasicViewNode(identifier), ILinkViewNode
{
	/// <summary>
	/// Indicates the start point.
	/// </summary>
	public Cell Start { get; } = start;

	/// <summary>
	/// Indicates the end point.
	/// </summary>
	public Cell End { get; } = end;

	/// <inheritdoc/>
	object ILinkViewNode.Start => Start;

	/// <inheritdoc/>
	object ILinkViewNode.End => End;

	/// <inheritdoc/>
	LinkShape ILinkViewNode.Shape => LinkShape.Cell;


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	public void Deconstruct(out ColorDescriptor identifier, out Cell start, out Cell end)
		=> (identifier, start, end) = (Identifier, Start, End);

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] ViewNode? other)
		=> base.Equals(other) && other is CellLinkViewNode comparer && Start == comparer.Start && End == comparer.End;

	/// <inheritdoc cref="object.GetHashCode"/>
	public override int GetHashCode() => HashCode.Combine(Start, End, TypeIdentifier);

	/// <inheritdoc cref="object.ToString"/>
	public override string ToString()
		=> $"{nameof(CellLinkViewNode)} {{ {nameof(Start)} = {Start}, {nameof(End)} = {End}, {nameof(Identifier)} = {Identifier} }}";

	/// <inheritdoc/>
	public override CellLinkViewNode Clone() => new(Identifier, Start, End);
}
