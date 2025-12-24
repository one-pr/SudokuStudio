namespace Sudoku.Filtering.Constraints;

/// <summary>
/// Represents minimal constraint.
/// </summary>
public sealed class MinimalConstraint : Constraint
{
	/// <summary>
	/// Indicates whether the puzzle shsould be minimal.
	/// </summary>
	public bool ShouldBeMinimal { get; set; }


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Constraint? other)
		=> other is MinimalConstraint comparer && ShouldBeMinimal == comparer.ShouldBeMinimal;

	/// <inheritdoc/>
	public override int GetHashCode() => ShouldBeMinimal ? 1 : 0;

	/// <inheritdoc/>
	public override string ToString(CultureInfo culture)
		=> string.Format(
			SR.Get("MinimalConstraint", culture),
			ShouldBeMinimal ? string.Empty : SR.Get("NoString", culture)
		);

	/// <inheritdoc/>
	public override MinimalConstraint Clone() => new() { IsNegated = IsNegated, ShouldBeMinimal = ShouldBeMinimal };

	/// <inheritdoc/>
	protected override bool CheckCore(ConstraintCheckingContext context) => context.Grid.IsMinimal == ShouldBeMinimal;
}
