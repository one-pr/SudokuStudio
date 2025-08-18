namespace System.Threading;

/// <summary>
/// Provides with extension members on <see cref="CancellationToken"/> instances.
/// </summary>
/// <seealso cref="CancellationToken"/>
public static class CancellationTokenExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="CancellationToken"/>.
	/// </summary>
	extension(CancellationToken)
	{
#if EXTENSION_OPERATORS
		/// <summary>
		/// Indicates whether the cancellation token is cancellation-requested.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>A <see cref="bool"/> result indicating that.</returns>
		public static bool operator !(CancellationToken value) => value.IsCancellationRequested;

		/// <summary>
		/// Indicates whether the cancellation token isn't cancellation-requested.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>A <see cref="bool"/> result indicating that.</returns>
		public static bool operator true(CancellationToken value) => !value.IsCancellationRequested;

		/// <summary>
		/// Indicates whether the cancellation token is cancellation-requested.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>A <see cref="bool"/> result indicating that.</returns>
		public static bool operator false(CancellationToken value) => value.IsCancellationRequested;
#endif
	}
}
