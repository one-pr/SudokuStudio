namespace SudokuStudio.Interaction;

/// <summary>
/// Provides event data used by event <see cref="SudokuPane.MouseWheelChanged"/>.
/// </summary>
/// <param name="isClockwise"><inheritdoc cref="IsClockwise" path="/summary"/></param>
/// <seealso cref="SudokuPane.MouseWheelChanged"/>
public sealed class SudokuPaneMouseWheelChangedEventArgs(bool isClockwise) : EventArgs
{
	/// <summary>
	/// A <see cref="bool"/> value indicating whether the mouse wheel is clockwise.
	/// </summary>
	public bool IsClockwise { get; } = isClockwise;
}
