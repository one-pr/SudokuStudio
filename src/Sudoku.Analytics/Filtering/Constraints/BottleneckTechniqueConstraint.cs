namespace Sudoku.Filtering.Constraints;

/// <summary>
/// Represents a constraint that determines whether bottleneck step represents the specified technique.
/// </summary>
[ConstraintOptions(AllowsMultiple = true, AllowsNegation = true)]
public sealed class BottleneckTechniqueConstraint : Constraint
{
	/// <summary>
	/// Indicates the filters to be used.
	/// </summary>
	public BottleneckFilter[] Filters { get; set; } = [
		new(TechniqueType.Direct, BottleneckType.SingleStepOnly),
		new(TechniqueType.Snyder, BottleneckType.HardestRating),
		new(TechniqueType.Advanced, BottleneckType.EliminationGroup)
	];

	/// <summary>
	/// Indicates the techniques selected.
	/// </summary>
	public TechniqueSet Techniques { get; set; } = [];

	private int FilterHashCode
	{
		get
		{
			var result = default(HashCode);
			foreach (var filter in Filters)
			{
				result.Add(filter);
			}
			return result.ToHashCode();
		}
	}


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Constraint? other)
		=> other is BottleneckTechniqueConstraint comparer
		&& Techniques == comparer.Techniques
		&& HashSet<BottleneckFilter>.CreateSetComparer().Equals([.. Filters], [.. comparer.Filters]);

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(Techniques, FilterHashCode);

	/// <inheritdoc/>
	public override string ToString(CultureInfo culture)
		=> string.Format(SR.Get("BottleneckTechniqueConstraint", culture), Techniques.ToString(culture));

	/// <inheritdoc/>
	public override BottleneckTechniqueConstraint Clone()
		=> new() { IsNegated = IsNegated, Techniques = Techniques[..], Filters = Filters[..] };

	/// <inheritdoc/>
	protected override bool CheckCore(ConstraintCheckingContext context)
		=> !!(Techniques & [.. from step in context.AnalysisResult.GetBottlenecks(Filters) select step.Code]);
}
