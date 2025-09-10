namespace System;

public partial class CharSetExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="string"/>.
	/// </summary>
	extension(string @this)
	{
		/// <summary>
		/// Indicates the upper-casing of the current string.
		/// </summary>
		public string UpperCasing => @this.ToUpper();

		/// <summary>
		/// Indicates the lower-casing of the current string.
		/// </summary>
		public string LowerCasing => @this.ToLower();

		/// <summary>
		/// Indicates the representation of type <see cref="ReadOnlySpan{T}"/> of <see cref="char"/>.
		/// </summary>
		public ReadOnlySpan<char> Span => @this.AsSpan();


		/// <summary>
		/// Removes all specified characters.
		/// </summary>
		/// <param name="character">The character to be removed from the base string.</param>
		/// <returns>The result string value after removal.</returns>
		public string RemoveAll(char character) => @this.Replace(character.ToString(), string.Empty);


		/// <summary>
		/// Unpacks a string into multiple characters.
		/// For example, <c>-"hello!"</c>will become <c>['h', 'e', 'l', 'l', 'o', '!']</c>.
		/// </summary>
		/// <param name="str">The string.</param>
		/// <returns>A sequence of characters.</returns>
		public static ReadOnlySpan<char> operator -(string str) => str.ToCharArray();

		/// <summary>
		/// Repeats the specified string specified times.
		/// </summary>
		/// <param name="value">The string.</param>
		/// <param name="times">The repeating times.</param>
		/// <returns>The result string.</returns>
		public static string operator *(string value, int times)
		{
			var result = new char[value.Length * times];
			var span = result.AsSpan();
			for (var i = 0; i < result.Length; i += value.Length)
			{
				value.CopyTo(span[i..(i + value.Length)]);
			}
			return new(result);
		}

		/// <summary>
		/// Splits the string into specified parts, as same length as possible.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="parts">The desired parts.</param>
		/// <returns>The sequence of strings.</returns>
		public static ReadOnlySpan<string> operator /(string value, int parts)
		{
			var result = new string[(value.Length + parts - 1) / parts];
			for (var i = 0; i < value.Length / parts; i++)
			{
				result[i] = value.Span.Slice(i * parts, parts).ToString();
			}
			return result;
		}

		/// <summary>
		/// Splits a string into substrings based on a specified delimiting character.
		/// </summary>
		/// <param name="value">The current string instance.</param>
		/// <param name="separator">A character that delimits the substrings in this string.</param>
		/// <returns>
		/// An array whose elements contain the substrings from this instance that are delimited by separator.
		/// </returns>
		public static ReadOnlySpan<string> operator /(string value, char separator) => value.Split(separator);

		/// <summary>
		/// Splits a string into substrings based on a specified delimiting string.
		/// </summary>
		/// <param name="value">The current string instance.</param>
		/// <param name="separator">A string that delimits the substrings in this string.</param>
		/// <returns>
		/// An array whose elements contain the substrings from this instance that are delimited by separator.
		/// </returns>
		public static ReadOnlySpan<string> operator /(string value, string separator) => value.Split(separator);
	}

	/// <summary>
	/// Provides extension members on <see cref="ReadOnlySpan{T}"/> of <see cref="string"/>.
	/// </summary>
	extension(ReadOnlySpan<string>)
	{
		/// <summary>
		/// Pack strings into a string. For example, <c>+["hello", ", ", "world", "!"]</c> will become <c>"hello, world!"</c>.
		/// </summary>
		/// <param name="strings">Strings.</param>
		/// <returns>A string.</returns>
		public static string operator +(ReadOnlySpan<string> strings) => string.Concat(strings);
	}
}
