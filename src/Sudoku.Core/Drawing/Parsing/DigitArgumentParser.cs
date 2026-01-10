namespace Sudoku.Drawing.Parsing;

/// <summary>
/// Represents a digit argument parser.
/// </summary>
internal sealed class DigitArgumentParser : ArgumentParser
{
	/// <inheritdoc/>
	public override ReadOnlySpan<ViewNode> Parse(
		ReadOnlySpan<string> arguments,
		[AllowNull] ref readonly Grid grid,
		ColorDescriptor colorIdentifier,
		CoordinateParser coordinateParser
	)
	{
		if (arguments is not [[var digitCh and >= '1' and <= '9']])
		{
			throw new FormatException();
		}

		var result = new List<ViewNode>();
		var digit = digitCh - '1';
		foreach (var cell in grid.CandidatesMap[digit])
		{
			result.Add(new CandidateViewNode(colorIdentifier, cell * 9 + digit));
		}
		return result.AsSpan();
	}
}
