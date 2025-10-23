namespace System.Linq;

public partial class SpanEnumerable
{
	/// <summary>
	/// Provides extension members on <see cref="ReadOnlySpan{T}"/> of <typeparamref name="TSource"/>.
	/// </summary>
	/// <typeparam name="TSource">The type of the elements of source.</typeparam>
	/// <typeparam name="TKey">The type of key.</typeparam>
	/// <param name="source">The collection to be used and checked.</param>
	extension<TSource, TKey>(ReadOnlySpan<TSource> source) where TKey : notnull
	{
		/// <inheritdoc cref="IGroupByMethod{TSelf, TSource}.GroupBy{TKey}(Func{TSource, TKey})"/>
		public ReadOnlySpan<SpanGrouping<TSource, TKey>> GroupBy(Func<TSource, TKey> keySelector)
		{
			var tempDictionary = new Dictionary<TKey, List<TSource>>(source.Length >> 2);
			foreach (var element in source)
			{
				var key = keySelector(element);
				if (!tempDictionary.TryAdd(key, [element]))
				{
					tempDictionary[key].AddRef(element);
				}
			}

			var result = new List<SpanGrouping<TSource, TKey>>(tempDictionary.Count);
			foreach (var key in tempDictionary.Keys)
			{
				var tempValues = tempDictionary[key];
				result.AddRef(new([.. tempValues], key));
			}
			return result.AsSpan();
		}

		/// <inheritdoc cref="IGroupByMethod{TSelf, TSource}.GroupBy{TKey, TElement}(Func{TSource, TKey}, Func{TSource, TElement})"/>
		/// <remarks>
		/// <include
		///     file="../../global-doc-comments.xml"
		///     path="g/csharp14/feature[@name='extension-container']/target[@name='generic-method']"/>
		/// </remarks>
		public ReadOnlySpan<SpanGrouping<TElement, TKey>> GroupBy<TElement>(
			Func<TSource, TKey> keySelector,
			Func<TSource, TElement> elementSelector
		)
		{
			var tempDictionary = new Dictionary<TKey, List<TSource>>(source.Length >> 2);
			foreach (var element in source)
			{
				var key = keySelector(element);
				if (!tempDictionary.TryAdd(key, [element]))
				{
					tempDictionary[key].AddRef(element);
				}
			}

			var result = new List<SpanGrouping<TElement, TKey>>(tempDictionary.Count);
			foreach (var key in tempDictionary.Keys)
			{
				var tempValues = tempDictionary[key];
				var valuesConverted = from value in tempValues select elementSelector(value);
				result.AddRef(new(valuesConverted.ToArray(), key));
			}
			return result.AsSpan();
		}
	}
}
