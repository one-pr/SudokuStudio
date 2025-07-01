namespace Sudoku.Generating.Filtering.Constraints;

/// <summary>
/// Represents a constraint that determines whether the specified member name satisfied the specified condition.
/// </summary>
public sealed class TypeOrPropertyConstraint : Constraint
{
	/// <summary>
	/// Represents the backing predicates to be called.
	/// </summary>
	public ICollection<TypeOrMemberPredicate> Predicates { get; } = [];


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Constraint? other) => throw new NotImplementedException();

	/// <inheritdoc/>
	public override int GetHashCode() => throw new NotImplementedException();

	/// <inheritdoc/>
	public override string ToString(IFormatProvider? formatProvider) => throw new NotImplementedException();

	/// <inheritdoc/>
	public override string ToString() => throw new NotImplementedException();

	/// <inheritdoc/>
	public override TypeOrPropertyConstraint Clone() => throw new NotImplementedException();

	/// <inheritdoc/>
	protected override bool CheckCore(ConstraintCheckingContext context) => throw new NotImplementedException();
}
