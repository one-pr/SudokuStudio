namespace Sudoku.Filtering.Constraints;

/// <summary>
/// Represents a conclusion constraint.
/// </summary>
[ConstraintOptions(AllowsMultiple = true)]
public sealed class ConclusionConstraint : Constraint
{
	/// <summary>
	/// Indicates whether the conclusion should be appeared.
	/// </summary>
	public bool ShouldAppear { get; set; }

	/// <summary>
	/// Indicates the conclusion.
	/// </summary>
	public Conclusion Conclusion { get; set; }


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Constraint? other)
		=> other is ConclusionConstraint comparer && (Conclusion, ShouldAppear) == (comparer.Conclusion, comparer.ShouldAppear);

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(ShouldAppear, Conclusion);

	/// <inheritdoc/>
	public override string ToString(CultureInfo culture)
		=> string.Format(
			SR.Get("ConclusionConstraint", culture),
			[
				Conclusion.ToString(culture),
				ShouldAppear ? string.Empty : SR.Get("NoString", culture)
			]
		);

	/// <inheritdoc/>
	public override ConclusionConstraint Clone()
		=> new() { IsNegated = IsNegated, Conclusion = Conclusion, ShouldAppear = ShouldAppear };

	/// <inheritdoc/>
	protected override bool CheckCore(ConstraintCheckingContext context)
	{
		var appeared = false;
		foreach (var step in context.AnalysisResult)
		{
			foreach (var conclusion in step.Conclusions)
			{
				if (Conclusion == conclusion)
				{
					appeared = true;
					break;
				}
			}
		}
		return !(ShouldAppear ^ appeared);
	}
}
