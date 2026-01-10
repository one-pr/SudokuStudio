namespace Sudoku.Concepts.ValueConversions;

/// <summary>
/// Represents pencilmark grid converter.
/// </summary>
public sealed partial class PencilmarkGridConverter : IGridConverter
{
	/// <summary>
	/// Indicates whether the formatter will subtle grid lines to make a good look. By default it's <see langword="true"/>.
	/// </summary>
	public bool SubtleGridLines { get; init; } = true;

	/// <summary>
	/// Indicates whether the formatter will treat all values as givens, regardless of its value state.
	/// </summary>
	public bool TreatValueAsGiven { get; init; }

	/// <summary>
	/// Indicates whether the formatter will use compatible mode to output grid values.
	/// </summary>
	public bool IsCompatibleMode { get; init; }

	/// <inheritdoc/>
	public int ParsingPriority => 7;


	[GeneratedRegex("""\<\d\>|\*\d\*|\d*[\+\-]?\d+""", RegexOptions.Compiled)]
	public static partial Regex GridPencilmarkPattern { get; }

	[GeneratedRegex("""[1-9\+\-]{1,9}""", RegexOptions.Compiled)]
	private static partial Regex ValueCellPattern { get; }


	/// <inheritdoc/>
	public bool TryFormat(ref readonly Grid value, IFormatProvider? provider, [NotNullWhen(true)] out string? result)
	{
		// Step 1: gets the candidates information grouped by columns.
		var valuesByColumn = createTempDictionary();
		var valuesByRow = createTempDictionary();

		for (var i = 0; i < 81; i++)
		{
			var mask = value[i];
			valuesByRow[i / 9].Add(mask);
			valuesByColumn[i % 9].Add(mask);
		}

		// Step 2: gets the maximal number of candidates in a cell, used for aligning by columns.
		const int bufferLength = 9;
		var maxLengths = (stackalloc int[bufferLength]);
		maxLengths.Clear();

		foreach (var (i, _) in valuesByColumn)
		{
			ref var maxLength = ref maxLengths[i];

			// Iteration on row index.
			for (var j = 0; j < 9; j++)
			{
				// Gets the number of candidates.
				var candidatesCount = 0;
				var mask = valuesByColumn[i][j];

				// Iteration on each candidate.
				// Counts the number of candidates.
				candidatesCount += PopCount((uint)mask);

				// Compares the values.
				var comparer = Math.Max(
					candidatesCount,
					MaskToCellState(mask) switch
					{
						// The output will be '<digit>' and consist of 3 characters.
						CellState.Given => Math.Max(candidatesCount, IsCompatibleMode ? 1 : 3),

						// The output will be '*digit*' and consist of 3 characters.
						CellState.Modifiable => Math.Max(candidatesCount, IsCompatibleMode ? 1 : 3),

						// Normal output: 'series' (at least 1 character).
						_ => candidatesCount
					}
				);
				if (comparer > maxLength)
				{
					maxLength = comparer;
				}
			}
		}

		// Step 3: prints all characters.
		var sb = new StringBuilder();
		var printingOperationDictionary = new Dictionary<(int LineIndex, bool SubtleGridLines), Action<Span<int>>>
		{
			// Print tabs of the first line.
			{ (0, true), maxLengths => printTabLines(sb, '.', '.', '-', maxLengths) },
			{ (0, false), maxLengths => printTabLines(sb, '+', '+', '-', maxLengths) },

			// Print tabs of mediate lines.
			{ (4, true), maxLengths => printTabLines(sb, ':', '+', '-', maxLengths) },
			{ (4, false), maxLengths => printTabLines(sb, '+', '+', '-', maxLengths) },
			{ (8, true), maxLengths => printTabLines(sb, ':', '+', '-', maxLengths) },
			{ (8, false), maxLengths => printTabLines(sb, '+', '+', '-', maxLengths) },

			// Print tabs of the foot line.
			{ (12, true), maxLengths => printTabLines(sb, '\'', '\'', '-', maxLengths) },
			{ (12, false), maxLengths => printTabLines(sb, '+', '+', '-', maxLengths) }
		};
		for (var i = 0; i < 13; i++)
		{
			if (printingOperationDictionary.TryGetValue((i, SubtleGridLines), out var action))
			{
				action(maxLengths);
			}
			else
			{
				defaultPrinting(sb, valuesByRow[(int)Math.Floor(3F * i / 4)], '|', '|', maxLengths);
			}
		}

		// The last step: returns the value.
		sb.RemoveFromEnd(Environment.NewLine.Length);
		result = sb.ToString();
		return true;


		void defaultPrinting(StringBuilder sb, List<Mask> valuesByRow, char c1, char c2, Span<int> maxLengths)
		{
			sb.Append(c1);
			printValues(sb, valuesByRow, 0, 2, maxLengths);
			sb.Append(c2);
			printValues(sb, valuesByRow, 3, 5, maxLengths);
			sb.Append(c2);
			printValues(sb, valuesByRow, 6, 8, maxLengths);
			sb.Append(c1);
			sb.AppendLine();
		}

		void printValues(StringBuilder sb, List<Mask> valuesByRow, int start, int end, Span<int> maxLengths)
		{
			sb.Append(' ');
			for (var i = start; i <= end; i++)
			{
				// Get digit.
				var value = valuesByRow[i];
				var state = MaskToCellState(value);

				value &= Grid.MaxCandidatesMask;
				var d = value == 0 ? -1 : (state != CellState.Empty ? TrailingZeroCount(value) : -1) + 1;
				var s = (state, TreatValueAsGiven, IsCompatibleMode) switch
				{
					(CellState.Given or CellState.Modifiable, _, true) => d.ToString(),
					(CellState.Given, _, _) or (CellState.Modifiable, true, _) => $"<{d}>",
					(CellState.Modifiable, false, _) => $"*{d}*",
					_ => appendingMask(value)
				};

				sb.Append(s.PadRight(maxLengths[i]));
				sb.Append(i != end ? "  " : " ");
			}


			static string appendingMask(Mask mask)
			{
				var sb = new StringBuilder(9);
				foreach (var digit in mask)
				{
					sb.Append(digit + 1);
				}
				return sb.ToString();
			}
		}

		static void printTabLines(StringBuilder sb, char c1, char c2, char fillingChar, Span<int> m)
			=> sb
				.Append(c1)
				.Append(string.Empty.PadRight(m[0] + m[1] + m[2] + 6, fillingChar))
				.Append(c2)
				.Append(string.Empty.PadRight(m[3] + m[4] + m[5] + 6, fillingChar))
				.Append(c2)
				.Append(string.Empty.PadRight(m[6] + m[7] + m[8] + 6, fillingChar))
				.Append(c1)
				.AppendLine();

		static Dictionary<Digit, List<Mask>> createTempDictionary()
			=> new(from digit in Enumerable.Range(0, 9) select KeyValuePair.Create(digit, new List<Mask>()));
	}

	/// <inheritdoc/>
	public bool TryParse(ReadOnlySpan<char> text, IFormatProvider? provider, out Grid result)
	{
		// Older regular expression pattern:
		if ((from m in GridPencilmarkPattern.Matches(text.ToString()) select m.Value) is not { Length: 81 } matches)
		{
			goto ReturnFalse;
		}

		result = Grid.Empty;
		for (var cell = 0; cell < 81; cell++)
		{
			if (matches[cell] is not { Length: var length and <= 9 } s)
			{
				// More than 9 characters.
				goto ReturnFalse;
			}

			if (s.Contains('<'))
			{
				// All values will be treated as normal characters: '<digit>', '*digit*' and 'candidates'.

				// Givens.
				if (length == 3)
				{
					if (s[1] is var c and >= '1' and <= '9')
					{
						result.SetDigit(cell, c - '1');
						result.SetState(cell, CellState.Given);
					}
					else
					{
						// Illegal characters found.
						goto ReturnFalse;
					}
				}
				else
				{
					// The length is not 3.
					goto ReturnFalse;
				}
			}
			else if (s.Contains('*'))
			{
				// Modifiables.
				if (length == 3)
				{
					if (s[1] is var c and >= '1' and <= '9')
					{
						result.SetDigit(cell, c - '1');
						result.SetState(cell, CellState.Modifiable);
					}
					else
					{
						// Illegal characters found.
						goto ReturnFalse;
					}
				}
				else
				{
					// The length is not 3.
					goto ReturnFalse;
				}
			}
			else if (ValueCellPattern.Match(s) is { Success: true, Value: var possibleStringMatched } && possibleStringMatched == s)
			{
				// Candidates.
				// Here don't need to check the length of the string, and also all characters are digit characters.
				var mask = (Mask)0;
				foreach (var c in s)
				{
					if (c is not ('+' or '-'))
					{
						mask |= (Mask)(1 << c - '1');
					}
				}

				if (mask == 0)
				{
					goto ReturnFalse;
				}

				if ((mask & mask - 1) == 0)
				{
					// Compatibility:
					// If the cell has only one candidate left, we should treat this as given also.
					// This may ignore Sukaku checking, which causes a bug in logic.
					result.SetDigit(cell, TrailingZeroCount(mask));
					result.SetState(cell, CellState.Given);
				}
				else
				{
					for (var digit = 0; digit < 9; digit++)
					{
						result.SetExistence(cell, digit, (mask >> digit & 1) != 0);
					}
				}
			}
			else
			{
				// All conditions can't match.
				goto ReturnFalse;
			}
		}
		return true;

	ReturnFalse:
		result = Grid.Undefined;
		return false;
	}
}
