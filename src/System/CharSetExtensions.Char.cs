namespace System;

public partial class CharSetExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="char"/>.
	/// </summary>
	extension(char @this)
	{
		/// <summary>
		/// Indicates the upper-casing of the current character.
		/// </summary>
		public char UpperCasing => char.ToUpper(@this);

		/// <summary>
		/// Indicates the lower-casing of the current character.
		/// </summary>
		public char LowerCasing => char.ToLower(@this);


#if EXTENSION_OPERATORS
#if false
		/// <summary>
		/// Repeats the specified string specified times.
		/// </summary>
		/// <param name="value">The string.</param>
		/// <param name="times">The repeating times.</param>
		/// <returns>The result string.</returns>
		[OverloadResolutionPriority(1)]
		public static string operator *(char value, int times)
		{
			var result = new char[times];
			result.AsSpan().Fill(value);
			return new(result);
		}
#endif
#endif
	}
}
