namespace Sudoku.Analytics.Dependency;

public partial class DependencyNode
{
	/// <summary>
	/// <para>
	/// Represents an enumerator type that can iterate on each node which is an ancestor node of the current node;
	/// the node itself will be iterated also.
	/// </para>
	/// <para>
	/// For example, if a sequence is <c>A -&gt; B -&gt; C -&gt; D -&gt; E</c>, the iteration will firstly produces
	/// <c>E</c>, and then <c>D</c>, <c>C</c>, <c>B</c> and finally <c>A</c>.
	/// </para>
	/// </summary>
	/// <param name="_node">The node to be checked.</param>
	public ref struct AncestorNodesEnumerator(DependencyNode? _node) : IEnumerable<DependencyNode>, IEnumerator<DependencyNode>
	{
		/// <inheritdoc/>
		public DependencyNode Current { get; private set; } = null!;

		/// <inheritdoc/>
		readonly object? IEnumerator.Current => Current;


		/// <inheritdoc/>
		public bool MoveNext()
		{
			if (_node is null)
			{
				return false;
			}

			if (Current is null)
			{
				Current = _node;
				return true;
			}

			Current = Current.Parent!;
			return Current is { Type: not DependencyNodeType.Root };
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
		private static IEnumerator<DependencyNode> GetEnumeratorDefaultImpl(DependencyNode? node)
		{
			if (node is null)
			{
				yield break;
			}

			for (var currentNode = node; currentNode?.Assignment is not null; currentNode = currentNode.Parent)
			{
				yield return currentNode;
			}
		}
	}
}
