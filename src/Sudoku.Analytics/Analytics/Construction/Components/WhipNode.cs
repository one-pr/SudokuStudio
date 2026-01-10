namespace Sudoku.Analytics.Construction.Components;

/// <summary>
/// Represents a whip node.
/// </summary>
/// <param name="assignment"><inheritdoc cref="Assignment" path="/summary"/></param>
/// <param name="availableAssignments"><inheritdoc cref="AvailableAssignments" path="/summary"/></param>
/// <param name="parent"><inheritdoc cref="Parent" path="/summary"/></param>
public sealed class WhipNode(WhipAssignment assignment, ReadOnlyMemory<WhipAssignment> availableAssignments, WhipNode? parent = null) :
	IParentLinkedNode<WhipNode>
{
	/// <summary>
	/// Initializes a <see cref="WhipNode"/> with no next assignments.
	/// </summary>
	/// <param name="assignment">Indicates assignments.</param>
	public WhipNode(WhipAssignment assignment) : this(assignment, ReadOnlyMemory<WhipAssignment>.Empty)
	{
	}


	/// <summary>
	/// Indicates the available assignments.
	/// </summary>
	public ReadOnlyMemory<WhipAssignment> AvailableAssignments { get; } = availableAssignments;

	/// <summary>
	/// Indicates the assignment conclusion.
	/// </summary>
	public WhipAssignment Assignment { get; } = assignment;

	/// <inheritdoc/>
	public WhipNode Root
	{
		get
		{
			var (result, p) = (this, Parent);
			while (p is not null)
			{
				_ = (result = p, p = p.Parent);
			}
			return result;
		}
	}

	/// <summary>
	/// Indicates the parent node.
	/// </summary>
	public WhipNode? Parent { get; } = parent;

	/// <inheritdoc/>
	ComponentType IComponent.Type => ComponentType.WhipNode;


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj) => Equals(obj as WhipNode);

	/// <inheritdoc/>
	public bool Equals([NotNullWhen(true)] WhipNode? other) => other is not null && Assignment == other.Assignment;

	/// <inheritdoc/>
	public override int GetHashCode() => Assignment.GetHashCode();

	/// <inheritdoc/>
	public override string ToString() => ToString(CoordinateConverter.InvariantCulture);

	/// <inheritdoc/>
	public string ToString(CultureInfo culture) => ToString(CoordinateConverter.GetInstance(culture));

	/// <inheritdoc/>
	public string ToString(CoordinateConverter converter)
	{
		var parentString = Parent is { Assignment: var assignment } ? converter.CandidateConverter(assignment.Map) : "<null>";
		return $$"""{{nameof(WhipNode)}} { {{nameof(Assignment)}} = {{Assignment}}, {{nameof(Parent)}} = {{parentString}} }""";
	}

	/// <inheritdoc/>
	public string ToString(ICandidateMapConverter converter) => ToString(converter, null);

	/// <inheritdoc/>
	public string ToString(ICandidateMapConverter converter, IFormatProvider? formatProvider)
	{
		var parentString = Parent is { Assignment.Map: var map }
			? converter.TryFormat(in map, formatProvider, out var r)
				? r
				: throw new FormatException()
			: "<null>";
		return $$"""{{nameof(WhipNode)}} { {{nameof(Assignment)}} = {{Assignment}}, {{nameof(Parent)}} = {{parentString}} }""";
	}


	/// <summary>
	/// Creates a <see cref="WhipNode"/> instance with parent node.
	/// </summary>
	/// <param name="current">The current node.</param>
	/// <param name="parent">The parent node.</param>
	/// <returns>The new node created.</returns>
	public static WhipNode Create(WhipNode current, WhipNode? parent)
		=> new(current.Assignment, current.AvailableAssignments, parent);


	/// <inheritdoc/>
	public static bool operator ==(WhipNode? left, WhipNode? right)
		=> (left, right) switch { (null, null) => true, (not null, not null) => left.Equals(right), _ => false };

	/// <inheritdoc/>
	public static bool operator !=(WhipNode? left, WhipNode? right) => !(left == right);
}
