namespace Sudoku.Descriptors;

/// <summary>
/// Indicates type of chunk of items (cell, candidate, house, etc.).
/// </summary>
public enum ChunkType
{
	/// <summary>
	/// Indicates placeholder field of this type.
	/// </summary>
	None = 0,

	/// <summary>
	/// Indicates the chunk type is for a cell.
	/// </summary>
	Cell,
}
