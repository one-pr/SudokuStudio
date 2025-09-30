namespace System;

/// <summary>
/// Represents an enumerator type that iterates elements in a range.
/// </summary>
public ref struct RangeEnumerator : IEnumerator<int>
{
	/// <summary>
	/// Indicates start index.
	/// </summary>
	private readonly int _start;

	/// <summary>
	/// Indicates end index.
	/// </summary>
	private readonly int _end;


	/// <summary>
	/// Initializes a <see cref="RangeEnumerator"/> instance via specified range.
	/// </summary>
	/// <param name="range">The range.</param>
	/// <exception cref="ArgumentOutOfRangeException">Throws when <paramref name="range"/> has from-end index.</exception>
	public RangeEnumerator(in Range range)
	{
		var start = range.Start is (var startValue, false) ? startValue : throw new ArgumentOutOfRangeException(nameof(range));
		var end = range.End is (var endValue, false) ? endValue : throw new ArgumentOutOfRangeException(nameof(range));
		Current = _start = start <= end ? start - 1 : start + 1;
		_end = end;
	}


	/// <inheritdoc/>
	public int Current { get; private set; }

	/// <inheritdoc/>
	readonly object IEnumerator.Current => Current;


	/// <inheritdoc/>
	public bool MoveNext() => _start <= _end ? ++Current < _end : --Current > _end;

	/// <inheritdoc/>
	readonly void IDisposable.Dispose() { }

	/// <inheritdoc/>
	[DoesNotReturn]
	readonly void IEnumerator.Reset() => throw new NotImplementedException();
}
