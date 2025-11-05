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
	/// Represents a new node becomes available because of hidden single in block, or pointing in locked candidates rule.
	/// </summary>
	Block,

	/// <summary>
	/// Represents a new node becomes available because of hidden single in row, or claiming in locked candidates rule.
	/// </summary>
	Row,

	/// <summary>
	/// Represents a new node becomes available because of hidden single in column, or claiming in locked candidates rule.
	/// </summary>
	Column,

	/// <summary>
	/// Represents a new node becomes available because of naked single.
	/// </summary>
	Cell
}
