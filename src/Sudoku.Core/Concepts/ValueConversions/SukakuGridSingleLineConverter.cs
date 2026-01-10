namespace Sudoku.Concepts.ValueConversions;

/// <summary>
/// Represents Sukaku grid converter, with single-line parsing rule.
/// </summary>
public sealed class SukakuGridSingleLineConverter : SukakuGridConverter
{
	/// <inheritdoc/>
	public override bool Multiline => false;

	/// <inheritdoc/>
	public override int ParsingPriority => 2;


	/// <inheritdoc/>
	public override bool TryFormat(ref readonly Grid value, IFormatProvider? provider, [NotNullWhen(true)] out string? result)
	{
		var sb = new StringBuilder();
		for (var i = 0; i < 81; i++)
		{
			sb.Append("123456789");
		}

		for (var i = 0; i < 729; i++)
		{
			if (!value.GetExistence(i / 9, i % 9))
			{
				sb[i] = Placeholder;
			}
		}

		result = sb.ToString();
		return true;
	}

	/// <inheritdoc/>
	public override bool TryParse(ReadOnlySpan<char> text, IFormatProvider? provider, out Grid result)
	{
		if (text.Length < 729)
		{
			goto ReturnFalse;
		}

		result = Grid.Empty;
		for (var i = 0; i < 729; i++)
		{
			var c = text[i];
			if (c is not (>= '0' and <= '9' or '.'))
			{
				goto ReturnFalse;
			}

			if (c is '0' or '.')
			{
				result.SetExistence(i / 9, i % 9, false);
			}
		}
		return true;

	ReturnFalse:
		result = Grid.Undefined;
		return false;
	}
}
