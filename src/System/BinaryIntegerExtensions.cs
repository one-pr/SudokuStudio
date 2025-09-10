namespace System;

/// <summary>
/// Provides with extension methods on <see cref="IBinaryInteger{TSelf}"/> instances.
/// </summary>
/// <seealso cref="IBinaryInteger{TSelf}"/>
public static class BinaryIntegerExtensions
{
	/// <summary>
	/// Provides extension members on <typeparamref name="TSelf"/>,
	/// where <typeparamref name="TSelf"/> satisfies <see cref="IBinaryInteger{TSelf}"/> constraint.
	/// </summary>
	extension<TSelf>(TSelf) where TSelf : IBinaryInteger<TSelf>
	{
		/// <summary>
		/// Determine whether the specified value isn't equal to 0.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>A <see cref="bool"/> result indicating that.</returns>
		public static bool operator true(TSelf value) => value != TSelf.Zero;

		/// <summary>
		/// Determine whether the specified value is equal to 0.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>A <see cref="bool"/> result indicating that.</returns>
		public static bool operator false(TSelf value) => value == TSelf.Zero;
	}
}
