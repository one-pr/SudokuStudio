namespace Sudoku.Descriptors;

/// <summary>
/// Provides metadata for a chunk element type of a field.
/// </summary>
[AttributeUsage(AttributeTargets.Field, Inherited = false)]
public sealed class ChunkElementAttribute(ChunkElement element) : Attribute
{
	/// <summary>
	/// Indicates the chunk element type.
	/// </summary>
	public ChunkElement Element { get; } = element;

	/// <summary>
	/// Indicates the converter type. The target type must be derived from <see cref="ChunkElementConverter"/>.
	/// </summary>
	/// <exception cref="InvalidOperationException">
	/// Throws when the target type cannot be assignable to <see cref="ChunkElementConverter"/>.
	/// </exception>
	/// <seealso cref="ChunkElementConverter"/>
	public required Type ConverterType
	{
		get;

		init => field = value.IsAssignableTo(typeof(ChunkElementConverter))
			? value
			: throw new InvalidOperationException($"The target type must be assignable to type '{nameof(ChunkElementConverter)}'.");
	}
}
