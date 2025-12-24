namespace Sudoku.Filtering.Constraints;

/// <summary>
/// Represents symmetry constraint.
/// </summary>
public sealed class SymmetryConstraint : Constraint
{
	/// <summary>
	/// Indicates an invalid value.
	/// </summary>
	public const SymmetricType InvalidSymmetricType = (SymmetricType)(-1);

	/// <summary>
	/// Indicates all possible symmetric types are included.
	/// </summary>
	public const SymmetricType AllSymmetricTypes = (SymmetricType)255;


	/// <summary>
	/// Indicates the supported symmetry types to be used.
	/// </summary>
	public SymmetricType SymmetricTypes { get; set; }


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Constraint? other)
		=> other is SymmetryConstraint comparer && SymmetricTypes == comparer.SymmetricTypes;

	/// <inheritdoc/>
	public override int GetHashCode() => (int)SymmetricTypes;

	/// <inheritdoc/>
	public override string ToString(CultureInfo culture)
		=> string.Format(
			SR.Get("SymmetryConstraint", culture),
			SymmetricTypes switch
			{
				InvalidSymmetricType => SR.Get("SymmetryConstraint_NoSymmetrySelected"),
				_ => string.Join(
					SR.Get("_Token_Comma"),
					from type in SymmetricTypes.AllFlags select type.GetName(culture)
				)
			}
		);

	/// <inheritdoc/>
	public override SymmetryConstraint Clone() => new() { IsNegated = IsNegated, SymmetricTypes = SymmetricTypes };

	/// <inheritdoc/>
	protected override bool CheckCore(ConstraintCheckingContext context)
		=> context.Grid.Symmetry is var symmetry
		&& SymmetricTypes switch
		{
			InvalidSymmetricType => symmetry == SymmetricType.None,
			_ => (SymmetricTypes & symmetry) != 0
		};
}
