namespace System.Linq;

/// <summary>
/// Represents equivalent <see cref="IGrouping{TKey, TElement}"/> implementation
/// on customized sequence-based types (like array and span).
/// </summary>
/// <typeparam name="TSelf"><include file="../../global-doc-comments.xml" path="/g/self-type-constraint"/></typeparam>
/// <typeparam name="TKey">Indicates the type of keys that group values.</typeparam>
/// <typeparam name="TElement">The type of each element.</typeparam>
internal interface ISequenceGrouping<TSelf, out TKey, TElement> :
	IEnumerable<TElement>,
	IEqualityOperators<TSelf, TSelf, bool>,
	IEquatable<TSelf>,
	IGrouping<TKey, TElement>,
	IReadOnlyCollection<TElement>,
	ISelectMethod<TSelf, TElement>,
	IWhereMethod<TSelf, TElement>
	where TSelf : ISequenceGrouping<TSelf, TKey, TElement>
	where TKey : notnull
{
	/// <summary>
	/// Indicates the backing elements.
	/// </summary>
	protected ReadOnlySpan<TElement> Elements { get; }

	/// <inheritdoc/>
	int IReadOnlyCollection<TElement>.Count => Elements.Length;


	/// <summary>
	/// Gets the element at the specified index.
	/// </summary>
	/// <param name="index">The desired index.</param>
	/// <returns>The reference to the element at the specified index.</returns>
	ref readonly TElement this[int index] { get; }


	/// <inheritdoc cref="ReadOnlySpan{T}.GetPinnableReference"/>
	ref readonly TElement GetPinnableReference();

	/// <summary>
	/// Creates an enumerator that can enumerate each element in the source collection.
	/// </summary>
	/// <returns>An enumerator instance.</returns>
	new AnonymousSpanEnumerator<TElement> GetEnumerator();

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => Elements.ToArray().GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator() => Elements.ToArray().AsEnumerable().GetEnumerator();
}
