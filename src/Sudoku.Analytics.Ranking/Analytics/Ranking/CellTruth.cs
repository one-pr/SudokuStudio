namespace Sudoku.Analytics.Ranking;

/// <summary>
/// Represents a cell truth.
/// </summary>
/// <param name="cell">The cell.</param>
[TypeImpl(TypeImplFlags.Object_GetHashCode)]
public sealed partial class CellTruth(Cell cell) : RankSet
{
	/// <inheritdoc/>
	public override RankSetType Type => RankSetType.CellTruth;

	/// <summary>
	/// Indicates the target cell.
	/// </summary>
	[HashCodeMember]
	public Cell Cell { get; } = cell;


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] RankSet? other)
		=> other is CellTruth comparer && Type == comparer.Type && Cell == comparer.Cell;

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
		return Cell.CompareTo(((CellTruth)other).Cell);
	}

	/// <inheritdoc/>
	public override string ToString() => Space.RowColumn(Cell / 9, Cell % 9).ToString();

	/// <inheritdoc/>
	public override CandidateMap GetAvailableRange(in Grid grid)
	{
		var result = CandidateMap.Empty;
		foreach (var digit in grid.GetCandidates(Cell))
		{
			result.Add(Cell * 9 + digit);
		}
		return result;
	}

	/// <inheritdoc/>
	protected internal override bool IsSatisfied(in CandidateMap assignments)
		=> BitOperations.IsPow2(assignments.GetDigitsFor(Cell));
}
