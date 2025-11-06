namespace Sudoku.Concepts;

public partial class ConclusionSet
{
	/// <summary>
	/// Represents an enumerator object that can iterate conclusions of the parent type.
	/// </summary>
	/// <param name="_bitArray">The backing bit array.</param>
	/// <param name="startIndex">The start index.</param>
	/// <param name="_endIndexExcluded">The end index, excluded.</param>
	public ref struct Enumerator(BitArray _bitArray, int startIndex, int _endIndexExcluded) :
		IEnumerable<Conclusion>,
		IEnumerator<Conclusion>
	{
		/// <summary>
		/// Indicates the start index.
		/// </summary>
		private readonly int _startIndex = startIndex;

		/// <summary>
		/// Indicates backing index.
		/// </summary>
		private int _index = startIndex - 1;


		/// <inheritdoc/>
		public Conclusion Current { get; private set; }

		/// <inheritdoc/>
		readonly object IEnumerator.Current => Current;


		/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
		public readonly Enumerator GetEnumerator() => this;

		/// <inheritdoc/>
		public bool MoveNext()
		{
			for (var i = _index + 1; i < _endIndexExcluded; i++)
			{
				if (_bitArray[i])
				{
					Current = new((ConclusionType)(i / HalfBitsCount), i % HalfBitsCount);
					_index = i;
					return true;
				}
			}
			return false;
		}

		/// <inheritdoc/>
		readonly void IDisposable.Dispose()
		{
		}

		/// <inheritdoc/>
		[DoesNotReturn]
		readonly void IEnumerator.Reset() => throw new NotImplementedException();

		/// <inheritdoc/>
		readonly IEnumerator IEnumerable.GetEnumerator()
		{
			var result = new List<Conclusion>();
			for (var i = _startIndex; i < _endIndexExcluded; i++)
			{
				if (_bitArray[i])
				{
					result.Add(new((ConclusionType)(i / HalfBitsCount), i % HalfBitsCount));
				}
			}
			return result.GetEnumerator();
		}

		/// <inheritdoc/>
		readonly IEnumerator<Conclusion> IEnumerable<Conclusion>.GetEnumerator()
		{
			var result = new List<Conclusion>();
			for (var i = _startIndex; i < _endIndexExcluded; i++)
			{
				if (_bitArray[i])
				{
					result.Add(new((ConclusionType)(i / HalfBitsCount), i % HalfBitsCount));
				}
			}
			return result.GetEnumerator();
		}
	}
}
