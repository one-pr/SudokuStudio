namespace System.Linq;

public partial class SpanEnumerable
{
	/// <summary>
	/// Provides extension members on <typeparamref name="TSource"/>[].
	/// </summary>
	/// <typeparam name="TSource">The type of each element.</typeparam>
	/// <param name="source">The array to be filtered.</param>
	extension<TSource>(ReadOnlySpan<TSource> source)
	{
		/// <summary>
		/// Filters duplicate items from an array.
		/// </summary>
		/// <returns>A new array of elements that doesn't contain any duplicate items.</returns>
		public ReadOnlySpan<TSource> Distinct()
		{
			if (source.Length == 0)
			{
				return [];
			}

			var tempSet = new HashSet<TSource>(source.Length, EqualityComparer<TSource>.Default);
			var result = new TSource[source.Length];
			var i = 0;
			foreach (var element in source)
			{
				if (tempSet.Add(element))
				{
					result[i++] = element;
				}
			}
			return result.AsReadOnlySpan()[..i];
		}

		/// <inheritdoc cref="Enumerable.DistinctBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})"/>
		/// <remarks>
		/// <include
		///     file="../../global-doc-comments.xml"
		///     path="g/csharp14/feature[@name='extension-container']/target[@name='generic-method']"/>
		/// </remarks>
		public ReadOnlySpan<TSource> DistinctBy<TKey>(Func<TSource, TKey> keySelector)
			where TKey : notnull, IEqualityOperators<TKey, TKey, bool>
		{
			var result = new TSource[source.Length];
			var i = 0;
			foreach (var element in source)
			{
				if (i == 0)
				{
					result[i++] = element;
				}
				else
				{
					var elementKey = keySelector(element);
					var contains = false;
					foreach (var recordedElement in result)
					{
						var recordedElementKey = keySelector(recordedElement);
						if (elementKey == recordedElementKey)
						{
							contains = true;
							break;
						}
					}
					if (!contains)
					{
						result[i++] = element;
					}
				}
			}
			return result.AsReadOnlySpan()[..i];
		}

		/// <inheritdoc cref="Enumerable.DistinctBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey}, IEqualityComparer{TKey})"/>
		/// <remarks>
		/// <include
		///     file="../../global-doc-comments.xml"
		///     path="g/csharp14/feature[@name='extension-container']/target[@name='generic-method']"/>
		/// </remarks>
		public ReadOnlySpan<TSource> DistinctBy<TKey>(Func<TSource, TKey> keySelector, IEqualityComparer<TKey> equalityComparer)
			where TKey : notnull
		{
			var result = new TSource[source.Length];
			var i = 0;
			foreach (var element in source)
			{
				if (i == 0)
				{
					result[i++] = element;
				}
				else
				{
					var elementKey = keySelector(element);
					var hashCodeThis = equalityComparer.GetHashCode(elementKey);

					var contains = false;
					foreach (ref readonly var recordedElement in result.AsReadOnlySpan()[..i])
					{
						var recordedElementKey = keySelector(recordedElement);
						var hashCodeOther = equalityComparer.GetHashCode(recordedElementKey);
						if (hashCodeThis == hashCodeOther && equalityComparer.Equals(elementKey, recordedElementKey))
						{
							contains = true;
							break;
						}
					}
					if (!contains)
					{
						result[i++] = element;
					}
				}
			}
			return result.AsReadOnlySpan()[..i];
		}
	}
}
