namespace Sudoku.Analytics.Ranking;

/// <summary>
/// Represents a cell link.
/// </summary>
/// <param name="cell">The cell.</param>
[TypeImpl(TypeImplFlags.Object_GetHashCode)]
public sealed partial class CellLink(Cell cell) : RankSet
{
	/// <inheritdoc/>
	public override RankSetType Type => RankSetType.CellLink;

	/// <summary>
	/// Indicates the cell.
	/// </summary>
	[HashCodeMember]
	public Cell Cell { get; } = cell;


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] RankSet? other)
		=> other is CellLink comparer && Type == comparer.Type && Cell == comparer.Cell;

	/// <inheritdoc/>
	public override bool ContainsAssignment(Candidate assignment) => assignment / 9 == Cell;

	/// <inheritdoc/>
	public override int CompareTo(RankSet? other)
	{
		if (other is null)
		{
			return 1;
		}
		if (Type.CompareTo(other.Type) is var r1 and not 0)
		{
			return r1;
		}
		return Cell.CompareTo(((CellLink)other).Cell);
	}

	/// <inheritdoc/>
	public override string ToString() => Space.RowColumn(Cell / 9, Cell % 9).ToString();

	/// <inheritdoc/>
	protected internal override bool IsSatisfied(in CandidateMap assignments)
		=> BitOperations.PopCount(assignments.GetDigitsFor(Cell)) is 0 or 1;
}
