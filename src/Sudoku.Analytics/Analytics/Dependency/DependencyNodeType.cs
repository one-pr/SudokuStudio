namespace Sudoku.Analytics.Dependency;

/// <summary>
/// Represents a dependency node type.
/// </summary>
public enum DependencyNodeType
{
	/// <summary>
	/// Represents root node.
	/// </summary>
	Root,

	/// <summary>
	/// Represents the node is for supposing a candidate to be <see langword="true"/>.
	/// </summary>
	Supposing,

	/// <summary>
	/// Represents a new node becomes available because of hidden single in block.
	/// </summary>
	HiddenSingleBlock,

	/// <summary>
	/// Represents a new node becomes available because of hidden single in row.
	/// </summary>
	HiddenSingleRow,

	/// <summary>
	/// Represents a new node becomes available because of hidden single in column.
	/// </summary>
	HiddenSingleColumn,

	/// <summary>
	/// Represents a new node becomes available because of naked single.
	/// </summary>
	NakedSingle
}
