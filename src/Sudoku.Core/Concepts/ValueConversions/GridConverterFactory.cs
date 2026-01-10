namespace Sudoku.Concepts.ValueConversions;

using OptimizedConverterCreationTuple = (Predicate<ReadOnlySpan<char>> FastChecker, IGridConverter[] Converters, bool ContinueSukakuChecking);

/// <summary>
/// Provides with an entry to retrieve valid <see cref="IGridConverter"/> instances.
/// </summary>
/// <seealso cref="IGridConverter"/>
public static class GridConverterFactory
{
	/// <summary>
	/// Indicates optimized converters.
	/// </summary>
	private static readonly OptimizedConverterCreationTuple[] OptimizedConverters = [
		(static text => text.Length == 729, [new SukakuGridSingleLineConverter()], false),
		(static text => text.Contains('\t'), [new TabSeparatedGridConverter()], false),
		(
			static text => text.Contains("-+-", StringComparison.InvariantCulture),
			[new MultilineGridDefaultConverter(), new MultilineGridBlockLineRemovedConverter(), new PencilmarkGridConverter()],
			true
		)
	];

	/// <summary>
	/// Indicates a list of built-in <see cref="IGridConverter"/> instances.
	/// </summary>
	private static readonly IGridConverter[] BuiltInConverters =
		from type in typeof(GridConverterFactory).Assembly.GetTypes()
		where type.IsAssignableTo(typeof(IGridConverter)) && type.IsClass && !type.IsAbstract && type.HasParameterlessConstructor
		select (IGridConverter)Activator.CreateInstance(type)! into instance
		where instance.ParsingPriority != -1
		orderby instance.ParsingPriority descending
		select instance into instance
		select instance;

	/// <summary>
	/// Indicates the table of format and creator.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	private static readonly (string?[] FormatChecker, Func<IGridConverter> Creator)[] ConvertersLookup = [
		([null, "."], static () => new SusserGridDefaultConverter()),
		(["0"], static () => new SusserGridDefaultConverter { Placeholder = '0' }),
		(["0+", "+0"], static () => new SusserGridDefaultConverter { Placeholder = '0', WithModifiables = true }),
		(["+", ".+", "+."], static () => new SusserGridDefaultConverter { WithModifiables = true }),
		(["+:", "+.:", ".+:", "#", "#."], static () => new SusserGridDefaultConverter{ WithCandidates = true, WithModifiables = true }),
		(["^+:", "^:+", "^.+:", "^#", "^#."], static () => new SusserGridDefaultConverter { WithCandidates = true, WithModifiables = true, NegateEliminationsTripletRule = true }),
		(["0+:", "+0:", "#0"], static () => new SusserGridDefaultConverter { WithCandidates = true, WithModifiables = true, Placeholder = '0' }),
		(["^0+:", "^+0:", "^#0"], static () => new SusserGridDefaultConverter { WithCandidates = true, WithModifiables = true, Placeholder = '0', NegateEliminationsTripletRule = true }),
		([":", ".:"], static () => new SusserGridDefaultConverter { WithCandidates = true, OnlyEliminations = true }),
		(["0:"], static () => new SusserGridDefaultConverter { WithCandidates = true, OnlyEliminations = true, Placeholder = '0' }),
		(["!", ".!", "!."], static () => new SusserGridDefaultConverter { TreatValueAsGiven = true, WithModifiables = true }),
		(["0!", "!0"], static () => new SusserGridDefaultConverter { TreatValueAsGiven = true, WithModifiables = true, Placeholder = '0' }),
		([".!:", "!.:"], static () => new SusserGridDefaultConverter { WithCandidates = true, WithModifiables = true, TreatValueAsGiven = true }),
		(["^.!:", "^!.:"], static () => new SusserGridDefaultConverter { WithCandidates = true, TreatValueAsGiven = true, NegateEliminationsTripletRule = true }),
		(["0!:", "!0:"], static () => new SusserGridDefaultConverter { WithCandidates = true, WithModifiables = true, TreatValueAsGiven = true, Placeholder = '0' }),
		(["^0!:", "^!0:"], static () => new SusserGridDefaultConverter { WithCandidates = true, WithModifiables = true, TreatValueAsGiven = true, NegateEliminationsTripletRule = true, Placeholder = '0' }),
		([".*", "*."], static () => new SusserGridShortenedConverter()),
		(["0*", "*0"], static () => new SusserGridShortenedConverter { Placeholder = '0' }),
		(["@", "@."], static () => new MultilineGridDefaultConverter()),
		(["@*", "@.*", "@*."], static () => new MultilineGridDefaultConverter { SubtleGridLines = false }),
		(["@0"], static () => new MultilineGridDefaultConverter { Placeholder = '0' }),
		(["@0!", "@!0"], static () => new MultilineGridDefaultConverter { Placeholder = '0', TreatValueAsGiven = true }),
		(["@0*", "@*0"], static () => new MultilineGridDefaultConverter { Placeholder = '0', SubtleGridLines = false }),
		(["@!", "@.!", "@!."], static () => new MultilineGridDefaultConverter { TreatValueAsGiven = true }),
		(["@!*", "@*!"], static () => new MultilineGridDefaultConverter { TreatValueAsGiven = true, SubtleGridLines = false }),
		(["@:"], static () => new PencilmarkGridConverter()),
		(["@*:", "@:*"], static () => new PencilmarkGridConverter { SubtleGridLines = false }),
		(["@:!", "@!:"], static () => new PencilmarkGridConverter { TreatValueAsGiven = true }),
		(["@!*:", "@*!:", "@!:*", "@*:!", "@:!*", "@:*!"], static () => new PencilmarkGridConverter { TreatValueAsGiven = true, SubtleGridLines = false }),
		(["~."], static () => new SukakuGridSingleLineConverter()),
		(["~", "~0"], static () => new SukakuGridSingleLineConverter { Placeholder = '0' }),
		(["@~", "~@", "@~.", "@.~", "~@.", "~.@"], static () => new SukakuGridMultilineConverter()),
		(["@~0", "@0~", "~@0", "~0@"], static () => new SukakuGridMultilineConverter { Placeholder = '0' })
	];


	/// <summary>
	/// Represents instance with invariant culture.
	/// </summary>
	public static IGridConverter InvariantCultureInstance => new SusserGridDefaultConverter();


	/// <summary>
	/// The entry point to invoke <see cref="IValueConverter{T}.TryFormat(ref readonly T, IFormatProvider?, out string?)"/>
	/// method on all built-in converters.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="formatProvider">The format provider.</param>
	/// <param name="result">The result text.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	/// <seealso cref="IValueConverter{T}.TryFormat(ref readonly T, IFormatProvider?, out string?)"/>
	public static bool TryFormat(ref readonly Grid grid, IFormatProvider? formatProvider, [NotNullWhen(true)] out string? result)
	{
		if (grid.IsEmpty)
		{
			result = $"<{nameof(Grid.Empty)}>";
			return true;
		}
		if (grid.IsUndefined)
		{
			result = $"<{nameof(Grid.Undefined)}>";
			return true;
		}

		foreach (var converter in BuiltInConverters)
		{
			if (converter.TryFormat(in grid, formatProvider, out result))
			{
				return true;
			}
		}
		result = null;
		return false;
	}

	/// <summary>
	/// The entry point to invoke <see cref="IValueConverter{T}.TryParse(ReadOnlySpan{char}, IFormatProvider?, out T)"/> method
	/// on all built-in converters, to get correct result parsed.
	/// </summary>
	/// <param name="s">The original string.</param>
	/// <param name="provider">The provider.</param>
	/// <param name="result">The result grid parsed.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	/// <seealso cref="IValueConverter{T}.TryParse(ReadOnlySpan{char}, IFormatProvider?, out T)"/>
	public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Grid result)
	{
		if (s.IsEmpty)
		{
			result = Grid.Undefined;
			return false;
		}

		// Optimized check.
		foreach (var (canOptimizeChecker, converters, continueSukakuChecking) in OptimizedConverters)
		{
			if (canOptimizeChecker(s))
			{
				foreach (var converter in converters)
				{
					if (converter.TryParse(s, provider, out result))
					{
						if (!continueSukakuChecking)
						{
							return true;
						}
						goto DoTransformationIfWorth;
					}
				}
			}
		}

		// Traverse the whole converter collection.
		foreach (var converter in BuiltInConverters)
		{
			if (converter.TryParse(s, provider, out result))
			{
				goto DoTransformationIfWorth;
			}
		}
		result = Grid.Undefined;
		return false;

	DoTransformationIfWorth:
#if TRANSFORM_LESS_GIVEN_STANDARD_SUDOKU_PUZZLES_TO_SUKAKU
		// Here need an extra check. Sukaku puzzles can be output as a normal pencilmark grid format.
		// We should check whether the puzzle is a Sukaku in fact or not.
		// This is a bug fix for pencilmark grid parser, which cannot determine whether a puzzle is a Sukaku.
		// I define that a Sukaku must contain 0 given cells, meaning all values should be candidates or modifiable values.
		// If so, we should treat it as a Sukaku instead of a standard sudoku puzzle.
		if (result.GivenCellsCount < 17)
		{
			reduceGivenCells(ref grid);
			result.AddSukakuHeader();
		}
#endif
		return true;


#if TRANSFORM_LESS_GIVEN_STANDARD_SUDOKU_PUZZLES_TO_SUKAKU
		static void reduceGivenCells(ref Grid grid)
		{
			foreach (ref var mask in grid)
			{
				if (MaskToCellState(mask) != CellState.Empty)
				{
					mask = (Mask)(Grid.EmptyMask | mask & Grid.MaxCandidatesMask);
				}
			}
		}
#endif
	}


	/// <summary>
	/// Gets the <see cref="IGridConverter"/> of <see cref="Grid"/> associated with the specified <see cref="CultureInfo"/>.
	/// </summary>
	/// <param name="culture">The <see cref="CultureInfo"/> instance.</param>
	/// <returns>
	/// The <see cref="IGridConverter"/> of <see cref="Grid"/> associated with the specified <see cref="CultureInfo"/>.
	/// </returns>
	public static IGridConverter GetInstance(CultureInfo? culture)
		=> GetInstance(culture switch { { IsEnglish: true } => "@:", { IsChinese: true } => ".", _ => "#" })!;

	/// <summary>
	/// Creates a <see cref="IGridConverter"/> instance that holds the specified format.
	/// </summary>
	/// <param name="format">The format.</param>
	/// <returns>A valid <see cref="IGridConverter"/> instance.</returns>
	public static IGridConverter? GetInstance(string? format)
	{
		var p = Array.FindIndex(ConvertersLookup, pair => Array.IndexOf(pair.FormatChecker, format) != -1);
		return p == -1 ? null : ConvertersLookup[p].Creator();
	}
}
