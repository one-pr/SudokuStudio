namespace Sudoku.Drawing.Nodes;

/// <summary>
/// Defines a view node that highlights for a chute (i.e. 3 houses that is in a three blocks in a line).
/// </summary>
/// <param name="identifier"><inheritdoc/></param>
/// <param name="chuteIndex"><inheritdoc cref="ChuteIndex" path="/summary"/></param>
[method: JsonConstructor]
public sealed class ChuteViewNode(ColorDescriptor identifier, int chuteIndex) : BasicViewNode(identifier)
{
	/// <summary>
	/// Indicates the chute index. The value can be between 0 and 5.
	/// </summary>
	public int ChuteIndex { get; } = chuteIndex;

	/// <summary>
	/// Indicates whether the chute is in a row.
	/// </summary>
	[JsonIgnore]
	public bool IsRow => ChuteIndex < 3;

	/// <summary>
	/// <para>
	/// Indicates a <see cref="HouseMask"/> that represents for the houses used.
	/// The result mask is a 27-bit digit that represents every possible houses using cases.
	/// </para>
	/// <para>
	/// Please note that the first 9-bit always keep the zero value because they is reserved bits
	/// for block houses, but all chutes don't use them.
	/// </para>
	/// </summary>
	[JsonIgnore]
	public HouseMask HousesMask => Chute.Chutes[ChuteIndex] switch { var (_, _, rawMask) => rawMask };


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	public void Deconstruct(out ColorDescriptor identifier, out int chute) => (identifier, chute) = (Identifier, ChuteIndex);

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] ViewNode? other)
		=> base.Equals(other) && other is ChuteViewNode comparer && ChuteIndex == comparer.ChuteIndex;

	/// <inheritdoc cref="object.GetHashCode"/>
	public override int GetHashCode() => HashCode.Combine(ChuteIndex, TypeIdentifier);

	/// <inheritdoc/>
	public override string ToString()
		=> $"{nameof(ChuteViewNode)} {{ {nameof(ChuteIndex)} = {ChuteIndex}, {nameof(Identifier)} = {Identifier} }}";

	/// <inheritdoc/>
	public override ChuteViewNode Clone() => new(Identifier, ChuteIndex);
}
