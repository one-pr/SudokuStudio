namespace SudokuStudio.Interaction;

/// <summary>
/// Provides event data used by event <see cref="AnalyzePage.SaveFileFailed"/>.
/// </summary>
/// <param name="reason"><inheritdoc cref="Reason" path="/summary"/></param>
/// <seealso cref="AnalyzePage.SaveFileFailed"/>
public sealed class SaveFileFailedEventArgs(SaveFileFailedReason reason) : EventArgs
{
	/// <summary>
	/// The failed reason.
	/// </summary>
	public SaveFileFailedReason Reason { get; } = reason;
}
