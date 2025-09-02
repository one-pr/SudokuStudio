namespace Sudoku.Generating.Filtering.Constraints;

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
	public override string ToString(IFormatProvider? formatProvider)
		=> string.Format(
			SR.Get("AllConstraint", formatProvider as CultureInfo),
			string.Join(Environment.NewLine, from constraint in Constraints select constraint.ToString(formatProvider))
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
