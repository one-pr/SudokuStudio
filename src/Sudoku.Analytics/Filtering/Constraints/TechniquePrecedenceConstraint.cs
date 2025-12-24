namespace Sudoku.Filtering.Constraints;

/// <summary>
/// Represents a precedence (ordering relation) of a technique.
/// </summary>
[ConstraintOptions(AllowsMultiple = true)]
public sealed class TechniquePrecedenceConstraint : Constraint
{
	/// <summary>
	/// Indicates the target technqiue to be compared.
	/// </summary>
	public Technique TargetTechnique { get; set; }

	/// <summary>
	/// Indicates the techniques that the target technique should be compared.
	/// </summary>
	public Technique ComparedTechnique { get; set; }

	/// <summary>
	/// Indicates the operator.
	/// </summary>
	public PrecedenceOperator Operator { get; set; }


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Constraint? other)
		=> other is TechniquePrecedenceConstraint comparer
		&& TargetTechnique == comparer.TargetTechnique
		&& Operator == comparer.Operator && ComparedTechnique == comparer.ComparedTechnique;

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(TargetTechnique, ComparedTechnique, Operator);

	/// <inheritdoc/>
	public override string ToString(CultureInfo culture)
		=> string.Format(
			SR.Get("TechniquePrecedenceConstraint", culture),
			[
				TargetTechnique.GetName(culture),
				Operator switch
				{
					PrecedenceOperator.Predecessor => SR.Get("PredecessorText", culture),
					_ => SR.Get("SuccessorText", culture)
				},
				ComparedTechnique.GetName(culture)
			]
		);

	/// <inheritdoc/>
	public override TechniquePrecedenceConstraint Clone()
		=> new() { TargetTechnique = TargetTechnique, ComparedTechnique = ComparedTechnique, Operator = Operator };

	/// <inheritdoc/>
	protected override bool CheckCore(ConstraintCheckingContext context)
	{
		if (context.AnalysisResult is not { IsSolved: true, StepsSpan: var steps })
		{
			return false;
		}

		if (!steps.Any(step => step.Code == TargetTechnique) || !steps.Any(step => step.Code == ComparedTechnique))
		{
			return false;
		}

		var stepsIndexed = steps.Index();
		var indexOfStep = f(stepsIndexed, ComparedTechnique) is var stepsFiltered && Operator == PrecedenceOperator.Predecessor
			? stepsFiltered.Min()
			: stepsFiltered.Max();
		var t = Operator == PrecedenceOperator.Predecessor ? ^1 : 0;
		var targetTechniqueComparingIndex = f(stepsIndexed, TargetTechnique)[t];
		return Operator == PrecedenceOperator.Predecessor
			? targetTechniqueComparingIndex < indexOfStep
			: targetTechniqueComparingIndex > indexOfStep;


		static ReadOnlySpan<int> f(ReadOnlySpan<(int Index, Step Value)> original, Technique technique)
			=> from pair in original where pair.Value.Code == technique select pair.Index;
	}
}
