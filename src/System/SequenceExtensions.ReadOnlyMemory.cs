namespace System;

public partial class SequenceExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="ReadOnlyMemory{T}"/>.
	/// </summary>
	extension<T>(ReadOnlyMemory<T> @this)
	{
#if false
		/// <summary>
		/// Gets the element at the specified index.
		/// </summary>
		/// <param name="index">The desired index.</param>
		/// <returns>The reference to the element at the specified index.</returns>
		public ref readonly T this[int index] => ref @this.Span[index];

		/// <inheritdoc cref="get_Item{T}(ReadOnlyMemory{T}, int)"/>
		public ref readonly T this[Index index] => ref @this.Span[index];
#endif


		/// <summary>
		/// Fetch the element at the specified index inside a <see cref="ReadOnlyMemory{T}"/>.
		/// </summary>
		/// <param name="index">The desired index.</param>
		/// <returns>The reference to the element at the specified index.</returns>
		public ref readonly T ElementAt(int index) => ref @this.Span[index];

		/// <inheritdoc cref="ElementAt{T}(ReadOnlyMemory{T}, int)"/>
		public ref readonly T ElementAt(Index index) => ref @this.Span[index];

		/// <summary>
		/// Creates a <see cref="ReadOnlyMemoryEnumerator{T}"/> instance that can be consumed by a <see langword="foreach"/> loop.
		/// </summary>
		/// <returns>A <see cref="ReadOnlyMemoryEnumerator{T}"/> instance.</returns>
		public ReadOnlyMemoryEnumerator<T> GetEnumerator() => new(@this);

		/// <summary>
		/// Returns a new <see cref="ReadOnlyMemory{T}"/> instance whose internal elements are all come from the current collection,
		/// with reversed order.
		/// </summary>
		/// <returns>A new collection whose elements are in reversed order.</returns>
		[OverloadResolutionPriority(1)]
		[Obsolete(DeprecatedMessages.ExtensionOperator_Reverse, false)]
		public ReadOnlyMemory<T> Reverse() => ~@this;


		/// <summary>
		/// Creates a reversed collection of <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The reversed collection.</returns>
		public static ReadOnlyMemory<T> operator ~(ReadOnlyMemory<T> value)
		{
			var result = new T[value.Length];
			for (var (i, j) = (value.Length - 1, 0); i >= 0; i--, j++)
			{
				result[j] = value.Span[i];
			}
			return result;
		}
	}
}
