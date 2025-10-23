namespace System.Linq;

public partial class SpanEnumerable
{
	/// <summary>
	/// Provides extension members on <see cref="ReadOnlySpan{T}"/> of <typeparamref name="TSource"/>.
	/// </summary>
	/// <typeparam name="TSource">The type of the elements of source.</typeparam>
	/// <typeparam name="TDerived">The desired derived type.</typeparam>
	/// <param name="source">The collection to be used and checked.</param>
	extension<TSource, TDerived>(ReadOnlySpan<TSource> source)
		where TSource : class
		where TDerived : class, TSource
	{
		/// <inheritdoc cref="ICastMethod{TSelf, TSource}.Cast{TResult}"/>
		public ReadOnlySpan<TDerived> Cast()
		{
			var result = new TDerived[source.Length];
			var i = 0;
			foreach (ref readonly var element in source)
			{
				result[i++] = (TDerived)element;
			}
			return result;
		}
	}
}
