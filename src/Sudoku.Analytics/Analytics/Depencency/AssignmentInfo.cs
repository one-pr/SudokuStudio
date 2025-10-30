namespace Sudoku.Analytics.Depencency;

/// <summary>
/// Represents an assignment for a group of cells and the specified digit.
/// </summary>
/// <param name="Digit">Indicates the digit.</param>
/// <param name="Cells">Indicates cells used.</param>
public readonly record struct AssignmentInfo(Digit Digit, in CellMap Cells) : IEqualityOperators<AssignmentInfo, AssignmentInfo, bool>
{
	/// <summary>
	/// Initializes an <see cref="AssignmentInfo"/> instance via the specified candidate.
	/// </summary>
	/// <param name="candidate">The candidate.</param>
	public AssignmentInfo(Candidate candidate) : this(candidate % 9, (candidate / 9).AsCellMap())
	{
	}


	/// <summary>
	/// Indicates whether the assignment instance is for grouped set rule.
	/// </summary>
	public bool IsGrouped => Cells.Count != 1;


	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp9/feature[@name='records']/target[@name='method' and @cref='PrintMembers']"/>
	private bool PrintMembers(StringBuilder builder)
	{
		builder.Append($"{nameof(Digit)} = {Digit + 1}, ");
		builder.Append($"{nameof(Cells)} = {Cells}");
		return true;
	}
}
