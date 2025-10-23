namespace System.Linq;

public partial class SpanEnumerable
{
	/// <summary>
	/// Provides extension members on <see cref="ReadOnlySpan{T}"/> of <typeparamref name="TSource"/>.
	/// </summary>
	/// <typeparam name="TSource">The type of the elements of source.</typeparam>
	/// <typeparam name="TResult">The type of result.</typeparam>
	/// <param name="source">The collection to be used and checked.</param>
	extension<TSource, TResult>(scoped ReadOnlySpan<TSource> source)
	{
		/// <inheritdoc cref="ISelectMethod{TSelf, TSource}.Select{TResult}(Func{TSource, TResult})"/>
		public ReadOnlySpan<TResult> Select(Func<TSource, TResult> selector)
		{
			var result = new TResult[source.Length];
			var i = 0;
			foreach (var element in source)
			{
				result[i++] = selector(element);
			}
			return result;
		}

		/// <inheritdoc cref="ISelectMethod{TSelf, TSource}.Select{TResult}(Func{TSource, int, TResult})"/>
		public ReadOnlySpan<TResult> Select(Func<TSource, int, TResult> selector)
		{
			var result = new TResult[source.Length];
			var i = 0;
			foreach (var element in source)
			{
				result[i++] = selector(element, i);
			}
			return result;
		}
	}
}
