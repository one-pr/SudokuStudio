namespace Sudoku.Analytics;

public partial class Hub
{
	/// <summary>
	/// Represents a type that supports excluders marking inside a direct technique.
	/// </summary>
	public static partial class Excluder
	{
		[GeneratedRegex("""(Row|Column)HiddenSingle\d{3}""", RegexOptions.Compiled)]
		private static partial Regex SubtypeTextPattern { get; }


		/// <summary>
		/// Indicates whether the solving steps contains at least one <see cref="Step"/>
		/// using hidden singles in line, with block excluders.
		/// </summary>
		public static bool HasBlockExcluders(AnalysisResult @this)
		{
			foreach (var step in @this.StepsSpan)
			{
				if (step is not HiddenSingleStep { Subtype: var subtype })
				{
					continue;
				}

				var text = subtype.ToString();
				if (!SubtypeTextPattern.IsMatch(text))
				{
					continue;
				}

				if (text[^3] != '0')
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Get all <see cref="Cell"/> offsets that represents as excluders.
		/// </summary>
		/// <param name="grid">The grid.</param>
		/// <param name="cell">The cell.</param>
		/// <param name="digit">The digit.</param>
		/// <param name="excluderHouses">The excluder houses.</param>
		/// <returns>A <see cref="CellMap"/> instance.</returns>
		public static CellMap GetNakedSingleExcluderCells(in Grid grid, Cell cell, Digit digit, out ReadOnlySpan<House> excluderHouses)
			=> [.. from node in GetNakedSingleExcluders(grid, cell, digit, out excluderHouses) select node.Cell];

		/// <summary>
		/// Try to create a list of <see cref="IconViewNode"/>s indicating the crosshatching base cells.
		/// </summary>
		/// <param name="grid">The grid.</param>
		/// <param name="digit">The digit.</param>
		/// <param name="house">The house.</param>
		/// <param name="cell">The cell.</param>
		/// <param name="chosenCells">The chosen cells.</param>
		/// <param name="excluderInfo">The excluder information.</param>
		/// <returns>A list of <see cref="IconViewNode"/> instances.</returns>
		public static ReadOnlySpan<IconViewNode> GetHiddenSingleExcluders(
			in Grid grid,
			Digit digit,
			House house,
			Cell cell,
			out CellMap chosenCells,
			out ExcluderInfo excluderInfo
		)
		{
			excluderInfo = ExcluderInfo.Create(grid, digit, house, cell.AsCellMap())!;
			if (excluderInfo is var (cc, covered, excluded))
			{
				chosenCells = cc;
				return (IconViewNode[])[
					.. from c in chosenCells select new CircleViewNode(ColorDescriptorAlias.Normal, c),
					..
					from c in covered
					let p = excluded.Contains(c) ? ColorDescriptorAlias.Auxiliary2 : ColorDescriptorAlias.Auxiliary1
					select (IconViewNode)(p == ColorDescriptorAlias.Auxiliary2 ? new TriangleViewNode(p, c) : new CrossViewNode(p, c))
				];
			}

			chosenCells = [];
			return [];
		}

		/// <summary>
		/// Get all <see cref="IconViewNode"/>s that represents as excluders.
		/// </summary>
		/// <param name="grid">The grid.</param>
		/// <param name="cell">The cell.</param>
		/// <param name="digit">The digit.</param>
		/// <param name="excluderHouses">The excluder houses.</param>
		/// <returns>A list of <see cref="IconViewNode"/> instances.</returns>
		public static ReadOnlySpan<IconViewNode> GetNakedSingleExcluders(in Grid grid, Cell cell, Digit digit, out ReadOnlySpan<House> excluderHouses)
		{
			var (block, row, column) = (
				HousesMap[cell.GetHouse(HouseType.Block)] & ~grid.EmptyCells,
				HousesMap[cell.GetHouse(HouseType.Row)] & ~grid.EmptyCells,
				HousesMap[cell.GetHouse(HouseType.Column)] & ~grid.EmptyCells
			);
			var (result, i) = (new IconViewNode[8], 0);
			excluderHouses = new House[8];
			var lastDigitsMask = (Mask)(Grid.MaxCandidatesMask & ~(1 << digit));
			foreach (var tempCell in Math.Max(block.Count, row.Count, column.Count) switch
			{
				var z when z == block.Count => block,
				var z when z == row.Count => row,
				_ => column
			})
			{
				var tempDigit = grid.GetDigit(tempCell);
				result[i] = new CircleViewNode(ColorDescriptorAlias.Normal, tempCell);
				Unsafe.AsRef(in excluderHouses[i]) = (cell.AsCellMap() + tempCell).FirstSharedHouse;
				i++;
				lastDigitsMask &= (Mask)~(1 << tempDigit);
			}
			foreach (var otherDigit in lastDigitsMask)
			{
				foreach (var otherCell in Peer.PeersMap[cell])
				{
					if (grid.GetDigit(otherCell) == otherDigit)
					{
						result[i] = new CircleViewNode(ColorDescriptorAlias.Normal, otherCell);
						Unsafe.AsRef(in excluderHouses[i]) = (cell.AsCellMap() + otherCell).FirstSharedHouse;
						i++;
						break;
					}
				}
			}

			excluderHouses = excluderHouses[..i];
			return result.AsReadOnlySpan()[..i];
		}

		/// <summary>
		/// Try to create a list of <see cref="IconViewNode"/>s indicating the crosshatching base cells.
		/// </summary>
		/// <param name="grid">The grid.</param>
		/// <param name="digit">The digit.</param>
		/// <param name="house">The house.</param>
		/// <param name="cells">The cells.</param>
		/// <returns>A list of <see cref="IconViewNode"/> instances.</returns>
		public static ReadOnlySpan<IconViewNode> GetLockedCandidatesExcluders(in Grid grid, Digit digit, House house, in CellMap cells)
		{
			var info = ExcluderInfo.Create(grid, digit, house, cells);
			if (info is not var (combination, emptyCellsShouldBeCovered, emptyCellsNotNeedToBeCovered))
			{
				return [];
			}

			var result = new List<IconViewNode>();
			foreach (var c in combination)
			{
				result.Add(new CircleViewNode(ColorDescriptorAlias.Normal, c));
			}
			foreach (var c in emptyCellsShouldBeCovered)
			{
				var p = emptyCellsNotNeedToBeCovered.Contains(c) ? ColorDescriptorAlias.Auxiliary2 : ColorDescriptorAlias.Auxiliary1;
				result.Add(p == ColorDescriptorAlias.Auxiliary2 ? new TriangleViewNode(p, c) : new CrossViewNode(p, c));
			}
			return result.AsSpan();
		}

		/// <summary>
		/// Try to create a list of <see cref="IconViewNode"/>s indicating the crosshatching base cells.
		/// </summary>
		/// <param name="grid">The grid.</param>
		/// <param name="digit">The digit.</param>
		/// <param name="house">The house.</param>
		/// <param name="cells">The cells.</param>
		/// <returns>A list of <see cref="IconViewNode"/> instances.</returns>
		public static ReadOnlySpan<IconViewNode> GetSubsetExcluders(in Grid grid, Digit digit, House house, in CellMap cells)
		{
			var info = ExcluderInfo.Create(grid, digit, house, cells);
			if (info is not var (combination, emptyCellsShouldBeCovered, emptyCellsNotNeedToBeCovered))
			{
				return [];
			}

			var result = new List<IconViewNode>();
			foreach (var c in combination)
			{
				result.Add(new CircleViewNode(ColorDescriptorAlias.Normal, c));
			}
			foreach (var c in emptyCellsShouldBeCovered)
			{
				var p = emptyCellsNotNeedToBeCovered.Contains(c) ? ColorDescriptorAlias.Auxiliary2 : ColorDescriptorAlias.Auxiliary1;
				result.Add(p == ColorDescriptorAlias.Auxiliary2 ? new TriangleViewNode(p, c) : new CrossViewNode(p, c));
			}
			return result.AsSpan();
		}
	}
}
