namespace System.Linq;

/// <summary>
/// Represents with LINQ methods for <see cref="List{T}"/>.
/// </summary>
/// <seealso cref="List{T}"/>
public static class ListEnumerable
{
	/// <summary>
	/// Provides extension members on <see cref="List{T}"/> of <typeparamref name="TSource"/>.
	/// </summary>
	extension<TSource>(List<TSource> @this)
	{
		/// <summary>
		/// Totals up the number of elements that satisfy the specified condition.
		/// </summary>
		/// <param name="predicate">The condition.</param>
		/// <returns>The number of elements satisfying the specified condition.</returns>
		public int Count(Func<TSource, bool> predicate)
		{
			var result = 0;
			foreach (var element in @this)
			{
				if (predicate(element))
				{
					result++;
				}
			}
			return result;
		}

		/// <inheritdoc cref="extension{TSource}(List{TSource}).Count(Func{TSource, bool})"/>
		public unsafe int CountUnsafe(delegate*<TSource, bool> predicate)
		{
			var result = 0;
			foreach (var element in @this)
			{
				if (predicate(element))
				{
					result++;
				}
			}
			return result;
		}

		/// <inheritdoc cref="Enumerable.Where{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>
		public ReadOnlySpan<TSource> Where(Func<TSource, bool> condition)
		{
			var result = new List<TSource>(@this.Count);
			foreach (var element in @this)
			{
				if (condition(element))
				{
					result.AddRef(element);
				}
			}
			return result.AsSpan();
		}

		/// <inheritdoc cref="Enumerable.Select{TSource, TResult}(IEnumerable{TSource}, Func{TSource, TResult})"/>
		/// <remarks>
		/// <include
		///     file="../../global-doc-comments.xml"
		///     path="g/csharp14/feature[@name='extension-container']/target[@name='generic-method']"/>
		/// </remarks>
		public ReadOnlySpan<TResult> Select<TResult>(Func<TSource, TResult> selector)
		{
			var result = new TResult[@this.Count];
			var i = 0;
			foreach (var element in @this)
			{
				result[i++] = selector(element);
			}
			return result;
		}

		/// <inheritdoc cref="Enumerable.SelectMany{TSource, TCollection, TResult}(IEnumerable{TSource}, Func{TSource, IEnumerable{TCollection}}, Func{TSource, TCollection, TResult})"/>
		/// <remarks>
		/// <include
		///     file="../../global-doc-comments.xml"
		///     path="g/csharp14/feature[@name='extension-container']/target[@name='generic-method']"/>
		/// </remarks>
		public ReadOnlySpan<TResult> SelectMany<TCollection, TResult>(
			Func<TSource, ReadOnlySpan<TCollection>> collectionSelector,
			Func<TSource, TCollection, TResult> resultSelector
		)
		{
			var result = new List<TResult>(@this.Count << 1);
			foreach (var element in @this)
			{
				foreach (ref readonly var collectionElement in collectionSelector(element))
				{
					result.AddRef(resultSelector(element, collectionElement));
				}
			}
			return result.AsSpan();
		}

		/// <inheritdoc cref="Enumerable.SelectMany{TSource, TCollection, TResult}(IEnumerable{TSource}, Func{TSource, IEnumerable{TCollection}}, Func{TSource, TCollection, TResult})"/>
		/// <remarks>
		/// <include
		///     file="../../global-doc-comments.xml"
		///     path="g/csharp14/feature[@name='extension-container']/target[@name='generic-method']"/>
		/// </remarks>
		public ReadOnlySpan<TResult> SelectMany<TCollection, TResult>(
			Func<TSource, IEnumerable<TCollection>> collectionSelector,
			Func<TSource, TCollection, TResult> resultSelector
		)
		{
			var result = new List<TResult>(@this.Count << 1);
			foreach (var element in @this)
			{
				foreach (var collectionElement in collectionSelector(element))
				{
					result.AddRef(resultSelector(element, collectionElement));
				}
			}
			return result.AsSpan();
		}
	}

	/// <summary>
	/// Provides extension members on <see cref="List{T}"/>,
	/// where <typeparamref name="T"/> satisfies <see cref="IAdditiveIdentity{TSelf, TResult}"/>, <see cref="IAdditionOperators{TSelf, TOther, TResult}"/> constraints.
	/// </summary>
	extension<T>(List<T> @this) where T : IAdditiveIdentity<T, T>, IAdditionOperators<T, T, T>
	{
		/// <inheritdoc cref="Enumerable.Sum(IEnumerable{int})"/>
		public T Sum()
		{
			var result = T.AdditiveIdentity;
			foreach (ref readonly var element in @this.AsSpan())
			{
				result += element;
			}
			return result;
		}
	}
}
