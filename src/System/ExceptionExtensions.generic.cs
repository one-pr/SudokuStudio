namespace System;

public partial class ExceptionExtensions
{
	/// <summary>
	/// Provides extension members on <typeparamref name="TException"/>,
	/// where <typeparamref name="TException"/> satisfies <see cref="SystemException"/> constraint.
	/// </summary>
	/// <typeparam name="TException">The type of exception.</typeparam>
	extension<TException>(TException) where TException : SystemException
	{
		/// <summary>
		/// Throws an <see cref="ArgumentException"/> instance if the specified assertion is failed.
		/// </summary>
		/// <param name="expression">The expression.</param>
		/// <param name="failedExpressionString">The string to the argument <paramref name="expression"/>.</param>
		public static void Assert(
			[DoesNotReturnIf(false)] bool expression,
			[CallerArgumentExpression(nameof(expression))] string failedExpressionString = null!
		)
		{
			if (!expression)
			{
				throw new ArgumentException($"The specified expression is failed to be checked: '{failedExpressionString}'.");
			}
		}
	}
}
