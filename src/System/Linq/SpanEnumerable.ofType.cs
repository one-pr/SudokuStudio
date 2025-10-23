namespace System.Linq;

public partial class SpanEnumerable
{
	/// <summary>
	/// Provides extension members on <see cref="ReadOnlySpan{T}"/> of <typeparamref name="TSource"/>.
	/// </summary>
	/// <typeparam name="TSource">The type of the elements of source.</typeparam>
	/// <typeparam name="TResult">The type of result.</typeparam>
	/// <param name="source">The collection to be used and checked.</param>
	extension<TSource, TResult>(ReadOnlySpan<TSource> source)
		where TSource : class
		where TResult : class?, TSource?
	{
		/// <inheritdoc cref="IOfTypeMethod{TSelf, TSource}.OfType{TResult}"/>
		public ReadOnlySpan<TResult> OfType()
		{
			var result = new TResult[source.Length];
			var i = 0;
			foreach (ref readonly var element in source)
			{
				if (element is TResult derived)
				{
					result[i++] = derived;
				}
			}
			return result.AsReadOnlySpan()[..i];
		}
	}
}
