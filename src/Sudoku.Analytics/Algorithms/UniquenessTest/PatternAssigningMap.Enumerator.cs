namespace Sudoku.Algorithms.UniquenessTest;

public partial class PatternAssigningMap
{
	/// <summary>
	/// Represents the backing enumerator instance.
	/// </summary>
	/// <param name="_dictionary">The backing dictionary.</param>
	public ref struct Enumerator(Dictionary<Cell, Mask> _dictionary) : IEnumerator<KeyValuePair<Cell, Mask>>
	{
		/// <summary>
		/// Indicates the backing enumerator.
		/// </summary>
		private Dictionary<Cell, Mask>.Enumerator _enumerator = _dictionary.GetEnumerator();


		/// <inheritdoc/>
		public readonly KeyValuePair<Cell, Mask> Current => _enumerator.Current;

		/// <inheritdoc/>
		readonly object IEnumerator.Current => Current;


		/// <inheritdoc/>
		public bool MoveNext() => _enumerator.MoveNext();

		/// <inheritdoc/>
		readonly void IDisposable.Dispose() { }

		/// <inheritdoc/>
		[DoesNotReturn]
		readonly void IEnumerator.Reset() => throw new NotImplementedException();
	}
}
