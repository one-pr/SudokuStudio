namespace System.Linq;

public partial class SpanEnumerable
{
	/// <summary>
	/// Provides extension members on <see cref="ReadOnlySpan{T}"/> of <typeparamref name="TSource"/>.
	/// </summary>
	/// <typeparam name="TSource">The type of the elements of source.</typeparam>
	/// <param name="source">The collection to be used and checked.</param>
	extension<TSource>(ReadOnlySpan<TSource> source)
	{
		/// <inheritdoc cref="IOrderByMethod{TSelf, TSource}.OrderBy{TKey}(Func{TSource, TKey})"/>
		/// <remarks>
		/// <include
		///     file="../../global-doc-comments.xml"
		///     path="g/csharp14/feature[@name='extension-container']/target[@name='generic-method']"/>
		/// </remarks>
		public SpanOrderedEnumerable<TSource> OrderBy<TKey>(Func<TSource, TKey> selector)
			=> new(
				source,
				Array.Single<Func<TSource, TSource, int>>(
					(l, r) => (selector(l), selector(r)) switch
					{
						(IComparable<TKey> left, var right) => left.CompareTo(right),
						var (a, b) => Comparer<TKey>.Default.Compare(a, b)
					}
				)
			);

		/// <inheritdoc cref="IOrderByMethod{TSelf, TSource}.OrderByDescending{TKey}(Func{TSource, TKey})"/>
		/// <remarks>
		/// <include
		///     file="../../global-doc-comments.xml"
		///     path="g/csharp14/feature[@name='extension-container']/target[@name='generic-method']"/>
		/// </remarks>
		public SpanOrderedEnumerable<TSource> OrderByDescending<TKey>(Func<TSource, TKey> selector)
			=> new(
				source,
				Array.Single<Func<TSource, TSource, int>>(
					(l, r) => (selector(l), selector(r)) switch
					{
						(IComparable<TKey> left, var right) => -left.CompareTo(right),
						var (a, b) => -Comparer<TKey>.Default.Compare(a, b)
					}
				)
			);
	}
}
