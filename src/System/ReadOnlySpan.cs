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

	/// <summary>
	/// Creates a <see cref="ReadOnlySpan{T}"/> instance.
	/// </summary>
	/// <typeparam name="T">The type of each element.</typeparam>
	/// <param name="value">The reference to the first element.</param>
	/// <param name="length">The length.</param>
	/// <returns><see cref="ReadOnlySpan{T}"/> instance.</returns>
	public static ReadOnlySpan<T> Create<T>(scoped in T value, int length) => MemoryMarshal.CreateReadOnlySpan(in value, length);
}

/// <summary>
/// Provides a way to create <see cref="Span{T}"/>.
/// </summary>
/// <seealso cref="Span{T}"/>
public static class Span
{
	/// <summary>
	/// Creates a <see cref="Span{T}"/> instance.
	/// </summary>
	/// <typeparam name="T">The type of each element.</typeparam>
	/// <param name="value">The reference to the first element.</param>
	/// <param name="length">The length.</param>
	/// <returns><see cref="Span{T}"/> instance.</returns>
	public static Span<T> Create<T>(scoped ref T value, int length) => MemoryMarshal.CreateSpan(ref value, length);

	/// <inheritdoc cref="Create{T}(ref T, int)"/>
	public static unsafe Span<T> Create<T>(T* value, int length) => MemoryMarshal.CreateSpan(ref *value, length);
}
