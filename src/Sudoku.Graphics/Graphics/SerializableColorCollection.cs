namespace Sudoku.Graphics;

/// <summary>
/// Represents a list of <see cref="SerializableColor"/> instances.
/// </summary>
/// <seealso cref="SerializableColor"/>
public sealed class SerializableColorCollection :
	List<SerializableColor>,
	ISliceMethod<SerializableColorCollection, SerializableColor>
{
	/// <inheritdoc cref="List{T}.Slice(int, int)"/>
	public new SerializableColorCollection Slice(int start, int length) => [.. base.Slice(start, length)];

	/// <inheritdoc/>
	IEnumerable<SerializableColor> ISliceMethod<SerializableColorCollection, SerializableColor>.Slice(int start, int count)
		=> Slice(start, count);
}
