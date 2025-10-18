namespace System.Text.RegularExpressions;

/// <summary>
/// Provides LINQ-based extension methods on <see cref="MatchCollection"/>, <see cref="GroupCollection"/> and <see cref="CaptureCollection"/>.
/// </summary>
/// <seealso cref="CaptureCollection"/>
/// <seealso cref="MatchCollection"/>
/// <seealso cref="GroupCollection"/>
public static class RegularExpressionCollectionEnumerable
{
	/// <summary>
	/// Provides extension members on <see cref="MatchCollection"/>.
	/// </summary>
	extension<TResult>(MatchCollection @this)
	{
		/// <inheritdoc cref="SpanEnumerable.Select{T, TResult}(ReadOnlySpan{T}, Func{T, TResult})"/>
		public ReadOnlySpan<TResult> Select(Func<Match, TResult> selector)
		{
			var result = new TResult[@this.Count];
			var i = 0;
			foreach (Match element in @this)
			{
				result[i++] = selector(element);
			}
			return result;
		}
	}

	/// <summary>
	/// Provides extension members on <see cref="GroupCollection"/>.
	/// </summary>
	extension<TResult>(GroupCollection @this)
	{
		/// <inheritdoc cref="SpanEnumerable.Select{T, TResult}(ReadOnlySpan{T}, Func{T, TResult})"/>
		public ReadOnlySpan<TResult> Select(Func<Group, TResult> selector)
		{
			var result = new TResult[@this.Count];
			var i = 0;
			foreach (Group element in @this)
			{
				result[i++] = selector(element);
			}
			return result;
		}
	}

	/// <summary>
	/// Provides extension members on <see cref="CaptureCollection"/>.
	/// </summary>
	extension<TResult>(CaptureCollection @this)
	{
		/// <inheritdoc cref="SpanEnumerable.Select{T, TResult}(ReadOnlySpan{T}, Func{T, TResult})"/>
		public ReadOnlySpan<TResult> Select(Func<Capture, TResult> selector)
		{
			var result = new TResult[@this.Count];
			var i = 0;
			foreach (Capture element in @this)
			{
				result[i++] = selector(element);
			}
			return result;
		}
	}
}
