namespace Sudoku.Drawing.Nodes;

/// <summary>
/// Defines a view node that highlights for a house.
/// </summary>
/// <param name="identifier"><inheritdoc/></param>
/// <param name="house"><inheritdoc cref="House" path="/summary"/></param>
[method: JsonConstructor]
public sealed class HouseViewNode(ColorDescriptor identifier, House house) : BasicViewNode(identifier)
{
	/// <summary>
	/// Indicates the house highlighted.
	/// </summary>
	public House House { get; } = house;


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	public void Deconstruct(out ColorDescriptor identifier, out House house) => (identifier, house) = (Identifier, House);

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] ViewNode? other)
		=> base.Equals(other) && other is HouseViewNode comparer && House == comparer.House;

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(House, TypeIdentifier);

	/// <inheritdoc/>
	public override string ToString()
		=> $"{nameof(HouseViewNode)} {{ {nameof(House)} = {House}, {nameof(Identifier)} = {Identifier} }}";

	/// <inheritdoc/>
	public override HouseViewNode Clone() => new(Identifier, House);
}
