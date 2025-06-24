namespace SudokuStudio.Interaction;

/// <summary>
/// Provides event data used by event <see cref="DigitSet.SelectedDigitChanged"/>.
/// </summary>
/// <param name="newDigit"><inheritdoc cref="NewDigit" path="/summary"/></param>
/// <seealso cref="DigitSet.SelectedDigitChanged"/>
public sealed class DigitSetSelectedDigitChangedEventArgs(Digit newDigit) : EventArgs
{
	/// <summary>
	/// Indicates the new digit selected.
	/// </summary>
	public Digit NewDigit { get; } = newDigit;
}
