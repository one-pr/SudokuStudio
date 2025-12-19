namespace System;

/// <summary>
/// Provides extension methods on <see cref="Activator"/>.
/// </summary>
/// <seealso cref="Activator"/>
public static class ActivatorExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="Activator"/>.
	/// </summary>
	extension(Activator)
	{
		/// <summary>
		/// Creates an instance of specified type, with parameter types specified.
		/// </summary>
		/// <param name="type">The type of instance to create.</param>
		/// <param name="parameterTypes">Types of parameters.</param>
		/// <param name="arguments">The parameter values.</param>
		/// <returns>The instance.</returns>
		public static object? Create(Type type, Type[] parameterTypes, object?[]? arguments)
		{
			var constructorInfo = type.GetConstructor(parameterTypes);
			return constructorInfo?.Invoke(arguments);
		}

		/// <summary>
		/// Creates an instance of specified type, with parameter types specified.
		/// </summary>
		/// <typeparam name="T">The type of instance to create.</typeparam>
		/// <param name="parameterTypes">Types of parameters.</param>
		/// <param name="arguments">The parameter values.</param>
		/// <returns>The instance.</returns>
		public static T? Create<T>(Type[] parameterTypes, object?[]? arguments)
			=> (T?)Activator.Create(typeof(T), parameterTypes, arguments);

		/// <summary>
		/// Creates an instance of specified type, with parameter types specified.
		/// </summary>
		/// <typeparam name="T">The type of instance to create.</typeparam>
		/// <typeparam name="TArg">The type of argument.</typeparam>
		/// <param name="arg">The parameter value.</param>
		/// <returns>The instance.</returns>
		public static T? Create<T, TArg>(TArg? arg) => Activator.Create<T>([typeof(TArg)], [arg]);

		/// <inheritdoc cref="Create{T, TArg1, TArg2, TArg3, TArg4}(TArg1, TArg2, TArg3, TArg4)"/>
		public static T? Create<T, TArg1, TArg2>(TArg1? arg1, TArg2? arg2)
			=> Activator.Create<T>([typeof(TArg1), typeof(TArg2)], [arg1, arg2]);

		/// <inheritdoc cref="Create{T, TArg1, TArg2, TArg3, TArg4}(TArg1, TArg2, TArg3, TArg4)"/>
		public static T? Create<T, TArg1, TArg2, TArg3>(TArg1? arg1, TArg2? arg2, TArg3? arg3)
			=> Activator.Create<T>([typeof(TArg1), typeof(TArg2), typeof(TArg3)], [arg1, arg2, arg3]);

		/// <summary>
		/// Creates an instance of specified type, with parameter types specified.
		/// </summary>
		/// <typeparam name="T">The type of instance to create.</typeparam>
		/// <typeparam name="TArg1">The type of argument #1.</typeparam>
		/// <typeparam name="TArg2">The type of argument #2.</typeparam>
		/// <typeparam name="TArg3">The type of argument #3.</typeparam>
		/// <typeparam name="TArg4">The type of argument #4.</typeparam>
		/// <param name="arg1">The parameter value #1.</param>
		/// <param name="arg2">The parameter value #2.</param>
		/// <param name="arg3">The parameter value #3.</param>
		/// <param name="arg4">The parameter value #4.</param>
		/// <returns>The instance.</returns>
		public static T? Create<T, TArg1, TArg2, TArg3, TArg4>(TArg1? arg1, TArg2? arg2, TArg3? arg3, TArg4? arg4)
			=> Activator.Create<T>([typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4)], [arg1, arg2, arg3, arg4]);
	}
}
