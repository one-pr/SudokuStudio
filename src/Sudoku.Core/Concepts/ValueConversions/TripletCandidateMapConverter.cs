namespace Sudoku.Concepts.ValueConversions;

/// <summary>
/// Provides triplet formatted candidate map converter.
/// </summary>
public sealed class TripletCandidateMapConverter : ICandidateMapConverter
{
	/// <inheritdoc/>
	public bool TryFormat(ref readonly CandidateMap value, IFormatProvider? provider, [NotNullWhen(true)] out string? result)
	{
		result = value switch { [] => string.Empty, [var p] => $"{p % 9 + 1}{p / 9 / 9 + 1}{p / 9 % 9 + 1}", _ => f(value) };
		return true;


		static string f(in CandidateMap map)
		{
			var sb = new StringBuilder();
			foreach (var candidate in map)
			{
				var (cell, digit) = (candidate / 9, candidate % 9);
				sb.Append($"{digit + 1}{cell / 9 + 1}{cell % 9 + 1} ");
			}
			return sb.RemoveFromEnd(1).ToString();
		}
	}

	/// <inheritdoc/>
	public bool TryParse(ReadOnlySpan<char> text, IFormatProvider? provider, [NotNullWhen(true)] out CandidateMap result)
	{
		result = CandidateMap.Empty;
		foreach (var segmentRange in text.Split(' '))
		{
			var segment = text[segmentRange];
			if (segment is [var d and >= '1' and <= '9', var r and >= '1' and <= '9', var c and >= '1' and <= '9'])
			{
				result += ((r - '1') * 9 + c - '1') * 9 + d - '1';
				continue;
			}

			result = CandidateMap.Empty;
			return false;
		}
		return true;
	}
}
