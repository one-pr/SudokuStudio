namespace Sudoku.Concepts;

public partial struct SegmentCollection
{
	/// <summary>
	/// Represents an enumerator of the current type.
	/// </summary>
	/// <param name="value">The value.</param>
	public ref struct Enumerator(scoped ref readonly SegmentCollection value) : IEnumerator<Segment>
	{
		/// <summary>
		/// Indicates the mask of the value.
		/// </summary>
		private readonly long _mask = value._mask;

		/// <summary>
		/// Indicates the index.
		/// </summary>
		private int _index = -1;


		/// <inheritdoc/>
		public readonly Segment Current => (Segment)_index;

		/// <inheritdoc/>
		readonly object IEnumerator.Current => Current;


		/// <inheritdoc/>
		public bool MoveNext()
		{
			while (++_index < 54)
			{
				if ((_mask >> _index & 1) != 0)
				{
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
		readonly void IEnumerator.Reset() => throw new NotSupportedException();
	}
}
