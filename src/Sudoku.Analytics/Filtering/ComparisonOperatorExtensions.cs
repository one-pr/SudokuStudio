namespace Sudoku.Filtering;

/// <summary>
/// Provides with extension methods on <see cref="ComparisonOperator"/>.
/// </summary>
/// <seealso cref="ComparisonOperator"/>
public static class ComparisonOperatorExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(ComparisonOperator @this)
	{
		/// <summary>
		/// Indicates the string representation of the operator.
		/// </summary>
		public string OperatorString
			=> @this switch
			{
				ComparisonOperator.Equality => "=",
				ComparisonOperator.Inequality => "<>",
				ComparisonOperator.GreaterThan => ">",
				ComparisonOperator.GreaterThanOrEqual => ">=",
				ComparisonOperator.LessThan => "<",
				ComparisonOperator.LessThanOrEqual => "<="
			};

		/// <summary>
		/// Create a <see cref="Func{T1, T2, TResult}"/> that compares two <see cref="int"/> values.
		/// </summary>
		public Func<int, int, bool> OperatorInt32 => @this.GetOperator<int>();


		/// <summary>
		/// Creates a delegate method that executes the specified rule of comparison.
		/// </summary>
		/// <typeparam name="T">The type of the target.</typeparam>
		/// <returns>A delegate function.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Throws when the argument is out of range (not defined).</exception>
		public Func<T, T, bool> GetOperator<T>() where T : IComparisonOperators<T, T, bool>
			=> @this switch
			{
				ComparisonOperator.Equality => static (a, b) => a == b,
				ComparisonOperator.Inequality => static (a, b) => a != b,
				ComparisonOperator.GreaterThan => static (a, b) => a > b,
				ComparisonOperator.GreaterThanOrEqual => static (a, b) => a >= b,
				ComparisonOperator.LessThan => static (a, b) => a < b,
				ComparisonOperator.LessThanOrEqual => static (a, b) => a <= b,
				_ => throw new ArgumentOutOfRangeException(nameof(@this))
			};
	}
}
