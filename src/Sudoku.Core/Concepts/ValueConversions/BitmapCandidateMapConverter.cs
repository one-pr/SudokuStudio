namespace Sudoku.Concepts.ValueConversions;

/// <summary>
/// Represents bitmap formatted candidate map converter.
/// </summary>
public sealed class BitmapCandidateMapConverter : ICandidateMapConverter
{
	/// <inheritdoc/>
	public bool TryFormat(ref readonly CandidateMap value, IFormatProvider? provider, [NotNullWhen(true)] out string? result)
	{
		var r = (stackalloc char[729]);
		r.Fill('0');

		for (var cell = 0; cell < 729; cell++)
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
	public bool TryParse(ReadOnlySpan<char> text, IFormatProvider? provider, out CandidateMap result)
	{
		result = CandidateMap.Empty;
		if (text.Length != 729)
		{
			return false;
		}
		
		for (var candidate = 0; candidate < 729; candidate++)
		{
			var character = text[candidate];
			if (character is '.' or '0')
			{
				continue;
			}

			if (text[candidate] - '0' == 1)
			{
				result += candidate;
				continue;
			}

			result = CandidateMap.Empty;
			return false;
		}
		return true;
	}
}
