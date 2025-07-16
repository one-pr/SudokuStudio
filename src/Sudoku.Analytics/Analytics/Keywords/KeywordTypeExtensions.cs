namespace Sudoku.Analytics.Keywords;

/// <summary>
/// Provides with extension methods on <see cref="KeywordType"/>.
/// </summary>
public static class KeywordTypeExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="KeywordType"/>.
	/// </summary>
	extension(KeywordType @this)
	{
		/// <summary>
		/// Indicates allowed verbs.
		/// </summary>
		public KeywordVerbs AllowedVerbs
			=> @this switch
			{
				KeywordType.Number => KeywordVerbs.NumberComparison | KeywordVerbs.NumberRange,
				KeywordType.String => KeywordVerbs.StringEqualityComparison | KeywordVerbs.StringPattern,
				_ => KeywordVerbs.None
			};
	}
}
