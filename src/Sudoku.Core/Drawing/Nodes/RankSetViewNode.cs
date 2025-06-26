namespace Sudoku.Drawing.Nodes;

/// <summary>
/// Represents a rank set (truth or link) view node.
/// </summary>
/// <param name="identifier"><inheritdoc cref="ViewNode.Identifier" path="/summary"/></param>
/// <param name="space"><inheritdoc cref="Space" path="/summary"/></param>
[TypeImpl(
	TypeImplFlags.Object_GetHashCode | TypeImplFlags.Object_ToString,
	OtherModifiersOnGetHashCode = "sealed",
	OtherModifiersOnToString = "sealed")]
public abstract partial class RankSetViewNode(ColorIdentifier identifier, Space space) : ViewNode(identifier), ILinkViewNode
{
	/// <summary>
	/// Indicates whether the rank set is a truth.
	/// </summary>
	public abstract bool IsTruth { get; }

	/// <summary>
	/// Indicates the space.
	/// </summary>
	[HashCodeMember]
	[StringMember]
	public Space Space { get; } = space;

	/// <inheritdoc/>
	object ILinkViewNode.Start => null!;

	/// <inheritdoc/>
	object ILinkViewNode.End => null!;

	/// <inheritdoc/>
	LinkShape ILinkViewNode.Shape => LinkShape.RankSet;
}
