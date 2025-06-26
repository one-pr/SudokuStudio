namespace Sudoku.Drawing.Parsing;

/// <summary>
/// Represents truth set argument parser.
/// </summary>
internal sealed class TruthSetArgumentParser : RankSetArgumentParser
{
	/// <inheritdoc/>
	public override bool IsTruthBased => true;
}
