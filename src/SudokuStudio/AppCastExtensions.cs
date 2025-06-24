namespace SudokuStudio;

/// <summary>
/// Provides with extension methods on <see cref="App"/>.
/// </summary>
/// <seealso cref="App"/>
public static class AppCastExtensions
{
	/// <summary>
	/// Provide casting method.
	/// </summary>
	extension(Application @this)
	{
		/// <summary>
		/// Converts the current instance into an <see cref="App"/> instance;
		/// throw <see cref="InvalidCastException"/> if the current object is not an <see cref="App"/> instance.
		/// </summary>
		/// <returns>The result casted.</returns>
		public App AsApp() => (App)@this;
	}
}
