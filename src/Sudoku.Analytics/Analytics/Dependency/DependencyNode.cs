namespace Sudoku.Analytics.Dependency;

/// <summary>
/// Represents a dependency node.
/// </summary>
/// <param name="type"><inheritdoc cref="Type" path="/summary"/></param>
/// <param name="grid"><inheritdoc cref="Grid" path="/summary"/></param>
/// <param name="assignment"><inheritdoc cref="Assignment" path="/summary"/></param>
/// <param name="parent"><inheritdoc cref="Parent" path="/summary"/></param>
public sealed class DependencyNode(DependencyNodeType type, in Grid grid, AssignmentInfo? assignment, DependencyNode? parent)
{
	/// <summary>
	/// The backing field of <see cref="Grid"/>.
	/// </summary>
	private readonly Grid _grid = grid;


	/// <summary>
	/// Indicates the type.
	/// </summary>
	public DependencyNodeType Type { get; } = type;

	/// <summary>
	/// Indicates the grid to be used.
	/// </summary>
	public ref readonly Grid Grid => ref _grid;

	/// <summary>
	/// Indicates the current assignment. The value is <see langword="null"/> if the node is root.
	/// </summary>
	public AssignmentInfo? Assignment { get; } = assignment;

	/// <summary>
	/// Indicates all assignments in this whole branch.
	/// </summary>
	public ReadOnlyMemory<AssignmentInfo> Assignments
	{
		get
		{
			var result = new List<AssignmentInfo>();
			for (var node = this; node is { Assignment: { } assignment }; node = node.Parent)
			{
				result.Add(assignment);
			}
			result.Reverse();
			return result.AsMemory();
		}
	}

	/// <summary>
	/// Indicates parent node.
	/// </summary>
	public DependencyNode? Parent { get; } = parent;


	/// <inheritdoc/>
	public override string ToString()
		=> string.Join(" -> ", from assignment in Assignments.Span select assignment.ToDebuggerDisplayString());

	/// <summary>
	/// Iterate on nodes of this branch, starting with the last node.
	/// </summary>
	/// <returns>The branch of nodes.</returns>
	public IEnumerable<DependencyNode> EnumerateAncestors()
	{
		for (var node = this; node?.Assignment is not null; node = node.Parent)
		{
			yield return node;
		}
	}
}
