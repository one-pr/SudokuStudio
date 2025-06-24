namespace SudokuStudio.Interaction;

/// <summary>
/// Provides event data used by event <see cref="CandidatePicker.SelectedCandidateChanged"/>.
/// </summary>
/// <seealso cref="CandidatePicker.SelectedCandidateChanged"/>
/// <param name="newValue"><inheritdoc cref="NewValue" path="/summary"/></param>
public sealed class CandidatePickerSelectedCandidateChangedEventArgs(Candidate newValue) : EventArgs
{
	/// <summary>
	/// The new value.
	/// </summary>
	public int NewValue { get; } = newValue;
}
