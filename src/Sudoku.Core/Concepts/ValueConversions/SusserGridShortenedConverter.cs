namespace Sudoku.Concepts.ValueConversions;

/// <summary>
/// Represents a Susser format converter, with shortened parsing rule.
/// </summary>
public sealed class SusserGridShortenedConverter : SusserGridConverter
{
	/// <inheritdoc/>
	public override bool ShortenSusser => true;

	/// <inheritdoc/>
	public override int ParsingPriority => 5;


	/// <inheritdoc/>
	protected override SusserGridShortenedConverter Clone()
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
