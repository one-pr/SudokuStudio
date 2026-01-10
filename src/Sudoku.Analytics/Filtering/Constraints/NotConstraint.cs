namespace Sudoku.Filtering.Constraints;

/// <summary>
/// Represents a constraint that negates the result.
/// </summary>
/// <param name="constraint"><inheritdoc cref="UnaryLogicalConstraint.Constraint" path="/summary"/></param>
public sealed class NotConstraint(Constraint constraint) : UnaryLogicalConstraint(constraint)
{
	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Constraint? other)
		=> other is NotConstraint comparer && Constraint == comparer.Constraint;

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(Constraint);

	/// <inheritdoc/>
	public override string ToString(CultureInfo culture)
		=> string.Format(SR.Get("NotConstraint", culture), Constraint.ToString(culture));

	/// <inheritdoc/>
	public override NotConstraint Clone() => new(Constraint.Clone());

	/// <inheritdoc/>
	protected override bool CheckCore(ConstraintCheckingContext context) => !Constraint.Check(context);
}
