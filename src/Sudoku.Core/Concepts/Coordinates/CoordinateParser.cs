namespace Sudoku.Concepts.Coordinates;

/// <summary>
/// Represents for a parser instance that parses a <see cref="string"/> text,
/// converting into a valid instance that can be represented as a sudoku concept.
/// </summary>
public abstract record CoordinateParser :
	ICoordinateProvider<CoordinateParser>,
	ICellParsable,
	ICandidateParsable,
	IHouseParsable,
	IConclusionParsable,
	IDigitParsable,
	ISegmentParsable,
	IChuteParsable,
	IConjuagtePairParsable
{
	/// <inheritdoc/>
	public abstract Func<string, CellMap> CellParser { get; }

	/// <inheritdoc/>
	public abstract Func<string, CandidateMap> CandidateParser { get; }

	/// <inheritdoc/>
	public abstract Func<string, HouseMask> HouseParser { get; }

	/// <inheritdoc/>
	public abstract Func<string, ConclusionSet> ConclusionParser { get; }

	/// <inheritdoc/>
	public abstract Func<string, Mask> DigitParser { get; }

	/// <inheritdoc/>
	public abstract Func<string, SegmentCollection> SegmentParser { get; }

	/// <inheritdoc/>
	public abstract Func<string, ReadOnlySpan<Chute>> ChuteParser { get; }

	/// <inheritdoc/>
	public abstract Func<string, ReadOnlySpan<Conjugate>> ConjugateParser { get; }


	/// <inheritdoc/>
	public static CoordinateParser InvariantCulture => new RxCyParser();


	/// <inheritdoc/>
	public static CoordinateParser GetInstance(CultureInfo? culture)
		=> culture switch
		{
			{ IsChinese: true } => new K9Parser(),
			{ IsEnglish: true } => new RxCyParser(),
			_ => InvariantCulture
		};
}
