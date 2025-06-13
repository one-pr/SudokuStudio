namespace Sudoku.Analytics.Ranking;

/// <summary>
/// Represents a house truth.
/// </summary>
/// <param name="house">Indicataes the house.</param>
/// <param name="digit">Indicates the digit.</param>
[TypeImpl(TypeImplFlags.Object_GetHashCode)]
public sealed partial class HouseTruth(House house, Digit digit) : RankSet
{
	/// <inheritdoc/>
	public override RankSetType Type => RankSetType.HouseTruth;

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
		=> other is HouseTruth comparer && Type == comparer.Type && House == comparer.House && Digit == comparer.Digit;

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
		if (House.CompareTo(((HouseTruth)other).House) is var r2 and not 0)
		{
			return r2;
		}
		return Digit.CompareTo(((HouseTruth)other).Digit);
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
	public override CandidateMap GetAvailableRange(in Grid grid)
	{
		var result = CandidateMap.Empty;
		foreach (var cell in HousesMap[House])
		{
			if (grid.Exists(cell, Digit) is true)
			{
				result.Add(cell * 9 + Digit);
			}
		}
		return result;
	}

	/// <inheritdoc/>
	protected internal override bool IsSatisfied(in CandidateMap assignments)
		=> BitOperations.IsPow2(assignments.GetPositionsFor(House, Digit));
}
