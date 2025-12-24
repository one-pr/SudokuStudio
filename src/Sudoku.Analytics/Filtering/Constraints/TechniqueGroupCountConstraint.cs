namespace Sudoku.Filtering.Constraints;

/// <summary>
/// Represents a technique group count constraint.
/// </summary>
[ConstraintOptions(AllowsMultiple = true)]
public sealed class TechniqueGroupCountConstraint : Constraint, IComparisonOperatorConstraint, ILimitCountConstraint<int>
{
	/// <inheritdoc/>
	public int LimitCount { get; set; }

	/// <inheritdoc/>
	public ComparisonOperator Operator { get; set; }

	/// <summary>
	/// Indicates the technique group.
	/// </summary>
	public TechniqueGroup TechniqueGroup { get; set; } = TechniqueGroup.None;


	/// <inheritdoc/>
	static int ILimitCountConstraint<int>.Minimum => 0;

	/// <inheritdoc/>
	static int ILimitCountConstraint<int>.Maximum => 20;


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Constraint? other)
		=> other is TechniqueGroupCountConstraint comparer
		&& (LimitCount, Operator, TechniqueGroup) == (comparer.LimitCount, comparer.Operator, comparer.TechniqueGroup);

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(LimitCount, Operator, TechniqueGroup);

	/// <inheritdoc/>
	public override string ToString(CultureInfo culture)
		=> string.Format(
			SR.Get("TechniqueGroupCountConstraint", culture),
			[TechniqueGroup.GetName(culture), Operator.OperatorString, LimitCount]
		);

	/// <inheritdoc/>
	public override TechniqueGroupCountConstraint Clone()
		=> new() { IsNegated = IsNegated, LimitCount = LimitCount, Operator = Operator, TechniqueGroup = TechniqueGroup };

	/// <inheritdoc/>
	protected override bool CheckCore(ConstraintCheckingContext context) 
	{
		if (TechniqueGroup == TechniqueGroup.None)
		{
			return true;
		}

		var times = 0;
		foreach (var step in context.AnalysisResult)
		{
			if (step.Code.Group == TechniqueGroup)
			{
				times++;
			}
		}
		return Operator.OperatorInt32(times, LimitCount);
	}
}
