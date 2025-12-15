namespace Sudoku.Descriptors;

/// <summary>
/// Indicates the element type of a chunk (a single item, multiple items, etc.).
/// </summary>
public enum ChunkElement
{
	/// <summary>
	/// The placeholder of this type.
	/// </summary>
	None = 0,

	/// <summary>
	/// Indicates the element is a single value.
	/// </summary>
	Single,

	/// <summary>
	/// Indicates the element is a bit state map.
	/// </summary>
	BitStateMap,

	/// <summary>
	/// Indicates the element is an array.
	/// </summary>
	Array,

	/// <summary>
	/// Indicates the element is a list.
	/// </summary>
	List,

	/// <summary>
	/// Indicates the element is a hash set.
	/// </summary>
	HashSet
}
