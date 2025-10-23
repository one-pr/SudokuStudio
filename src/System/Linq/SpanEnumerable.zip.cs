namespace System.Linq;

public partial class SpanEnumerable
{
	/// <summary>
	/// Provides extension members on <see cref="ReadOnlySpan{T}"/> of <typeparamref name="TFirst"/>.
	/// </summary>
	/// <typeparam name="TFirst">The type of the first source elements.</typeparam>
	/// <typeparam name="TSecond">The type of the second source elements.</typeparam>
	/// <param name="first">The first sequence.</param>
	extension<TFirst, TSecond>(ReadOnlySpan<TFirst> first)
	{
		/// <inheritdoc cref="Enumerable.Zip{TFirst, TSecond}(IEnumerable{TFirst}, IEnumerable{TSecond})"/>
		public ReadOnlySpan<(TFirst Left, TSecond Right)> Zip(ReadOnlySpan<TSecond> second)
		{
			ArgumentException.Assert(first.Length == second.Length);

			var result = new (TFirst, TSecond)[first.Length];
			for (var i = 0; i < first.Length; i++)
			{
				result[i] = (first[i], second[i]);
			}
			return result;
		}
	}
}
