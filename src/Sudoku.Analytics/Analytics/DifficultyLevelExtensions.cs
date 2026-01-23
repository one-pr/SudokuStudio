namespace Sudoku.Analytics;

/// <summary>
/// Provides with extension methods on <see cref="DifficultyLevel"/>.
/// </summary>
/// <seealso cref="DifficultyLevel"/>
public static class DifficultyLevelExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(DifficultyLevel @this)
	{
		/// <summary>
		/// Gets the name of the current value, with specified culture.
		/// </summary>
		/// <param name="culture">The culture.</param>
		/// <returns>The string value.</returns>
		public string GetName(CultureInfo? culture)
			=> BitOperations.PopCount((uint)(int)@this) < 2
				? SR.Get(@this.ToString(), culture)
				: throw new InvalidOperationException(SR.ExceptionMessage("MultipleFlagsExist"));
	}
}
