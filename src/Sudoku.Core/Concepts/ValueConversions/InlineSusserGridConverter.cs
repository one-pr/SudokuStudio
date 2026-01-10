namespace Sudoku.Concepts.ValueConversions;

/// <summary>
/// Represents inline Susser formatted grid converter.
/// </summary>
public sealed partial class InlineSusserGridConverter : IGridConverter
{
	/// <summary>
	/// Indicates the plus token that describes for modifiable values.
	/// </summary>
	private const string PlusToken = "+";


	/// <summary>
	/// <para>
	/// Indicates whether the parser will negate the rule, treating all digits as candidates existing in the grid
	/// instead of removed ones.
	/// </para>
	/// <para>The default value is <see langword="false"/>.</para>
	/// </summary>
	public bool NegateEliminationsTripletRule { get; init; }

	/// <inheritdoc/>
	public int ParsingPriority => -1;


	[GeneratedRegex("""(\+?[\d\.]|\[[1-9]{1,9}\])""", RegexOptions.Compiled, 5000)]
	public static partial Regex GridSusserPattern { get; }


	/// <inheritdoc/>
	public bool TryFormat(ref readonly Grid value, IFormatProvider? provider, [NotNullWhen(true)] out string? result)
	{
		var sb = new StringBuilder();
		for (var cell = 0; cell < 81; cell++)
		{
			_ = value.GetState(cell) switch
			{
				CellState.Empty when (NegateEliminationsTripletRule ? (Mask)0 : value.GetCandidates(cell)) is var digitsMask
					=> sb.Append(digitsMask == 0 ? "0" : $"[{new(from digit in digitsMask.AllSets select (char)(digit + '1'))}]"),
				CellState.Modifiable => sb.Append($"+{value.GetDigit(cell) + 1}"),
				CellState.Given => sb.Append(value.GetDigit(cell) + 1),
				_ => throw new FormatException()
			};
		}
		result = sb.ToString();
		return true;
	}

	/// <inheritdoc/>
	public bool TryParse(ReadOnlySpan<char> text, IFormatProvider? provider, out Grid result)
	{
		var match = GridSusserPattern.Matches(text.ToString());
		if (match is not { Count: 81 } captures)
		{
			goto ReturnFalse;
		}

		result = Grid.Empty;
		for (var cell = 0; cell < 81; cell++)
		{
			switch (captures[cell].Value)
			{
				case [.. var token, var digitChar and >= '1' and <= '9']:
				{
					var state = token switch { PlusToken => CellState.Modifiable, "" => CellState.Given, _ => default };
					if (state is not (CellState.Given or CellState.Modifiable))
					{
						goto ReturnFalse;
					}

					var digit = digitChar - '1';
					result.SetDigit(cell, digit);
					result.SetState(cell, state);
					break;
				}
				case ['0' or '.']:
				{
					continue;
				}
				case ['[', .. { Length: <= 9 } digitsStr, ']']:
				{
					var digits = Mask.Create(from c in digitsStr select c - '1');
					if (!NegateEliminationsTripletRule)
					{
						// This applies for normal rule - removing candidates marked.
						foreach (var digit in digits)
						{
							// Set the candidate with false to eliminate the candidate.
							result.SetExistence(cell, digit, false);
						}
					}
					else
					{
						// If negate candidates, we should remove all possible candidates from all empty cells, making the grid invalid firstly.
						// Then we should add candidates onto the grid to make the grid valid.
						result[cell] = (Mask)(Grid.EmptyMask | digits);
					}
					break;
				}
				default:
				{
					goto ReturnFalse;
				}
			}
		}
		return true;

	ReturnFalse:
		result = Grid.Undefined;
		return false;
	}
}
