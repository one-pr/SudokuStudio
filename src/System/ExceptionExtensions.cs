namespace System;

/// <summary>
/// Provides with extension members on <see cref="Exception"/> and its derived types.
/// </summary>
public static class ExceptionExtensions
{
	/// <summary>
	/// Provides extension members on <typeparamref name="TException"/>,
	/// where <typeparamref name="TException"/> satisfies <see cref="SystemException"/> constraint.
	/// </summary>
	/// <typeparam name="TException">The type of exception.</typeparam>
	extension<TException>(TException) where TException : SystemException
	{
		/// <summary>
		/// Throws an instance of type <typeparamref name="TException"/> if the specified assertion is failed.
		/// </summary>
		/// <param name="expression">The expression.</param>
		/// <param name="failedExpressionString">The string to the argument <paramref name="expression"/>.</param>
		/// <exception cref="InvalidOperationException">Throws when assertion is failed.</exception>
		public static void Assert(
			[DoesNotReturnIf(false)] bool expression,
			[CallerArgumentExpression(nameof(expression))] string failedExpressionString = null!
		)
		{
			if (!expression)
			{
				var expr = $"The specified expression is failed to be checked: '{failedExpressionString}'.";
				throw Activator.Create<TException, string>(expr)!;
			}
		}
	}
}
