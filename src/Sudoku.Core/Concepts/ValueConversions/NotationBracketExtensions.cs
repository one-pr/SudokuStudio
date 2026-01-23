namespace Sudoku.Concepts.ValueConversions;

/// <summary>
/// Provides with extension methods on <see cref="NotationBracket"/>.
/// </summary>
/// <seealso cref="NotationBracket"/>
public static class NotationBracketExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(NotationBracket @this)
	{
		/// <summary>
		/// Indicates to get open bracket token.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">Throws when the argument is not defined.</exception>
		public string? OpenBracket
			=> @this switch
			{
				NotationBracket.None => null,
				NotationBracket.Round => "(",
				NotationBracket.Square => "[",
				NotationBracket.Curly => "{",
				NotationBracket.Angle => "<",
				_ => throw new ArgumentOutOfRangeException(nameof(@this))
			};

		/// <summary>
		/// Indicates closed bracket token.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">Throws when the argument is not defined.</exception>
		public string? ClosedBracket
			=> @this switch
			{
				NotationBracket.None => null,
				NotationBracket.Round => ")",
				NotationBracket.Square => "]",
				NotationBracket.Curly => "}",
				NotationBracket.Angle => ">",
				_ => throw new ArgumentOutOfRangeException(nameof(@this))
			};
	}
}
