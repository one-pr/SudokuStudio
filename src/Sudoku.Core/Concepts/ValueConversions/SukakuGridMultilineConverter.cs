namespace Sudoku.Concepts.ValueConversions;

/// <summary>
/// Represents Sukaku grid converter, with multiple-line parsing rule.
/// </summary>
public sealed class SukakuGridMultilineConverter : SukakuGridConverter
{
	/// <inheritdoc/>
	public override bool Multiline => true;

	/// <inheritdoc/>
	public override int ParsingPriority => 1;


	/// <inheritdoc/>
	public override bool TryFormat(ref readonly Grid value, IFormatProvider? provider, [NotNullWhen(true)] out string? result)
	{
		// Append all digits.
		var builders = new StringBuilder[81];
		for (var i = 0; i < 81; i++)
		{
			builders[i] = new();
			foreach (var digit in value.GetCandidates(i))
			{
				builders[i].Append(digit + 1);
			}
		}

		// Now consider the alignment for each column of output text.
		var sb = new StringBuilder();
		var span = (stackalloc int[9]);
		for (var column = 0; column < 9; column++)
		{
			var maxLength = 0;
			for (var p = 0; p < 9; p++)
			{
				maxLength = Math.Max(maxLength, builders[p * 9 + column].Length);
			}

			span[column] = maxLength;
		}
		for (var row = 0; row < 9; row++)
		{
			for (var column = 0; column < 9; column++)
			{
				var cell = row * 9 + column;
				sb.Append(builders[cell].ToString().PadLeft(span[column])).Append(' ');
			}
			sb.RemoveFromEnd(1).AppendLine(); // Remove last whitespace.
		}

		result = sb.ToString();
		return true;
	}

	/// <inheritdoc/>
	public override bool TryParse(ReadOnlySpan<char> text, IFormatProvider? provider, out Grid result)
	{
		var matches = from m in GridSukakuSegmentPattern.Matches(text.ToString()) select m.Value;
		if (matches is { Length: not 81 })
		{
			goto ReturnFalse;
		}

		result = Grid.Empty;
		for (var offset = 0; offset < 81; offset++)
		{
			var s = Regex.Replace(matches[offset], @"\d", static _ => string.Empty);
			if (s.Length > 9)
			{
				// More than 9 characters.
				goto ReturnFalse;
			}

			var mask = (Mask)0;
			foreach (var c in s)
			{
				mask |= (Mask)(1 << c - '1');
			}

			if (mask == 0)
			{
				goto ReturnFalse;
			}

			for (var digit = 0; digit < 9; digit++)
			{
				result.SetExistence(offset, digit, (mask >> digit & 1) != 0);
			}
		}
		return true;

	ReturnFalse:
		result = Grid.Undefined;
		return false;
	}
}
