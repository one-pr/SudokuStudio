namespace Sudoku.Keywords;

/// <summary>
/// Represents an exception that describes "invalid keyword" information.
/// </summary>
public sealed class InvalidKeywordException : Exception
{
	/// <inheritdoc/>
	public override string Message => SR.Get("Message_InvalidKeyword");
}
