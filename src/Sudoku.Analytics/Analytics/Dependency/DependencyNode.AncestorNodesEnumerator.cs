namespace Sudoku.Analytics.Dependency;

public partial class DependencyNode
{
	/// <summary>
	/// Represents an enumerator type that can iterate on each node which is an ancestor node of the current node.
	/// </summary>
	/// <param name="_node">The node to be checked.</param>
	public ref struct AncestorNodesEnumerator(DependencyNode _node) : IEnumerable<DependencyNode>, IEnumerator<DependencyNode>
	{
		/// <inheritdoc/>
		public DependencyNode Current { get; private set; } = null!;

		/// <inheritdoc/>
		readonly object? IEnumerator.Current => Current;


		/// <inheritdoc/>
		public bool MoveNext()
		{
			if (Current is null)
			{
				Current = _node;
				return true;
			}

			Current = Current.Parent!;
			return Current?.Assignment is not null;
		}

		/// <inheritdoc/>
		readonly void IDisposable.Dispose()
		{
		}

		/// <inheritdoc/>
		[DoesNotReturn]
		readonly void IEnumerator.Reset() => throw new NotSupportedException();

		/// <inheritdoc/>
		readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumeratorDefaultImpl(_node);

		/// <inheritdoc/>
		readonly IEnumerator<DependencyNode> IEnumerable<DependencyNode>.GetEnumerator() => GetEnumeratorDefaultImpl(_node);


		/// <summary>
		/// The backing implementation of interface implementation methods.
		/// </summary>
		/// <param name="node">The current node.</param>
		/// <returns>An enumerator instance.</returns>
		private static List<DependencyNode>.Enumerator GetEnumeratorDefaultImpl(DependencyNode node)
		{
			var result = new List<DependencyNode>();
			for (var currentNode = node; currentNode?.Assignment is not null; currentNode = currentNode.Parent)
			{
				result.Add(currentNode);
			}
			return result.GetEnumerator();
		}
	}
}
