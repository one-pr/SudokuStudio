namespace Sudoku.Filtering.Constraints;

/// <summary>
/// Represents a constraint that determines whether all constraints are determined true.
/// </summary>
/// <param name="constraints"><inheritdoc cref="QuantifierConstraint.Constraints" path="/summary"/></param>
public sealed class AllConstraint(params ConstraintCollection constraints) : QuantifierConstraint(constraints)
{
	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Constraint? other)
	{
		if (other is not AllConstraint comparer || Constraints.Count != comparer.Constraints.Count)
		{
			return false;
		}

		for (var i = 0; i < Constraints.Count; i++)
		{
			if (Constraints[i] != comparer.Constraints[i])
			{
				return false;
			}
		}
		return true;
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		var hashCode = new HashCode();
		foreach (var constraint in Constraints)
		{
			hashCode.Add(constraint);
		}
		return hashCode.ToHashCode();
	}

	/// <inheritdoc/>
	public override string ToString(CultureInfo culture)
		=> string.Format(
			SR.Get("AllConstraint", culture),
			string.Join(Environment.NewLine, from constraint in Constraints select constraint.ToString(culture))
		);

	/// <inheritdoc/>
	public override AllConstraint Clone() => new(Constraints.Clone());

	/// <inheritdoc/>
	protected override bool CheckCore(ConstraintCheckingContext context)
	{
		foreach (var constraint in Constraints)
		{
			if (!constraint.Check(context))
			{
				return false;
			}
		}
		return true;
	}
}
