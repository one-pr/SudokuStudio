namespace System.Linq;

public partial class SpanEnumerable
{
	/// <summary>
	/// Provides extension members on <see cref="ReadOnlySpan{T}"/> of <typeparamref name="TFirst"/>.
	/// </summary>
	extension<TFirst>(ReadOnlySpan<TFirst> first)
	{
		/// <inheritdoc cref="Enumerable.Zip{TFirst, TSecond}(IEnumerable{TFirst}, IEnumerable{TSecond})"/>
		/// <remarks>
		/// <include
		///     file="../../global-doc-comments.xml"
		///     path="g/csharp14/feature[@name='extension-container']/target[@name='generic-method']"/>
		/// </remarks>
		public ReadOnlySpan<(TFirst Left, TSecond Right)> Zip<TSecond>(ReadOnlySpan<TSecond> second)
		{
			ArgumentException.ThrowIf(first.Length == second.Length);

			var result = new (TFirst, TSecond)[first.Length];
			for (var i = 0; i < first.Length; i++)
			{
				result[i] = (first[i], second[i]);
			}
			return result;
		}
	}
}
