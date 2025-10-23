namespace System.Linq;

public partial class SpanEnumerable
{
	/// <summary>
	/// Provides extension members on <see cref="ReadOnlySpan{T}"/> of <typeparamref name="TSource"/>.
	/// </summary>
	/// <typeparam name="TSource">The type of the elements of source.</typeparam>
	/// <typeparam name="TKey">The type of key.</typeparam>
	/// <param name="source">The collection to be used and checked.</param>
	extension<TSource, TKey>(ReadOnlySpan<TSource> source)
	{
		/// <inheritdoc cref="IOrderByMethod{TSelf, TSource}.OrderBy{TKey}(Func{TSource, TKey})"/>
		public SpanOrderedEnumerable<TSource> OrderBy(Func<TSource, TKey> selector)
			=> new(
				source,
				Array.Single(
					(TSource l, TSource r) => (selector(l), selector(r)) switch
					{
						(IComparable<TKey> left, var right) => left.CompareTo(right),
						var (a, b) => Comparer<TKey>.Default.Compare(a, b)
					}
				)
			);

		/// <inheritdoc cref="IOrderByMethod{TSelf, TSource}.OrderByDescending{TKey}(Func{TSource, TKey})"/>
		public SpanOrderedEnumerable<TSource> OrderByDescending(Func<TSource, TKey> selector)
			=> new(
				source,
				Array.Single(
					(TSource l, TSource r) => (selector(l), selector(r)) switch
					{
						(IComparable<TKey> left, var right) => -left.CompareTo(right),
						var (a, b) => -Comparer<TKey>.Default.Compare(a, b)
					}
				)
			);
	}
}
