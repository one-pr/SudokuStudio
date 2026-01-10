namespace Sudoku.Concepts.ValueConversions;

/// <summary>
/// Provides mask grid converter.
/// </summary>
public sealed class MaskGridConverter : IGridConverter
{
	/// <inheritdoc/>
	public int ParsingPriority => -1;

	/// <summary>
	/// Indicates the separator used. By default it's comma <c>", "</c>.
	/// </summary>
	public string Separator { get; init; } = ", ";


	/// <inheritdoc/>
	public bool TryFormat(ref readonly Grid value, IFormatProvider? provider, [NotNullWhen(true)] out string? result)
	{
		var sb = new StringBuilder(400);
		foreach (var mask in value)
		{
			sb.Append(mask).Append(Separator);
		}
		result = sb.RemoveFromEnd(Separator.Length).ToString();
		return true;
	}

	/// <inheritdoc/>
	public bool TryParse(ReadOnlySpan<char> text, IFormatProvider? provider, out Grid result) => throw new NotSupportedException();
}
