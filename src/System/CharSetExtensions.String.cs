namespace System;

public partial class CharSetExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="string"/>.
	/// </summary>
	extension(string @this)
	{
		/// <summary>
		/// Indicates the upper-cased string to the current string.
		/// </summary>
		public string UpperCased => @this.ToUpper();

		/// <summary>
		/// Indicates the lower-cased string to the current string.
		/// </summary>
		public string LowerCased => @this.ToLower();

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

		/// <inheritdoc cref="op_UnaryNegation(string)"/>
		[Obsolete(DeprecatedMessages.ExtensionOperator_Unpack, false)]
		public ReadOnlySpan<char> Unpack() => -@this;


		/// <inheritdoc cref="op_Multiply(string, int)"/>
		[Obsolete(DeprecatedMessages.ExtensionOperator_Repeat, false)]
		public static string Repeat(string value, int times) => value * times;

		/// <inheritdoc cref="op_Division(string, int)"/>
		[Obsolete(DeprecatedMessages.ExtensionOperator_Chunk, false)]
		public static ReadOnlySpan<string> Chunk(string value, int size) => value / size;


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
		/// <param name="size">The desired parts.</param>
		/// <returns>The sequence of strings.</returns>
		public static ReadOnlySpan<string> operator /(string value, int size)
		{
			var result = new string[(value.Length + size - 1) / size];
			for (var i = 0; i < value.Length / size; i++)
			{
				result[i] = value.Span.Slice(i * size, size).ToString();
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
	extension(ReadOnlySpan<string> @this)
	{
		/// <inheritdoc cref="op_UnaryPlus(ReadOnlySpan{string})"/>
		[Obsolete(DeprecatedMessages.ExtensionOperator_Pack, false)]
		public string Pack() => +@this;


		/// <summary>
		/// Pack strings into a string. For example, <c>+["hello", ", ", "world", "!"]</c> will become <c>"hello, world!"</c>.
		/// </summary>
		/// <param name="strings">Strings.</param>
		/// <returns>A string.</returns>
		public static string operator +(ReadOnlySpan<string> strings) => string.Concat(strings);
	}
}
