namespace SudokuStudio;

/// <summary>
/// Provides with extension methods on <see cref="App"/>.
/// </summary>
/// <seealso cref="App"/>
public static class AppExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="App"/>.
	/// </summary>
	extension(App @this)
	{
		/// <summary>
		/// Try to get <see cref="StepSearcher"/> instances via configuration for the specified application.
		/// </summary>
		/// <returns>A list of <see cref="StepSearcher"/> instances.</returns>
		public StepSearcher[] GetStepSearchers()
			=> [
				..
				from data in @this.Preference.StepSearcherOrdering.StepSearchersOrder
				where data.IsEnabled
				select data.CreateStepSearcher()
			];
	}

	/// <summary>
	/// Provides extension members on <see cref="Application"/>.
	/// </summary>
	extension(Application @this)
	{
		/// <inheritdoc cref="Application.Current"/>
		public static App CurrentApp => (App)Application.Current;
	}
}
