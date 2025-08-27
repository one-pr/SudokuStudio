namespace System.Text;

/// <summary>
/// Represents a printer that can print table grid and return as a string.
/// </summary>
public static class TableGridBuilder
{
	/// <summary>
	/// Builds a table as a string with optional borders, row separators, and customizable border characters.
	/// Handles alignment with proper width calculation for wide characters (CJK, full-width symbols).
	/// </summary>
	/// <param name="table">List of string arrays, where each array represents a row.</param>
	/// <param name="printBorders">Whether to print outer table borders.</param>
	/// <param name="printRowSeparators">Whether to print separators between every row.</param>
	/// <param name="vertical">Vertical border character(s), e.g., "|" or "||".</param>
	/// <param name="horizontal">Horizontal border character(s), e.g., "-" or "=".</param>
	/// <param name="corner">Corner character(s), e.g., "+" or "#".</param>
	/// <returns>The formatted table as a string.</returns>
	public static string BuildTable(
		ReadOnlySpan<string[]> table,
		bool printBorders = true,
		bool printRowSeparators = false,
		string vertical = "|",
		string horizontal = "-",
		string corner = "+"
	)
	{
		var sbResult = new StringBuilder();

		if (table.Length == 0)
		{
			sbResult.AppendLine("(empty table)");
			return sbResult.ToString();
		}

		// Calculate max width for each column considering wide characters.
		var columnCount = table.Max(static row => row.Length);
		var columnWidths = new int[columnCount];

		foreach (var row in table)
		{
			for (var i = 0; i < row.Length; i++)
			{
				var width = GetDisplayWidth(row[i]);
				if (width > columnWidths[i])
				{
					columnWidths[i] = width;
				}
			}
		}

		// Precompute border line considering custom characters.
		var borderLine = string.Empty;
		if (printBorders)
		{
			var sbBorder = new StringBuilder();
			sbBorder.Append(corner);
			foreach (var width in columnWidths)
			{
				sbBorder.Append(new string(horizontal[0], width + vertical.Length * 2)); // Adjust for vertical padding.
				sbBorder.Append(corner);
			}
			borderLine = sbBorder.ToString();
		}

		// Append top border.
		if (printBorders)
		{
			sbResult.AppendLine(borderLine);
		}

		// Append table rows.
		for (var r = 0; r < table.Length; r++)
		{
			var row = table[r];
			var line = new StringBuilder();
			if (printBorders)
			{
				line.Append(vertical);
			}

			for (var c = 0; c < columnCount; c++)
			{
				var cell = c < row.Length ? row[c] : string.Empty;
				var pad = columnWidths[c] - GetDisplayWidth(cell);

				line.Append(' ').Append(cell).Append(new string(' ', pad)).Append(' ');
				if (printBorders)
				{
					line.Append(vertical);
				}
			}

			sbResult.AppendLine(line.ToString());

			// Append row separator after each row if enabled.
			if (printRowSeparators && r < table.Length - 1 && printBorders)
			{
				sbResult.AppendLine(borderLine);
			}
		}

		// Append bottom border.
		if (printBorders)
		{
			sbResult.AppendLine(borderLine);
		}

		return sbResult.ToString();
	}

	/// <summary>
	/// Calculates the display width of a string, counting wide characters (CJK, full-width) as 2 units.
	/// </summary>
	private static int GetDisplayWidth(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return 0;
		}

		var width = 0;
		foreach (var ch in text)
		{
			width += IsWideChar(ch) ? 2 : 1;
		}
		return width;
	}

	/// <summary>
	/// Determines whether a character is wide (CJK or full-width symbol).
	/// </summary>
	private static bool IsWideChar(char ch)
		=> ch
		is >= (char)0x1100
		and (
			<= (char)0x115F or (char)0x2329 or (char)0x232A
			or >= (char)0x2E80 and <= (char)0xA4CF
			or >= (char)0xAC00 and <= (char)0xD7A3
			or >= (char)0xF900 and <= (char)0xFAFF
			or >= (char)0xFE10 and <= (char)0xFE19
			or >= (char)0xFE30 and <= (char)0xFE6F
			or >= (char)0xFF00 and <= (char)0xFF60
			or >= (char)0xFFE0 and <= (char)0xFFE6
		);
}
