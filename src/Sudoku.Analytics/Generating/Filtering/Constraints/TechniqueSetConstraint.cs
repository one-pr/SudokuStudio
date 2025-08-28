namespace Sudoku.Generating.Filtering.Constraints;

/// <summary>
/// Represents a constraint that limits a puzzle that can only use such techniques to be finished.
/// </summary>
public sealed class TechniqueSetConstraint : Constraint
{
	/// <summary>
	/// Indicates the technique used.
	/// </summary>
	public TechniqueSet Techniques { get; set; } = TechniqueSet.None;


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Constraint? other)
		=> other is TechniqueSetConstraint comparer && Techniques == comparer.Techniques;

	/// <inheritdoc/>
	public override int GetHashCode() => Techniques.GetHashCode();

	/// <inheritdoc/>
	public override string ToString(IFormatProvider? formatProvider)
	{
		var culture = formatProvider as CultureInfo;
		return string.Format(SR.Get("TechniqueSetConstraint", culture), Techniques.ToString(culture));
	}

	/// <inheritdoc/>
	public override TechniqueSetConstraint Clone() => new() { Techniques = Techniques };

	/// <inheritdoc/>
	protected override bool CheckCore(ConstraintCheckingContext context)
	{
		ref readonly var grid = ref context.Grid;
		return Techniques switch
		{
			[Technique.FullHouse] => grid.CanPrimaryFullHouse,
			[Technique.CrosshatchingBlock or Technique.HiddenSingleBlock] => grid.CanPrimaryHiddenSingle(false),
			[Technique.CrosshatchingRow or Technique.CrosshatchingColumn or Technique.HiddenSingleRow or Technique.HiddenSingleColumn]
				=> grid.CanPrimaryHiddenSingle(true),
			[Technique.NakedSingle] => grid.CanPrimaryNakedSingle,
			_ => b(context)
		};


		bool b(ConstraintCheckingContext context)
		{
			foreach (var step in context.AnalysisResult)
			{
				if (!Techniques.Contains(step.Code))
				{
					return false;
				}
			}
			return true;
		}
	}
}
