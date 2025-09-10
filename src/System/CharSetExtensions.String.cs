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
		/// Gets a new <see cref="string"/>[] result, with each element (a <see cref="string"/> with a single character)
		/// from the specified <see cref="string"/>.
		/// </summary>
		/// <returns>An array of <see cref="string"/> elements.</returns>
		public ReadOnlySpan<string> ExpandCharacters() => from c in @this.Span select c.ToString();

		/// <summary>
		/// Cut the array to multiple part, making them are all of length <paramref name="length"/>.
		/// </summary>
		/// <param name="length">The desired length.</param>
		/// <returns>A list of <see cref="string"/> values.</returns>
		public ReadOnlySpan<string> Chunk(int length)
		{
			var result = new string[@this.Length % length == 0 ? @this.Length / length : @this.Length / length + 1];
			for (var i = 0; i < @this.Length / length; i++)
			{
				result[i] = @this.Span.Slice(i * length, length).ToString();
			}
			return result;
		}


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
		public static ReadOnlySpan<string> operator /(string value, int parts) => Chunk(value, parts);

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
}
