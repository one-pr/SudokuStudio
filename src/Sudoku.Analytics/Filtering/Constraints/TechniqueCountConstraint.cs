namespace Sudoku.Filtering.Constraints;

/// <summary>
/// Represents a rule that checks whether the specified analyzer result after analyzed by a grid
/// contains the specified techniques.
/// </summary>
[ConstraintOptions(AllowsMultiple = true)]
public sealed class TechniqueCountConstraint : Constraint, IComparisonOperatorConstraint, ILimitCountConstraint<int>
{
	/// <inheritdoc/>
	public int LimitCount { get; set; }

	/// <inheritdoc/>
	public ComparisonOperator Operator { get; set; }

	/// <summary>
	/// Indicates the technique used.
	/// </summary>
	public Technique Technique { get; set; }


	/// <inheritdoc/>
	static int ILimitCountConstraint<int>.Minimum => 0;

	/// <inheritdoc/>
	static int ILimitCountConstraint<int>.Maximum => 20;


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Constraint? other)
		=> other is TechniqueCountConstraint comparer
		&& (LimitCount, Operator, Technique) == (comparer.LimitCount, comparer.Operator, comparer.Technique);

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(LimitCount, Operator, Technique);

	/// <inheritdoc/>
	public override string ToString(CultureInfo culture)
		=> string.Format(
			SR.Get("TechniqueCountConstraint", culture),
			[Technique.GetName(culture), Operator.OperatorString, LimitCount]
		);

	/// <inheritdoc/>
	public override TechniqueCountConstraint Clone()
		=> new() { IsNegated = IsNegated, LimitCount = LimitCount, Operator = Operator, Technique = Technique };

	/// <inheritdoc/>
	protected override bool CheckCore(ConstraintCheckingContext context)
	{
		var times = 0;
		foreach (var step in context.AnalysisResult)
		{
			if (Technique == step.Code)
			{
				times++;
			}
		}
		return Operator.OperatorInt32(times, LimitCount);
	}
}
