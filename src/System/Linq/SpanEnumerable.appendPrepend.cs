namespace System.Linq;

public partial class SpanEnumerable
{
	/// <summary>
	/// Provides extension members on <typeparamref name="TSource"/>[].
	/// </summary>
	extension<TSource>(ReadOnlySpan<TSource> source)
	{
		/// <inheritdoc cref="Enumerable.Append{TSource}(IEnumerable{TSource}, TSource)"/>
		public SpanAppendIterator<TSource> Append(TSource value) => new(source, value);

		/// <inheritdoc cref="Enumerable.Prepend{TSource}(IEnumerable{TSource}, TSource)"/>
		public SpanPrependIterator<TSource> Prepend(TSource value) => new(source, value);
	}
}
