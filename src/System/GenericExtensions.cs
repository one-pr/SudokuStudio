namespace System;

/// <summary>
/// Provides with extension methods on generic type.
/// </summary>
public static class GenericExtensions
{
	/// <summary>
	/// Provides extension members on <typeparamref name="T"/>,
	/// where <typeparamref name="T"/> satisfies <see langword="class"/> constraint.
	/// </summary>
	extension<T>(T) where T : class
	{
#if EXTENSION_OPERATORS
		/// <summary>
		/// Determines whether the current value is non-<see langword="null"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>A <see cref="bool"/> result indicating that.</returns>
		public static bool operator true(T? value) => value is not null;

		/// <summary>
		/// Determines whether the current value is <see langword="null"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>A <see cref="bool"/> result indicating that.</returns>
		public static bool operator false(T? value) => value is null;
#endif
	}

	/// <summary>
	/// Provides extension members on <typeparamref name="T"/>,
	/// where <typeparamref name="T"/> satisfies <see langword="struct"/> constraint.
	/// </summary>
	extension<T>(T) where T : struct, allows ref struct
	{
		/// <summary>
		/// Represents null reference of the type <typeparamref name="T"/>.
		/// </summary>
		[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
		public static ref T nullref => ref Unsafe.NullRef<T>();
	}
}
