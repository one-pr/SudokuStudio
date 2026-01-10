namespace Sudoku.Concepts.ValueConversions;

/// <summary>
/// Represents a Susser format converter, with default parsing rule.
/// </summary>
public sealed class SusserGridDefaultConverter : SusserGridConverter
{
	/// <inheritdoc/>
	public override bool ShortenSusser => false;

	/// <inheritdoc/>
	public override int ParsingPriority => 6;


	/// <inheritdoc/>
	protected override SusserGridDefaultConverter Clone()
		=> new()
		{
			IsCompatibleMode = IsCompatibleMode,
			WithCandidates = WithCandidates,
			WithModifiables = WithModifiables,
			NegateEliminationsTripletRule = NegateEliminationsTripletRule,
			Placeholder = Placeholder,
			TreatValueAsGiven = TreatValueAsGiven,
			OnlyEliminations = OnlyEliminations
		};
}
