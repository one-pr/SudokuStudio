namespace System.Linq;

public partial class SpanEnumerable
{
	/// <summary>
	/// Provides extension members on <see cref="ReadOnlySpan{T}"/> of <typeparamref name="TBase"/>.
	/// </summary>
	/// <typeparam name="TBase">The type of the elements of source.</typeparam>
	/// <typeparam name="TDerived">The derived type to be checked.</typeparam>
	/// <param name="source">The collection to be used and checked.</param>
	extension<TBase, TDerived>(ReadOnlySpan<TBase> source) where TDerived : TBase?
	{
		/// <summary>
		/// Determines whether all elements are of type <typeparamref name="TDerived"/>.
		/// </summary>
		/// <returns>A <see cref="bool"/> result indicating that.</returns>
		public bool AllAre()
		{
			foreach (ref readonly var element in source)
			{
				if (element is not TDerived)
				{
					return false;
				}
			}
			return true;
		}
	}
}
