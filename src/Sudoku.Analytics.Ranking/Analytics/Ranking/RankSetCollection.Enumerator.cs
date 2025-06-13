namespace Sudoku.Analytics.Ranking;

public partial class RankSetCollection
{
	/// <summary>
	/// Represents an enumerator object.
	/// </summary>
	/// <param name="_set">Indicates the set.</param>
	public ref struct Enumerator(SortedSet<RankSet> _set) :
		IEnumerator<RankSet>,
		IEnumerable<RankSet>
	{
		/// <summary>
		/// Indicates the backing enumerator.
		/// </summary>
		private SortedSet<RankSet>.Enumerator _enumerator = _set.GetEnumerator();


		/// <inheritdoc/>
		public readonly RankSet Current => _enumerator.Current;

		/// <inheritdoc/>
		readonly object IEnumerator.Current => Current;


		/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
		public readonly Enumerator GetEnumerator() => this;

		/// <inheritdoc/>
		public bool MoveNext() => _enumerator.MoveNext();

		/// <inheritdoc/>
		readonly void IDisposable.Dispose()
		{
		}

		/// <inheritdoc/>
		[DoesNotReturn]
		readonly void IEnumerator.Reset() => throw new NotSupportedException();

		/// <inheritdoc/>
		readonly IEnumerator IEnumerable.GetEnumerator() => _set.GetEnumerator();

		/// <inheritdoc/>
		readonly IEnumerator<RankSet> IEnumerable<RankSet>.GetEnumerator() => _set.GetEnumerator();
	}
}
