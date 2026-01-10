namespace Sudoku.Analytics;

/// <summary>
/// Provides with extension methods on <see cref="DifficultyLevel"/>.
/// </summary>
/// <seealso cref="DifficultyLevel"/>
public static class DifficultyLevelExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="DifficultyLevel"/>.
	/// </summary>
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
