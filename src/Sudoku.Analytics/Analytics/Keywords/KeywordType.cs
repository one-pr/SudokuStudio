namespace Sudoku.Analytics.Keywords;

/// <summary>
/// Represents a keyword type.
/// </summary>
public enum KeywordType
{
	/// <summary>
	/// Indicates the keyword type is unknown. The type should be parsed if matched;
	/// otherwise, you should configure them using <see cref="KeywordAttribute"/>.
	/// </summary>
	Unknown,

	/// <summary>
	/// Indicates the number.
	/// </summary>
	Number,

	/// <summary>
	/// Indicates the string.
	/// </summary>
	String
}
