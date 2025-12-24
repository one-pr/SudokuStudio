namespace Sudoku.Filtering.Constraints;

/// <summary>
/// Represents an elimination count constraint.
/// </summary>
public sealed class EliminationCountConstraint : Constraint, IComparisonOperatorConstraint, ILimitCountConstraint<int>
{
	/// <inheritdoc/>
	public int LimitCount { get; set; }

	/// <summary>
	/// Indicates the technique used.
	/// </summary>
	public Technique Technique { get; set; }

	/// <inheritdoc/>
	public ComparisonOperator Operator { get; set; }


	/// <inheritdoc/>
	static int ILimitCountConstraint<int>.Maximum => 30;

	/// <inheritdoc/>
	static int ILimitCountConstraint<int>.Minimum => 0;


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Constraint? other)
		=> other is EliminationCountConstraint comparer && (LimitCount, Operator) == (comparer.LimitCount, comparer.Operator);

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(LimitCount, Technique, Operator);

	/// <inheritdoc/>
	public override string ToString(CultureInfo culture)
		=> string.Format(
			SR.Get("EliminationCountConstraint", culture),
			Operator.OperatorString,
			LimitCount,
			LimitCount == 1 ? string.Empty : SR.Get("NounPluralSuffix", culture),
			Technique.GetName(culture)
		);

	/// <inheritdoc/>
	public override EliminationCountConstraint Clone()
		=> new() { IsNegated = IsNegated, LimitCount = LimitCount, Operator = Operator, Technique = Technique };

	/// <inheritdoc/>
	protected override bool CheckCore(ConstraintCheckingContext context)
	{
		var @operator = Operator.OperatorInt32;
		foreach (var step in context.AnalysisResult)
		{
			if (step.Code == Technique && @operator(step.Conclusions.Length, LimitCount))
			{
				return true;
			}
		}
		return false;
	}
}
