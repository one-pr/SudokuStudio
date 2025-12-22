namespace Sudoku.Concepts.Coordinates;

/// <summary>
/// Represents for a parser instance that parses a <see cref="string"/> text,
/// converting into a valid instance that can be represented as a sudoku concept.
/// </summary>
public abstract record CoordinateParser :
	ICoordinateProvider<CoordinateParser>,
	ICellParser,
	ICandidateParser,
	IHouseParser,
	IConclusionParser,
	IDigitParser,
	IIntersectionParser,
	IChuteParser,
	IConjuagtePairParser
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
	public abstract Func<string, ReadOnlySpan<Miniline>> IntersectionParser { get; }

	/// <inheritdoc/>
	public abstract Func<string, ReadOnlySpan<Chute>> ChuteParser { get; }

	/// <inheritdoc/>
	public abstract Func<string, ReadOnlySpan<Conjugate>> ConjugateParser { get; }


	/// <inheritdoc/>
	public static CoordinateParser InvariantCultureInstance => new RxCyParser();


	/// <inheritdoc/>
	[return: NotNullIfNotNull(nameof(formatType))]
	public abstract object? GetFormat(Type? formatType);


	/// <inheritdoc/>
	public static CoordinateParser GetInstance(IFormatProvider? formatProvider)
		=> formatProvider switch
		{
			CultureInfo { Name: var name } when name.CultureNameEqual(SR.ChineseLanguage) => new K9Parser(),
			CoordinateParser c => c,
			_ => InvariantCultureInstance
		};
}
