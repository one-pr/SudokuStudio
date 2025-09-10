#undef EXTENSION_OPEARTORS_SUPPORT_ON_NULLABLE_GENERIC_TYPES

namespace System;

/// <summary>
/// Provides with extension methods on generic type.
/// </summary>
public static class GenericExtensions
{
	/// <summary>
	/// Provides extension members on <typeparamref name="T"/>?,
	/// where <typeparamref name="T"/> satisfies <see langword="class"/>? constraint.
	/// </summary>
	extension<T>(T?) where T : class?
	{
#if EXTENSION_OPEARTORS_SUPPORT_ON_NULLABLE_GENERIC_TYPES
		/// <summary>
		/// Determines whether the current value is <see langword="null"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>A <see cref="bool"/> result indicating that.</returns>
		[OverloadResolutionPriority(-1)]
		public static bool operator !([MaybeNullWhen(true)] T? value) => value is null;

		/// <summary>
		/// Determines whether the current value is non-<see langword="null"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>A <see cref="bool"/> result indicating that.</returns>
		public static bool operator true([NotNullWhen(true)] T? value) => value is not null;

		/// <summary>
		/// Determines whether the current value is <see langword="null"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>A <see cref="bool"/> result indicating that.</returns>
		public static bool operator false([MaybeNullWhen(true)] T? value) => value is null;
#endif
	}

	/// <summary>
	/// Provides extension members on <typeparamref name="T"/>? (i.e. <see cref="Nullable{T}"/>),
	/// where <typeparamref name="T"/> satisfies <see langword="struct"/> constraint.
	/// </summary>
	extension<T>(T?) where T : struct
	{
#if EXTENSION_OPEARTORS_SUPPORT_ON_NULLABLE_GENERIC_TYPES
		/// <summary>
		/// Determines whether the current value is <see langword="null"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>A <see cref="bool"/> result indicating that.</returns>
		[OverloadResolutionPriority(-1)]
		public static bool operator !([MaybeNullWhen(true)] T? value) => !value.HasValue;

		/// <summary>
		/// Determines whether the current value is non-<see langword="null"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>A <see cref="bool"/> result indicating that.</returns>
		public static bool operator true([NotNullWhen(true)] T? value) => value.HasValue;

		/// <summary>
		/// Determines whether the current value is <see langword="null"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>A <see cref="bool"/> result indicating that.</returns>
		public static bool operator false([MaybeNullWhen(true)] T? value) => !value.HasValue;
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
