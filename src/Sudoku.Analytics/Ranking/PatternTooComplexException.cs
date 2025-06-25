namespace Sudoku.Ranking;

/// <summary>
/// Represents too complex exception.
/// </summary>
public sealed class PatternTooComplexException : Exception
{
	/// <inheritdoc/>
	public override string Message => SR.ExceptionMessage("Message_RankPatternIsTooComplex");
}
