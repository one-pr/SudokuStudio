namespace Sudoku.Filtering.Constraints;

/// <summary>
/// Represents a lasting constraint.
/// </summary>
[ConstraintOptions(AllowsMultiple = true)]
public sealed class LastingConstraint : Constraint, ILimitCountConstraint<int>, IComparisonOperatorConstraint
{
	/// <inheritdoc/>
	public int LimitCount { get; set; }

	/// <summary>
	/// Indicates the technique used.
	/// </summary>
	public SingleTechniqueFlag TechniqueFlag { get; set; }

	/// <inheritdoc/>
	public ComparisonOperator Operator { get; set; }


	/// <inheritdoc/>
	static int ILimitCountConstraint<int>.Minimum => 0;

	/// <inheritdoc/>
	static int ILimitCountConstraint<int>.Maximum => 8;


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Constraint? other)
		=> other is LastingConstraint comparer
		&& (LimitCount, TechniqueFlag, Operator) == (comparer.LimitCount, comparer.TechniqueFlag, comparer.Operator);

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(LimitCount, TechniqueFlag, Operator);

	/// <inheritdoc/>
	public override string ToString(CultureInfo culture)
		=> string.Format(
			SR.Get("LastingConstraint", culture),
			[TechniqueFlag.GetName(culture), Operator.OperatorString, LimitCount.ToString()]
		);

	/// <inheritdoc/>
	public override LastingConstraint Clone()
		=> new() { LimitCount = LimitCount, TechniqueFlag = TechniqueFlag, Operator = Operator, IsNegated = IsNegated };

	/// <inheritdoc/>
	protected override bool CheckCore(ConstraintCheckingContext context)
	{
		var desiredTechnique = TechniqueFlag switch
		{
			SingleTechniqueFlag.HiddenSingleRow => Technique.CrosshatchingRow,
			SingleTechniqueFlag.HiddenSingleColumn => Technique.CrosshatchingColumn,
			SingleTechniqueFlag.NakedSingle => Technique.NakedSingle
		};
		if (Operator is ComparisonOperator.Inequality or ComparisonOperator.Equality && LimitCount == 0)
		{
			// Optimization: If a user sets 'lasting != 0' constraint, it'll degenerate to check existence of such technique.
			// also, 'lasting == 0' can be handled here.
			foreach (var step in context.AnalysisResult)
			{
				if (step.Code == desiredTechnique)
				{
					// '!= 0' means it exists, and '== 0' means it not.
					return Operator == ComparisonOperator.Inequality;
				}
			}
			return Operator == ComparisonOperator.Equality;
		}

		foreach (var step in context.AnalysisResult)
		{
			if (step is not (SingleStep { Code: var technique } and ILastingTrait { Lasting: var factLasting })
				|| desiredTechnique == technique && !Operator.OperatorInt32(factLasting, LimitCount))
			{
				return false;
			}
		}
		return true;
	}
}
