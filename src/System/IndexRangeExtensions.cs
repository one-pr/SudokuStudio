namespace System;

/// <summary>
/// Provides with extension methods on <see cref="Index"/> and <see cref="Range"/> instances.
/// </summary>
/// <seealso cref="Index"/>
/// <seealso cref="Range"/>
public static class IndexRangeExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="Index"/>.
	/// </summary>
	extension(Index @this)
	{
		/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
		public void Deconstruct(out int value, out bool isFromEnd) => (value, isFromEnd) = (@this.Value, @this.IsFromEnd);


		/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)"/>
		public static bool operator ==(Index left, Index right) => left.Equals(right);

		/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)"/>
		public static bool operator !=(Index left, Index right) => !(left == right);
	}

	/// <summary>
	/// Provides extension members on <see cref="Range"/>.
	/// </summary>
	extension(in Range @this)
	{
		/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
		public void Deconstruct(out Index start, out Index end) => (start, end) = (@this.Start, @this.End);


		/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
		public RangeEnumerator GetEnumerator() => new(@this);


		/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)"/>
		public static bool operator ==(in Range left, in Range right) => left.Equals(right);

		/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)"/>
		public static bool operator !=(in Range left, in Range right) => !(left == right);
	}
}
