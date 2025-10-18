namespace System.Linq.Iterators;

/// <summary>
/// Represents an enumerator that will be created after <see cref="SpanEnumerable.Append"/>.
/// </summary>
/// <typeparam name="T">The type of each element.</typeparam>
/// <param name="span">The span.</param>
/// <param name="value"><inheritdoc cref="_value" path="/summary"/></param>
/// <seealso cref="SpanEnumerable.Append"/>
public ref struct SpanAppendIterator<T>(ReadOnlySpan<T> span, T value) : IIterator<SpanAppendIterator<T>, T>
{
	/// <summary>
	/// The final element to be iterated.
	/// </summary>
	private readonly T _value = value;

	/// <summary>
	/// The span.
	/// </summary>
	private readonly ReadOnlySpan<T> _span = span;

	/// <summary>
	/// Indicates the index.
	/// </summary>
	private int _index = -1;


	/// <inheritdoc cref="IEnumerator{T}.Current"/>
	[UnscopedRef]
	public readonly ref readonly T Current => ref _index == _span.Length ? ref _value : ref _span[_index];

	/// <inheritdoc/>
	readonly object? IEnumerator.Current => Current;

	/// <inheritdoc/>
	readonly T IEnumerator<T>.Current => Current;


	/// <inheritdoc/>
	public bool MoveNext() => ++_index < _span.Length + 1;

	/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
	public readonly SpanAppendIterator<T> GetEnumerator() => this;


	/// <inheritdoc/>
	readonly void IDisposable.Dispose() { }

	/// <inheritdoc/>
	readonly void IEnumerator.Reset() => throw new NotImplementedException();

	/// <inheritdoc/>
	readonly IEnumerator IEnumerable.GetEnumerator() => ((T[])[.. _span, _value]).GetEnumerator();

	/// <inheritdoc/>
	readonly IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((T[])[.. _span, _value]).AsEnumerable().GetEnumerator();
}
