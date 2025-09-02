namespace Sudoku.Generating.Filtering.Constraints;

/// <summary>
/// Represents a constraint that only checks for one constraint.
/// </summary>
/// <param name="constraint"><inheritdoc cref="Constraint" path="/summary"/></param>
public abstract class UnaryLogicalConstraint(Constraint constraint) : LogicalConstraint
{
	/// <summary>
	/// Indicates the constraint.
	/// </summary>
	public Constraint Constraint { get; } = constraint;
}
