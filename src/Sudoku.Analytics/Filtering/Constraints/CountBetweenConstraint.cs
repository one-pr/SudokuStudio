namespace Sudoku.Filtering.Constraints;

/// <summary>
/// Represents a count between constraint.
/// </summary>
public sealed class CountBetweenConstraint : Constraint, IBetweenRuleConstraint
{
	/// <summary>
	/// Indicates the range of the numbers set.
	/// </summary>
	[JsonConverter(typeof(RangeConverter))]
	public Range Range { get; set; }

	/// <summary>
	/// Indicates the cell state to be checked. The desired values should be
	/// <see cref="CellState.Given"/> or <see cref="CellState.Empty"/>.
	/// </summary>
	/// <seealso cref="CellState.Given"/>
	/// <seealso cref="CellState.Empty"/>
	public CellState CellState { get; set; }

	/// <inheritdoc/>
	public BetweenRule BetweenRule { get; set; }


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Constraint? other)
		=> other is CountBetweenConstraint comparer
		&& Range.Equals(comparer.Range) && (CellState, BetweenRule) == (comparer.CellState, comparer.BetweenRule);

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(Range, CellState, BetweenRule);

	/// <inheritdoc/>
	public override string ToString(CultureInfo culture)
		=> string.Format(
			SR.Get("CountBetweenConstraint", culture),
			SR.Get(CellState switch { CellState.Given => "GivenCell", _ => "EmptyCell" }, culture),
			Range.Start.Value,
			Range.End.Value,
			BetweenRule switch
			{
				BetweenRule.BothOpen => SR.Get("BothOpen", culture),
				BetweenRule.LeftOpen => SR.Get("LeftOpen", culture),
				BetweenRule.RightOpen => SR.Get("RightOpen", culture),
				BetweenRule.BothClosed => SR.Get("BothClosed", culture)
			}
		);

	/// <inheritdoc/>
	public override CountBetweenConstraint Clone()
		=> new() { IsNegated = IsNegated, BetweenRule = BetweenRule, CellState = CellState, Range = Range };

	/// <inheritdoc/>
	protected override bool CheckCore(ConstraintCheckingContext context)
	{
		ref readonly var grid = ref context.Grid;
		return CellState switch { CellState.Empty => grid.EmptyCellsCount, _ => grid.GivenCellsCount } is var factCount
			&& Range is { Start.Value: var min, End.Value: var max }
			&& BetweenRule switch
			{
				BetweenRule.BothOpen => factCount > min && factCount < max,
				BetweenRule.LeftOpen => factCount >= min && factCount <= max,
				BetweenRule.RightOpen => factCount >= min && factCount < max,
				BetweenRule.BothClosed => factCount >= min && factCount <= max
			};
	}
}
