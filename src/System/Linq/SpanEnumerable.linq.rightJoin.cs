namespace System.Linq;

public partial class SpanEnumerable
{
	/// <summary>
	/// Provides extension members on <see cref="ReadOnlySpan{T}"/> of <typeparamref name="TOuter"/>.
	/// </summary>
	/// <typeparam name="TOuter">The type of the elements of the first sequence.</typeparam>
	/// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
	/// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
	/// <typeparam name="TResult">The type of the result elements.</typeparam>
	/// <param name="outer">The first sequence to join.</param>
	extension<TOuter, TInner, TKey, TResult>(ReadOnlySpan<TOuter> outer) where TKey : notnull
	{
		/// <inheritdoc cref="IRightJoinMethod{TSelf, TSource}.RightJoin{TInner, TKey, TResult}(IEnumerable{TInner}, Func{TSource, TKey}, Func{TInner, TKey}, Func{TSource, TInner, TResult})"/>
		public ReadOnlySpan<TResult?> RightJoin(
			ReadOnlySpan<TInner> inner,
			Func<TOuter, TKey> outerKeySelector,
			Func<TInner, TKey> innerKeySelector,
			Func<TOuter?, TInner, TResult?> resultSelector
		) => RightJoin(outer, inner, outerKeySelector, innerKeySelector, resultSelector, null);

		/// <inheritdoc cref="IRightJoinMethod{TSelf, TSource}.RightJoin{TInner, TKey, TResult}(IEnumerable{TInner}, Func{TSource, TKey}, Func{TInner, TKey}, Func{TSource, TInner, TResult}, IEqualityComparer{TKey}?)"/>
		public ReadOnlySpan<TResult?> RightJoin(
			ReadOnlySpan<TInner> inner,
			Func<TOuter, TKey> outerKeySelector,
			Func<TInner, TKey> innerKeySelector,
			Func<TOuter?, TInner, TResult?> resultSelector,
			IEqualityComparer<TKey>? comparer
		)
		{
			comparer ??= EqualityComparer<TKey>.Default;

			var outerLookup = outer.ToLookup(outerKeySelector, comparer);
			var result = new List<TResult?>();
			foreach (ref readonly var innerElement in inner)
			{
				if (outerLookup[innerKeySelector(innerElement)] is var outerElements and not [])
				{
					foreach (ref readonly var outerElement in outerElements)
					{
						result.AddRef(resultSelector(outerElement, innerElement));
					}
				}
				else
				{
					result.AddRef(resultSelector(default, innerElement));
				}
			}
			return result.AsSpan();
		}
	}
}
