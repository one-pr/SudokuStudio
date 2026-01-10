namespace Sudoku.Concepts.ValueConversions;

/// <summary>
/// Represents a format converter for Sukaku puzzles.
/// A Sukaku puzzle is a kind of sudoku variant, with all cells empty cells with candidates instead.
/// </summary>
public abstract partial class SukakuGridConverter : IGridConverter
{
	/// <inheritdoc/>
	public abstract int ParsingPriority { get; }

	/// <summary>
	/// Indicates whether the formatter will handle the value with multiple-line mode.
	/// </summary>
	public abstract bool Multiline { get; }

	/// <summary>
	/// Indicates the placeholder of the grid text formatter.
	/// </summary>
	/// <value>The new placeholder text character to be set. The value must be <c>'.'</c> or <c>'0'</c>.</value>
	/// <returns>The placeholder text.</returns>
	public char Placeholder { get; init; }


	[GeneratedRegex("""\d*[\-\+]?\d+""", RegexOptions.Compiled, 5000)]
	public static partial Regex GridSukakuSegmentPattern { get; }


	/// <inheritdoc/>
	public abstract bool TryFormat(ref readonly Grid value, IFormatProvider? provider, [NotNullWhen(true)] out string? result);

	/// <inheritdoc/>
	public abstract bool TryParse(ReadOnlySpan<char> text, IFormatProvider? provider, out Grid result);
}
