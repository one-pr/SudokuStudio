namespace Sudoku.Filtering.Constraints;

/// <summary>
/// Represents a constraint that checks for a missing digit.
/// </summary>
public sealed class MissingDigitConstraint : Constraint
{
	/// <summary>
	/// Indicates the missing digit.
	/// </summary>
	public Digit Digit { get; set; }


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Constraint? other)
		=> other is MissingDigitConstraint comparer && Digit == comparer.Digit;

	/// <inheritdoc/>
	public override int GetHashCode() => Digit;

	/// <inheritdoc/>
	public override string ToString(CultureInfo culture)
		=> string.Format(SR.Get("MissingDigitConstraint", culture), (Digit + 1).ToString());

	/// <inheritdoc/>
	public override MissingDigitConstraint Clone() => new() { Digit = Digit };

	/// <inheritdoc/>
	protected override bool CheckCore(ConstraintCheckingContext context)
	{
		ref readonly var grid = ref context.Grid;
		foreach (var cell in grid.GivenCells)
		{
			if (grid.GetDigit(cell) == Digit)
			{
				return false;
			}
		}
		return true;
	}
}
