namespace SudokuStudio.Interaction;

/// <summary>
/// Represents a rotating option on <see cref="SudokuPaneCell"/> control.
/// </summary>
/// <seealso cref="SudokuPaneCell"/>
public enum GridCandidateRotating
{
	/// <summary>
	/// Represents no rotating.
	/// </summary>
	None,

	/// <summary>
	/// Represents rotating mode as XSudo behaves.
	/// </summary>
	XSudoRotating
}
