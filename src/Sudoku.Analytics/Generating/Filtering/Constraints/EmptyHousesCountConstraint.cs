namespace Sudoku.Generating.Filtering.Constraints;

/// <summary>
/// Represents a constraint that calculates for empty houses.
/// </summary>
[TypeImpl(TypeImplFlags.Object_GetHashCode | TypeImplFlags.Object_ToString)]
public sealed partial class EmptyHousesCountConstraint : Constraint, ILimitCountConstraint<Digit>
{
	/// <summary>
	/// Indicates the target house type.
	/// </summary>
	[HashCodeMember]
	[StringMember]
	public HouseType HouseType { get; set; }

	/// <summary>
	/// Indicates the number of empty houses should be appeared.
	/// </summary>
	[HashCodeMember]
	[StringMember]
	public Digit Count { get; set; }

	/// <inheritdoc/>
	Digit ILimitCountConstraint<Digit>.LimitCount { get => Count; set => Count = value; }


	/// <inheritdoc/>
	public static Digit Minimum => 0;

	/// <inheritdoc/>
	public static Digit Maximum => 4;


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Constraint? other)
		=> other is EmptyHousesCountConstraint comparer
		&& Count == comparer.Count && HouseType == comparer.HouseType;

	/// <inheritdoc/>
	public override string ToString(IFormatProvider? formatProvider)
	{
		var culture = formatProvider as CultureInfo;
		return string.Format(
			SR.Get("EmptyHousesCountConstraint", culture),
			[
				Count.ToString(),
				SR.Get($"{HouseType}Name", culture),
				Count == 1 ? string.Empty : SR.Get("NounPluralSuffix", culture)
			]
		);
	}

	/// <inheritdoc/>
	public override EmptyHousesCountConstraint Clone() => new() { HouseType = HouseType, Count = Count };

	/// <inheritdoc/>
	protected override bool CheckCore(ConstraintCheckingContext context)
	{
		ref readonly var grid = ref context.Grid;
		var emptyCells = grid.EmptyCells;

		var count = 0;
		var start = (House)HouseType * 9;
		for (var i = start; i < start + 9; i++)
		{
			if ((emptyCells & HousesMap[i]) == HousesMap[i])
			{
				count++;
			}
		}
		return Count == count;
	}
}
