namespace Sudoku.Concepts.ValueConversions;

/// <summary>
/// Provides bitmap formatted cell map converter.
/// </summary>
public sealed class BitmapCellMapConverter : ICellMapConverter
{
	/// <inheritdoc/>
	public bool TryFormat(ref readonly CellMap value, IFormatProvider? provider, [NotNullWhen(true)] out string? result)
	{
		var r = (stackalloc char[81]);
		r.Fill('0');

		for (var cell = 0; cell < 81; cell++)
		{
			if (value.Contains(cell))
			{
				r[cell] = '1';
			}
		}
		result = r.ToString();
		return true;
	}

	/// <inheritdoc/>
	public bool TryParse(ReadOnlySpan<char> text, IFormatProvider? provider, out CellMap result)
	{
		result = CellMap.Empty;
		if (text.Length != 81)
		{
			return false;
		}

		for (var cell = 0; cell < 81; cell++)
		{
			var character = text[cell];
			if (character is '.' or '0')
			{
				continue;
			}

			if (text[cell] - '0' == 1)
			{
				result += cell;
				continue;
			}

			result = CellMap.Empty;
			return false;
		}
		return true;
	}
}
