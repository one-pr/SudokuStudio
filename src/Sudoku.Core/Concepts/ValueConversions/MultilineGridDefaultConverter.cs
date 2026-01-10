namespace Sudoku.Concepts.ValueConversions;

/// <summary>
/// Represents a multiline grid converter, with block lines reserved parsing rule.
/// </summary>
public sealed class MultilineGridDefaultConverter : MultilineGridConverter
{
	/// <inheritdoc/>
	public override bool RemoveBlockLines => false;

	/// <inheritdoc/>
	public override int ParsingPriority => 9;
}
