namespace Sudoku.Analytics.Ranking;

/// <summary>
/// Represents a house truth.
/// </summary>
/// <param name="house">Indicataes the house.</param>
/// <param name="digit">Indicates the digit.</param>
[TypeImpl(TypeImplFlags.Object_GetHashCode)]
public sealed partial class HouseLink(House house, Digit digit) : RankSet
{
	/// <inheritdoc/>
	public override RankSetType Type => RankSetType.HouseLink;

	/// <summary>
	/// Indicates the house.
	/// </summary>
	[HashCodeMember]
	public House House { get; } = house;

	/// <summary>
	/// Indicates the digit.
	/// </summary>
	[HashCodeMember]
	public Digit Digit { get; } = digit;


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] RankSet? other)
		=> other is HouseLink comparer && Type == comparer.Type && House == comparer.House && Digit == comparer.Digit;

	/// <inheritdoc/>
	public override bool ContainsAssignment(Candidate assignment)
	{
		var digit = assignment % 9;
		if (digit != Digit)
		{
			return false;
		}

		var cell = assignment / 9;
		return HousesMap[House].Contains(cell);
	}

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
		if (House.CompareTo(((HouseLink)other).House) is var r2 and not 0)
		{
			return r2;
		}
		return Digit.CompareTo(((HouseLink)other).Digit);
	}

	/// <inheritdoc/>
	public override string ToString()
		=> (
			House switch
			{
				< 9 => Space.BlockNumber(House, Digit),
				< 18 => Space.RowNumber(House - 9, Digit),
				_ => Space.ColumnNumber(House - 18, Digit)
			}
		).ToString();

	/// <inheritdoc/>
	protected internal override bool IsSatisfied(in CandidateMap assignments)
		=> BitOperations.PopCount(assignments.GetPositionsFor(House, Digit)) is 0 or 1;
}
