namespace System;

/// <summary>
/// Provides a way to create <see cref="ReadOnlySpan{T}"/>.
/// </summary>
/// <seealso cref="ReadOnlySpan{T}"/>
public static class ReadOnlySpan
{
	/// <summary>
	/// Creates a <see cref="ReadOnlySpan{T}"/> that only contains one element.
	/// </summary>
	/// <typeparam name="T">The type of the element.</typeparam>
	/// <param name="value">The value.</param>
	/// <returns>The result.</returns>
	public static ReadOnlySpan<T> Single<T>(scoped in T value) => MemoryMarshal.CreateReadOnlySpan(in value, 1);
}
