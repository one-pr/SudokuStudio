namespace Sudoku;

/// <summary>
/// Represents a list of constants of <see cref="PencilmarkVisibility"/>.
/// </summary>
/// <seealso cref="PencilmarkVisibility"/>
public static class PencilmarkVisibilities
{
	/// <summary>
	/// Indicates all visibilities.
	/// </summary>
	public const PencilmarkVisibility All = PencilmarkVisibility.Direct | PencilmarkVisibility.Snyder | PencilmarkVisibility.FullMarking;

	/// <summary>
	/// Indicates only for marks.
	/// </summary>
	public const PencilmarkVisibility Marks = All & ~PencilmarkVisibility.Direct;
}
