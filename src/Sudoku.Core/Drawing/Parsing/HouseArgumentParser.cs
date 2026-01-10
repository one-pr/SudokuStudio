namespace Sudoku.Drawing.Parsing;

/// <summary>
/// Represents a house argument parser.
/// </summary>
internal sealed partial class HouseArgumentParser : ArgumentParser
{
	/// <inheritdoc/>
	public override ReadOnlySpan<ViewNode> Parse(
		ReadOnlySpan<string> arguments,
		[AllowNull] ref readonly Grid grid,
		ColorDescriptor colorIdentifier,
		CoordinateParser coordinateParser
	)
	{
		if (arguments.All(static argument => HouseWithDigitPattern.IsMatch(argument)
			|| argument.StartsWith('+') || argument.StartsWith('-')))
		{
			// Syntax:
			// 'house' ' ' color_identifier ' ' houses '(' digits ')' ('+' cells)* ('-' cells)*

			if (Unsafe.IsNullRef(in grid))
			{
				throw new FormatException();
			}

			var (includingCells, excludingCells) = (CellMap.Empty, CellMap.Empty);
			var includingExcludingClauseIndices = new List<int>();
			foreach (var (i, argument) in arguments.Index())
			{
				if (argument is ['+', .. var possibleIncludingCellsString])
				{
					includingCells |= coordinateParser.CellParser(possibleIncludingCellsString);
					includingExcludingClauseIndices.Add(i);
				}
				if (argument is ['-', .. var possibleExcludingCellsString])
				{
					excludingCells |= coordinateParser.CellParser(possibleExcludingCellsString);
					includingExcludingClauseIndices.Add(i);
				}
			}

			var fullCells = (includingCells ? includingCells : CellMap.Full) & ~excludingCells;
			var candidateViewNodes = new List<ViewNode>();
			var candidatesMap = grid.CandidatesMap;
			foreach (var (i, argument) in arguments.Index())
			{
				if (includingExcludingClauseIndices.Contains(i))
				{
					// Skip for including and excluding clauses.
					continue;
				}

				var indexOfOpenBrace = argument.IndexOf('(');
				var houses = coordinateParser.HouseParser(argument[..indexOfOpenBrace]);
				for (var j = indexOfOpenBrace + 1; j < argument.IndexOf(')'); j++)
				{
					if (argument[j] - '1' is not (var digit and >= 0 and < 9))
					{
						throw new FormatException();
					}

					foreach (var house in houses)
					{
						foreach (var cell in HousesMap[house] & candidatesMap[digit])
						{
							if (fullCells.Contains(cell))
							{
								candidateViewNodes.Add(new CandidateViewNode(colorIdentifier, cell * 9 + digit));
							}
						}
					}
				}
			}
			return candidateViewNodes.AsSpan();
		}
		else
		{
			// Syntax:
			// 'house' ' ' color_identifier ' ' houses

			var houses = arguments.Aggregate(0, (interim, next) => interim | coordinateParser.HouseParser(next));
			return from house in houses select new HouseViewNode(colorIdentifier, house);
		}
	}


	[GeneratedRegex(""".+\([1-9]+\)""", RegexOptions.Compiled)]
	private static partial Regex HouseWithDigitPattern { get; }
}
