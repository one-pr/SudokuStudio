namespace Sudoku.Concepts.Supersymmetry;

/// <summary>
/// Provides spaces for a certain candidate.
/// </summary>
public static class CandidateSpaces
{
	/// <summary>
	/// Provides extension members on <see cref="Candidate"/>.
	/// </summary>
	extension(Candidate @this)
	{
		/// <summary>
		/// Indicates spaces of the current candidate.
		/// </summary>
		public ReadOnlySpan<Space> Spaces
		{
			get
			{
				var cell = @this / 9;
				var digit = @this % 9;
				return (Space[])[
					Space.RowColumn(cell / 9, cell % 9),
					Space.BlockDigit(cell.GetHouse(HouseType.Block), digit),
					Space.RowDigit(cell.GetHouse(HouseType.Row) - 9, digit),
					Space.ColumnDigit(cell.GetHouse(HouseType.Column) - 18, digit)
				];
			}
		}
	}
}
