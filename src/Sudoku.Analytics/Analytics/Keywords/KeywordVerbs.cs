namespace Sudoku.Analytics.Keywords;

/// <summary>
/// Represents a verb of keyword filtering rule.
/// </summary>
/// <remarks><include file="../../global-doc-comments.xml" path="/g/flags-attribute"/></remarks>
[Flags]
public enum KeywordVerbs
{
	/// <summary>
	/// The placeholder of the verb enumeration type.
	/// </summary>
	None = 0,

	/// <summary>
	/// Indicates the verb is to compare string equality.
	/// </summary>
	StringEqualityComparison = 1 << 0,

	/// <summary>
	/// Indicates the verb is to check regular expression pattern of a string.
	/// </summary>
	StringPattern = 1 << 1,

	/// <summary>
	/// Indicates the verb is to compare number equality, also includes inequality operators like greater-than <c>&gt;=</c>.
	/// </summary>
	NumberComparison = 1 << 2,

	/// <summary>
	/// Indicates the verb is to check whether a number is in a range.
	/// </summary>
	NumberRange = 1 << 3
}
