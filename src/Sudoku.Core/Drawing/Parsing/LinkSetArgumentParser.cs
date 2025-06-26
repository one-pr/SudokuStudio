namespace Sudoku.Drawing.Parsing;

/// <summary>
/// Represents link set argument parser.
/// </summary>
internal sealed class LinkSetArgumentParser : RankSetArgumentParser
{
	/// <inheritdoc/>
	public override bool IsTruthBased => false;
}
