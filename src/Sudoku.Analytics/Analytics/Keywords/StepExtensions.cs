namespace Sudoku.Analytics.Keywords;

/// <summary>
/// Represents extension methods on <see cref="Step"/> instances.
/// </summary>
/// <seealso cref="Step"/>
public static class StepExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="Step"/>.
	/// </summary>
	extension(Step @this)
	{
		/// <summary>
		/// Determine whether the current instance contains a property of name <paramref name="propertyName"/>.
		/// </summary>
		/// <typeparam name="T">The type of property value.</typeparam>
		/// <param name="propertyName">The name of property.</param>
		/// <param name="result">The result value.</param>
		/// <returns>A <see cref="bool"/> result.</returns>
		public bool TryGetPropertyValue<T>(string propertyName, out T? result) where T : notnull
		{
			var propertyInfo = @this.GetType().GetProperty(propertyName);
			if (propertyInfo is null)
			{
				result = default;
				return false;
			}
			result = (T?)propertyInfo.GetValue(@this);
			return true;
		}
	}
}
