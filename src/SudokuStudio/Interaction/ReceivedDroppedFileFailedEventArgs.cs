namespace SudokuStudio.Interaction;

/// <summary>
/// Provides event data used by event <see cref="SudokuPane.ReceivedDroppedFileFailed"/>.
/// </summary>
/// <param name="reason"><inheritdoc cref="Reason" path="/summary"/></param>
/// <remarks>
/// Initializes a <see cref="ReceivedDroppedFileFailedEventArgs"/> instance via the specified reason.
/// </remarks>
/// <seealso cref="SudokuPane.ReceivedDroppedFileFailed"/>
public sealed class ReceivedDroppedFileFailedEventArgs(ReceivedDroppedFileFailedReason reason) : EventArgs
{
	/// <summary>
	/// The failed reason.
	/// </summary>
	public ReceivedDroppedFileFailedReason Reason { get; } = reason;
}
