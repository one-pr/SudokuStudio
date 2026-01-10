namespace Sudoku.Drawing.Nodes;

/// <summary>
/// Defines a view node that highlights for a baba group.
/// </summary>
/// <param name="identifier"><inheritdoc/></param>
/// <param name="cell"><inheritdoc cref="Cell" path="/summary"/></param>
/// <param name="unknownValueChar"><inheritdoc cref="UnknownValueChar" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="DigitsMask" path="/summary"/></param>
[method: JsonConstructor]
public sealed class BabaGroupViewNode(ColorDescriptor identifier, Cell cell, char unknownValueChar, Mask digitsMask) :
	BasicViewNode(identifier)
{
	/// <summary>
	/// Initializes a <see cref="BabaGroupViewNode"/> instance via the specified values.
	/// </summary>
	/// <inheritdoc cref="BabaGroupViewNode(ColorDescriptor, Cell, char, Mask)"/>
	public BabaGroupViewNode(Cell cell, char unknownValueChar, Mask digitsMask) :
		this(ColorDescriptorAlias.Normal, cell, unknownValueChar, digitsMask)
	{
	}


	/// <summary>
	/// Indicates the cell used.
	/// </summary>
	public int Cell { get; } = cell;

	/// <summary>
	/// Indicates the character that represents the baba group name.
	/// </summary>
	public char UnknownValueChar { get; } = unknownValueChar;

	/// <summary>
	/// Indicates a mask that hold digits used.
	/// </summary>
	public Mask DigitsMask { get; } = digitsMask;


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	public void Deconstruct(out ColorDescriptor identifier, out Cell cell, out char unknownValueChar)
		=> (identifier, cell, unknownValueChar) = (Identifier, Cell, UnknownValueChar);

	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	public void Deconstruct(out ColorDescriptor identifier, out Cell cell, out Mask digitsMask, out char unknownValueChar)
		=> ((identifier, cell, unknownValueChar), digitsMask) = (this, DigitsMask);

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] ViewNode? other)
		=> base.Equals(other) && other is BabaGroupViewNode comparer && Cell == comparer.Cell;

	/// <inheritdoc cref="object.GetHashCode"/>
	public override int GetHashCode() => HashCode.Combine(Cell, TypeIdentifier);

	/// <inheritdoc/>
	public override string ToString()
	{
		var cellsString = Cell.ToCellString(Cell, CoordinateConverter.InvariantCulture);
		var digitsString = Convert.ToString(DigitsMask, 2).ToString();
		return $"{nameof(BabaGroupViewNode)} {{ {nameof(UnknownValueChar)} = {UnknownValueChar}, Cell = {cellsString}, Digits = {digitsString}, Identifier = {Identifier} }}";
	}

	/// <inheritdoc/>
	public override BabaGroupViewNode Clone() => new(Identifier, Cell, UnknownValueChar, DigitsMask);
}
