namespace SudokuStudio.Interaction;

/// <summary>
/// Provides with extension methods on <see cref="SudokuFormatFlags"/>.
/// </summary>
/// <seealso cref="SudokuFormatFlags"/>
internal static class SudokuFormatFlagsExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="SudokuFormatFlags"/>.
	/// </summary>
	extension(SudokuFormatFlags @this)
	{
		/// <summary>
		/// Try to get target <see cref="IGridConverter"/> instance of type <see cref="Grid"/>.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">Throws when the argument is not defined.</exception>
		public IGridConverter Converter
			=> @this switch
			{
				SudokuFormatFlags.InitialFormat => new SusserGridDefaultConverter(),
				SudokuFormatFlags.CurrentFormat => new SusserGridDefaultConverter { WithCandidates = true, WithModifiables = true },
				SudokuFormatFlags.CurrentFormatIgnoringValueKind => new SusserGridDefaultConverter
				{
					WithModifiables = true,
					WithCandidates = true,
					TreatValueAsGiven = true
				},
				SudokuFormatFlags.MultipleGridFormat => new MultilineGridBlockLineRemovedConverter(),
				SudokuFormatFlags.PencilMarkFormat => new PencilmarkGridConverter { SubtleGridLines = true },
				SudokuFormatFlags.SukakuFormat => new SukakuGridSingleLineConverter(),
				SudokuFormatFlags.ExcelFormat => new TabSeparatedGridConverter(),
				SudokuFormatFlags.OpenSudokuFormat => new OpenSudokuGridConverter(),
				_ => throw new ArgumentOutOfRangeException(nameof(@this))
			};
	}
}
