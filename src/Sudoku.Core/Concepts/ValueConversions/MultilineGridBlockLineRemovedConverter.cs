namespace Sudoku.Concepts.ValueConversions;

/// <summary>
/// Represents a multiline grid converter, with block lines removed parsing rule.
/// </summary>
public sealed class MultilineGridBlockLineRemovedConverter : MultilineGridConverter
{
	/// <inheritdoc/>
	public override bool RemoveBlockLines => true;

	/// <inheritdoc/>
	public override int ParsingPriority => 8;
}
