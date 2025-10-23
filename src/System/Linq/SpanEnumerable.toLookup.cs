namespace System.Linq;

public partial class SpanEnumerable
{
	/// <summary>
	/// Provides extension members on <see cref="ReadOnlySpan{T}"/> of <typeparamref name="TSource"/>.
	/// </summary>
	/// <typeparam name="TSource">The type of source elements.</typeparam>
	/// <typeparam name="TKey">The type of key.</typeparam>
	/// <param name="source">The source.</param>
	extension<TSource, TKey>(ReadOnlySpan<TSource> source) where TKey : notnull
	{
		/// <inheritdoc cref="Enumerable.ToLookup{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})"/>
		public ValueLookup<TKey, TSource> ToLookup(Func<TSource, TKey> keySelector)
			=> source.ToLookup(keySelector, Func<TSource>.Self, null);

		/// <inheritdoc cref="Enumerable.ToLookup{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey}, IEqualityComparer{TKey}?)"/>
		public ValueLookup<TKey, TSource> ToLookup(Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
			=> source.ToLookup(keySelector, Func<TSource>.Self, comparer);

		/// <inheritdoc cref="Enumerable.ToLookup{TSource, TKey, TElement}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TSource, TElement})"/>
		/// <remarks>
		/// <include
		///     file="../../global-doc-comments.xml"
		///     path="g/csharp14/feature[@name='extension-container']/target[@name='generic-method']"/>
		/// </remarks>
		public ValueLookup<TKey, TElement> ToLookup<TElement>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
			=> source.ToLookup(keySelector, elementSelector, null);

		/// <inheritdoc cref="Enumerable.ToLookup{TSource, TKey, TElement}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TSource, TElement}, IEqualityComparer{TKey}?)"/>
		/// <remarks>
		/// <include
		///     file="../../global-doc-comments.xml"
		///     path="g/csharp14/feature[@name='extension-container']/target[@name='generic-method']"/>
		/// </remarks>
		public ValueLookup<TKey, TElement> ToLookup<TElement>(
			Func<TSource, TKey> keySelector,
			Func<TSource, TElement> elementSelector,
			IEqualityComparer<TKey>? comparer
		)
		{
			var dictionary = new Dictionary<TKey, List<TElement>>(comparer);
			foreach (var sourceElement in source)
			{
				var key = keySelector(sourceElement);
				var element = elementSelector(sourceElement);
				if (!dictionary.TryAdd(key, [element]))
				{
					dictionary[key].AddRef(element);
				}
			}
			return new(dictionary.ToDictionary(static kvp => kvp.Key, static kvp => kvp.Value.ToArray()));
		}
	}
}
