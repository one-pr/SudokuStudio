namespace Sudoku.Analytics.Dependency;

/// <summary>
/// Represents a dependency node.
/// </summary>
/// <param name="type"><inheritdoc cref="Type" path="/summary"/></param>
/// <param name="grid"><inheritdoc cref="Grid" path="/summary"/></param>
/// <param name="assignment"><inheritdoc cref="Assignment" path="/summary"/></param>
/// <param name="siblingAssignments"><inheritdoc cref="SiblingAssignments" path="/summary"/></param>
/// <param name="parent"><inheritdoc cref="Parent" path="/summary"/></param>
public sealed partial class DependencyNode(
	DependencyNodeType type,
	in Grid grid,
	DependencyAssignment assignment,
	ReadOnlyMemory<DependencyAssignment> siblingAssignments,
	DependencyNode? parent
) :
	IEquatable<DependencyNode>,
	IEqualityOperators<DependencyNode, DependencyNode, bool>
{
	/// <summary>
	/// The backing field of <see cref="Grid"/>.
	/// </summary>
	private readonly Grid _grid = grid;


	/// <summary>
	/// Indicates the depth of the node.
	/// </summary>
	public int Depth
	{
		get
		{
			var result = 0;
			foreach (var _ in EnumerateAncestors(true))
			{
				result++;
			}
			return result;
		}
	}

	/// <summary>
	/// Indicates the type.
	/// </summary>
	public DependencyNodeType Type { get; } = type;

	/// <summary>
	/// Indicates the grid to be used.
	/// </summary>
	public ref readonly Grid Grid => ref _grid;

	/// <summary>
	/// Indicates the truth of the current node. If invalid, <see cref="Space.InvalidSpace"/> will be returned.
	/// </summary>
	/// <seealso cref="Space.InvalidSpace"/>
	public Space Truth
	{
		get
		{
			var (digit, cells) = Assignment;
			return Type switch
			{
				DependencyNodeType.Block when cells.SharedBlock is var sharedBlock and not FallbackConstants.@int
					=> Space.BlockDigit(sharedBlock, digit),
				DependencyNodeType.Row or DependencyNodeType.Column
					when cells.SharedHouses is var sharedLineMask and not 0
						&& (Type == DependencyNodeType.Row ? AllRowsMask : AllColumnsMask) is var lineMask
						&& BitOperations.TrailingZeroCount(sharedLineMask & lineMask) is var sharedLine
					=> sharedLine < 18 ? Space.RowDigit(sharedLine - 9, digit) : Space.ColumnDigit(sharedLine - 18, digit),
				DependencyNodeType.Cell when cells is [var onlyCell]
					=> Space.RowColumn(onlyCell / 9, onlyCell % 9),
				DependencyNodeType.Root or DependencyNodeType.Supposing
					or DependencyNodeType.Block or DependencyNodeType.Row or DependencyNodeType.Column or DependencyNodeType.Cell
					=> Space.InvalidSpace
			};
		}
	}

	/// <summary>
	/// Indicates the truths.
	/// </summary>
	public SpaceSet Truths
	{
		get
		{
			var result = SpaceSet.Empty;
			foreach (var ancestor in EnumerateAncestors(true))
			{
				result += ancestor.Truth;
			}
			return result;
		}
	}

	/// <summary>
	/// Indicates the current assignment.
	/// </summary>
	public DependencyAssignment Assignment { get; } = assignment;

	/// <summary>
	/// Indicates sibling assignments.
	/// </summary>
	public ReadOnlyMemory<DependencyAssignment> SiblingAssignments { get; } = siblingAssignments;

	/// <summary>
	/// Indicates all assignments in this whole branch.
	/// </summary>
	public ReadOnlyMemory<DependencyAssignment> Assignments
	{
		get
		{
			var result = new Stack<DependencyAssignment>();
			foreach (var node in EnumerateAncestors(true))
			{
				result.Push(node.Assignment);
			}
			return result.ToArray();
		}
	}

	/// <summary>
	/// Indicates parent node.
	/// </summary>
	public DependencyNode? Parent { get; } = parent;

	/// <summary>
	/// Indicates root node.
	/// </summary>
	public DependencyNode Root
	{
		get
		{
			var result = this;
			for (; result!.Type != DependencyNodeType.Root; result = result.Parent) ;
			return result;
		}
	}


	/// <inheritdoc/>
	public override bool Equals(object? obj) => Equals(obj as DependencyNode);

	/// <inheritdoc/>
	public bool Equals([NotNullWhen(true)] DependencyNode? other) => Equals(other, DependencyNodeComparison.Default);

	/// <summary>
	/// Indicates whether the current instance is equal to another object of the same type,
	/// under the specified option to control equality comparison rule.
	/// </summary>
	/// <param name="other">The other instance to be compared.</param>
	/// <param name="option">The option.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public bool Equals([NotNullWhen(true)] DependencyNode? other, DependencyNodeComparison option)
	{
		switch (option)
		{
			default:
			{
				throw new ArgumentOutOfRangeException(nameof(option));
			}
			case DependencyNodeComparison.Default:
			{
				return e(this, other);
			}
			case DependencyNodeComparison.AllAncestors:
			{
				var leftNode = this;
				var rightNode = other;
				while ((leftNode, rightNode) is (not null, not null))
				{
					if (!e(leftNode, rightNode))
					{
						return false;
					}

					leftNode = leftNode.Parent;
					rightNode = rightNode.Parent;
				}
				return (leftNode, rightNode) is (null, null);
			}
		}


		static bool e(DependencyNode left, [NotNullWhen(true)] DependencyNode? right)
			=> right is not null
			&& left.Type == right.Type && left._grid == right._grid
			&& left.Assignment == right.Assignment
			&& left.Parent is not null && right.Parent is not null;
	}

	/// <inheritdoc/>
	public override int GetHashCode() => GetHashCode(DependencyNodeComparison.Default);

	/// <summary>
	/// Serves as the default hash code function, using the specified option to control equality comparison rule.
	/// </summary>
	/// <param name="option">The option.</param>
	/// <returns>A hash code for the current object.</returns>
	public int GetHashCode(DependencyNodeComparison option)
	{
		switch (option)
		{
			default:
			{
				throw new ArgumentOutOfRangeException(nameof(option));
			}
			case DependencyNodeComparison.Default:
			{
				return h(this);
			}
			case DependencyNodeComparison.AllAncestors:
			{
				var hashCode = new HashCode();
				for (var node = this; node is { Type: DependencyNodeType.Supposing }; node = node.Parent)
				{
					hashCode.Add(h(node));
				}
				return hashCode.ToHashCode();
			}
		}


		static int h(DependencyNode instance)
			=> HashCode.Combine(
				instance.Type,
				instance.Grid.GetHashCode(),
				instance.Assignment,
				instance.Parent is null ? 19 : 131731
			);
	}

	/// <inheritdoc/>
	public override string ToString()
		=> Type == DependencyNodeType.Root
			? "<root>"
			: string.Join(" -> ", from assignment in Assignments.Span select assignment.ToCandidateFormatString(false));

	/// <summary>
	/// Iterate on nodes of this branch, specifying a <see cref="bool"/> variable indicating
	/// whether iteration operation includes the current instance as result or not.
	/// </summary>
	/// <param name="includingSelf">
	/// <para>
	/// Indicates whether <see langword="this"/> will be included to be iterated;
	/// if <see langword="false"/>, the iteration will start with the parent node of the current instance.
	/// </para>
	/// <para>By default it's <see langword="false"/>.</para>
	/// </param>
	/// <returns>The branch of nodes using an enumerator.</returns>
	public AncestorNodesEnumerator EnumerateAncestors(bool includingSelf = false) => new(includingSelf ? this : Parent);


	/// <inheritdoc/>
	public static bool operator ==(DependencyNode? left, DependencyNode? right)
		=> (left, right) switch { (null, null) => true, (not null, not null) => left.Equals(right), _ => false };

	/// <inheritdoc/>
	public static bool operator !=(DependencyNode? left, DependencyNode? right) => !(left == right);
}
