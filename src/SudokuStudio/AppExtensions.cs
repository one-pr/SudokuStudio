namespace SudokuStudio;

/// <summary>
/// Provides with extension methods on <see cref="App"/>.
/// </summary>
/// <seealso cref="App"/>
public static class AppExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
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

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(Application @this)
	{
		/// <inheritdoc cref="Application.Current"/>
		public static App CurrentApp => (App)Application.Current;
	}
}
