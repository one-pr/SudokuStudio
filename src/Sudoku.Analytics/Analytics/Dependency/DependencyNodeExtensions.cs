namespace Sudoku.Analytics.Dependency;

/// <summary>
/// Provides with extension methods on <see cref="DependencyNode"/>.
/// </summary>
/// <seealso cref="DependencyNode"/>
public static class DependencyNodeExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="DependencyNode"/>.
	/// </summary>
	/// <param name="this">The current instance.</param>
	extension(DependencyNode @this)
	{
		/// <summary>
		/// Indicates all assignments and their own types in this whole branch.
		/// </summary>
		public ReadOnlyMemory<(AssignmentInfo Assignment, DependencyNodeType Type)> AssignmentsWithType
		{
			get
			{
				var result = new List<(AssignmentInfo, DependencyNodeType)>();
				for (var node = @this; node is { Assignment: { } assignment, Type: var type }; node = node.Parent)
				{
					result.Add((assignment, type));
				}
				result.Reverse();
				return result.AsMemory();
			}
		}
	}
}
