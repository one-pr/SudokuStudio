namespace Sudoku.Generating.Filtering.Constraints;

/// <summary>
/// Represents a constraint that determine whether two constraints are both true.
/// </summary>
/// <param name="constraint1"><inheritdoc cref="BinaryLogicalConstraint.Constraint1" path="/summary"/></param>
/// <param name="constraint2"><inheritdoc cref="BinaryLogicalConstraint.Constraint2" path="/summary"/></param>
public sealed class AndConstraint(Constraint constraint1, Constraint constraint2) : BinaryLogicalConstraint(constraint1, constraint2)
{
	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Constraint? other)
		=> other is AndConstraint comparer
		&& Constraint1 == comparer.Constraint1 && Constraint2 == comparer.Constraint2;

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(Constraint1, Constraint2);

	/// <inheritdoc/>
	public override string ToString(IFormatProvider? formatProvider)
		=> string.Format(
			SR.Get("AndConstraint", formatProvider as CultureInfo),
			Constraint1.ToString(formatProvider),
			Constraint2.ToString(formatProvider)
		);

	/// <inheritdoc/>
	public override AndConstraint Clone() => new(Constraint1.Clone(), Constraint2.Clone());

	/// <inheritdoc/>
	protected override bool CheckCore(ConstraintCheckingContext context) => Constraint1.Check(context) && Constraint2.Check(context);
}
