namespace Sudoku.Concepts.ValueConversions;

/// <summary>
/// Represents tab-separated grid converter.
/// </summary>
public sealed class TabSeparatedGridConverter : IGridConverter
{
	/// <inheritdoc/>
	public int ParsingPriority => 4;


	/// <inheritdoc/>
	public bool TryFormat(ref readonly Grid value, IFormatProvider? provider, [NotNullWhen(true)] out string? result)
	{
		var span = value.ToString("0").Span;
		var sb = new StringBuilder(81 + 72 + 9);
		for (var i = 0; i < 9; i++)
		{
			for (var j = 0; j < 9; j++)
			{
				if (span[i * 9 + j] - '0' is var digit and not 0)
				{
					sb.Append(digit);
				}
				sb.Append('\t');
			}
			sb.RemoveFromEnd(1).AppendLine();
		}
		result = sb.RemoveFromEnd(Environment.NewLine.Length).ToString();
		return true;
	}

	/// <inheritdoc/>
	public bool TryParse(ReadOnlySpan<char> text, IFormatProvider? provider, out Grid result)
	{
		if (!text.Contains('\t'))
		{
			goto ReturnFalse;
		}

		var lineSplitCount = 0;
		var sb = new StringBuilder(81);
		foreach (var range in text.Split("\r\n"))
		{
			var value = text[range];
			var digitSplitCount = 0;
			foreach (var digitStringRange in value.Split('\t'))
			{
				var digitString = value[digitStringRange];
				sb.Append(digitStringRange.Start == digitStringRange.End ? '.' : digitString[0]);
				digitSplitCount++;
			}
			if (digitSplitCount != 9)
			{
				goto ReturnFalse;
			}

			lineSplitCount++;
		}
		if (lineSplitCount != 9)
		{
			goto ReturnFalse;
		}

		result = Grid.Parse(sb.ToString());
		return true;

	ReturnFalse:
		result = Grid.Undefined;
		return false;
	}
}
