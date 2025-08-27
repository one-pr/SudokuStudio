namespace Sudoku.Drawing;

/// <summary>
/// Represents an option that specifies a behavior while view cloning.
/// </summary>
/// <seealso cref="View"/>
public enum ViewCloningOption
{
	/// <summary>
	/// Indicates the cloneation behavior is default one, all nodes are copied into a new collection,
	/// but nodes themselves are not cloned with new instances.
	/// </summary>
	/// <remarks>
	/// Meaning: <c>result = [.. <see langword="this"/>]</c>
	/// </remarks>
	Default,

	/// <summary>
	/// Indicates the cloneation behavior includes nodes.
	/// All nodes are copied into a new collection, and data from each node will be copied into a new instance.
	/// </summary>
	/// <remarks>
	/// Meaning: <c>result = [.. <see langword="from"/> node <see langword="in this select"/> node.Clone()]</c>
	/// </remarks>
	IncludingNodes
}
