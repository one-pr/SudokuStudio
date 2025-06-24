namespace SudokuStudio.Interaction;

/// <summary>
/// Provides event data used by event <see cref="SudokuPane.ReceivedDroppedFileSuccessfully"/>.
/// </summary>
/// <param name="filePath"><inheritdoc cref="FilePath" path="/summary"/></param>
/// <param name="gridInfo"><inheritdoc cref="GridInfo" path="/summary"/></param>
/// <seealso cref="SudokuPane.ReceivedDroppedFileSuccessfully"/>
public sealed class ReceivedDroppedFileSuccessfullyEventArgs(string filePath, GridInfo gridInfo) : EventArgs
{
	/// <summary>
	/// The path of the dropped file.
	/// </summary>
	public string FilePath { get; } = filePath;

	/// <summary>
	/// The loaded grid info.
	/// </summary>
	public GridInfo GridInfo { get; } = gridInfo;
}
