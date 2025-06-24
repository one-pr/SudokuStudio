namespace SudokuStudio.Interaction;

/// <summary>
/// Provides event data used by event <see cref="StepSearcherListView.ItemSelected"/>.
/// </summary>
/// <param name="selectedSearcherInfo"><inheritdoc cref="SelectedSearcherInfo" path="/summary"/></param>
/// <seealso cref="StepSearcherListView.ItemSelected"/>
public sealed class StepSearcherListViewItemSelectedEventArgs(StepSearcherInfo selectedSearcherInfo) : EventArgs
{
	/// <summary>
	/// The selected searcher's information.
	/// </summary>
	public StepSearcherInfo SelectedSearcherInfo { get; } = selectedSearcherInfo;
}
