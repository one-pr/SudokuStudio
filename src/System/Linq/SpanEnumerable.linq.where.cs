namespace System.Linq;

public partial class SpanEnumerable
{
	/// <inheritdoc cref="IWhereMethod{TSelf, TSource}.Where(Func{TSource, bool})"/>
	public static ReadOnlySpan<TSource> Where<TSource>(this ReadOnlySpan<TSource> @this, Func<TSource, bool> predicate)
	{
		var result = new TSource[@this.Length];
		var i = 0;
		foreach (var element in @this)
		{
			if (predicate(element))
			{
				result[i++] = element;
			}
		}
		return result.AsReadOnlySpan()[..i];
	}

	/// <inheritdoc cref="IWhereMethod{TSelf, TSource}.Where(Func{TSource, int, bool})"/>
	public static ReadOnlySpan<TSource> Where<TSource>(this ReadOnlySpan<TSource> @this, Func<TSource, int, bool> predicate)
	{
		var result = new TSource[@this.Length];
		var i = 0;
		for (var j = 0; j < @this.Length; j++)
		{
			if (predicate(@this[j], j))
			{
				result[i++] = @this[j];
			}
		}
		return result.AsReadOnlySpan()[..i];
	}
}
