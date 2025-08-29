namespace Sudoku.Keywords;

/// <summary>
/// Represents an exception that describes "invalid keyword converter type" information.
/// </summary>
public sealed class InvalidKeywordConverterTypeException : Exception
{
	/// <inheritdoc/>
	public override string Message
		=> string.Format(SR.Get("Message_InvalidKeywordConverterType"), typeof(KeywordValueConverter).FullName);
}
