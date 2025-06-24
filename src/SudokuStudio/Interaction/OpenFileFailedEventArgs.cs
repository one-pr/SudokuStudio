namespace SudokuStudio.Interaction;

/// <summary>
/// Provides event data used by event <see cref="AnalyzePage.OpenFileFailed"/>.
/// </summary>
/// <param name="reason"><inheritdoc cref="Reason" path="/summary"/></param>
/// <seealso cref="AnalyzePage.OpenFileFailed"/>
public sealed class OpenFileFailedEventArgs(OpenFileFailedReason reason) : EventArgs
{
	/// <summary>
	/// The failed reason.
	/// </summary>
	public OpenFileFailedReason Reason { get; } = reason;
}
