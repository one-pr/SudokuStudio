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
		where TSource : INumberBase<TSource>
		where TResult : INumberBase<TResult>
	{
		/// <inheritdoc cref="IAverageMethod{TSelf, TSource}.Average{TAccumulator, TResult}()"/>
		public TResult Average()
		{
			var sum = source.Sum();
			return TResult.CreateChecked(sum) / TResult.CreateChecked(source.Length);
		}
	}
}
